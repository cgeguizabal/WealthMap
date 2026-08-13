# WealthMap — Project Guide

This document exists to teach, not just to describe. It walks through what the system does,
how it is built, and — most importantly — *why* each decision went the way it did. Where a
choice had a real alternative, the alternative is named and the trade-off stated.

Read it top to bottom once. After that it works as a reference.

---

## Table of contents

1. [What WealthMap is](#1-what-wealthmap-is)
2. [The architecture, and why layers exist at all](#2-the-architecture-and-why-layers-exist-at-all)
3. [The patterns, one at a time](#3-the-patterns-one-at-a-time)
4. [The modules](#4-the-modules) — including the [payments ledger](#413-payments-ledger)
5. [The database schema](#5-the-database-schema)
6. [Decisions and their reasoning](#6-decisions-and-their-reasoning)
7. [Running, testing, migrating](#7-running-testing-migrating)
8. [Known limitations](#8-known-limitations)
9. [Glossary](#9-glossary)

---

## 1. What WealthMap is

A personal finance API. It answers six questions:

- How much money do I have, and where?
- How much do I owe, and how much credit is left?
- How much is safe to spend this month?
- What is due, and when?
- Am I on track for my goals?
- What happened last month?

The domain has a spine: **money always lives in an account**. Every balance change writes an
immutable `AccountMovement`, so the balance is never a number you have to trust — it is a number
you can reconstruct. Credit cards, debts, purchases, installments and goals all hang off that spine.

Almost everything is written in response to a request. The one exception is salary, which posts
itself on each payday (§4.5) — the only place the system moves money without being asked.

---

## 2. The architecture, and why layers exist at all

Four projects, dependencies pointing strictly inward:

```
Api  ──►  Infrastructure  ──►  Application  ──►  Domain
 │                                  │
 └──────────────────────────────────┘
        (Api also references Application)
```

| Project | Contains | Depends on |
|---|---|---|
| `WealthMap.Domain` | Entities, value objects, enums, domain services, `DomainException` | **nothing** |
| `WealthMap.Application` | CQRS handlers, DTOs, validators, repository *interfaces* | Domain |
| `WealthMap.Infrastructure` | EF Core, repositories, JWT, hashing, PDF rendering | Application, Domain |
| `WealthMap.Api` | Controllers, middleware, DI composition | Application, Infrastructure |

### The dependency rule

Arrows point inward, never outward. Domain knows nothing about EF Core, HTTP, or JSON.
Application knows *what* a repository does but not *how* — it holds `IAccountRepository`, while
`AccountRepository` with its `DbSet` lives in Infrastructure.

**Why bother?** Three concrete payoffs, not architectural theatre:

1. **The rules are readable in one place.** `Account.Withdraw` says money cannot go negative and a
   blocked savings account refuses withdrawals. That is the business rule, in a file with no
   database or HTTP noise around it.
2. **Infrastructure is replaceable.** Swapping Neon for another Postgres host, or QuestPDF for a
   different renderer, touches one project. `IPdfReportGenerator` exists precisely so the report's
   *content* (Application) and its *appearance* (Infrastructure) can change independently.
3. **The compiler enforces it.** There is no `using` you can add in Domain to reach EF Core,
   because the project reference does not exist. Discipline you cannot forget is better than
   discipline you must remember.

The rule the compiler *cannot* enforce, and you must: **Api never touches Domain entities
directly.** Controllers speak DTOs. If a controller returned an `Account`, the JSON shape would be
welded to the domain model, and changing a private setter would break clients.

---

## 3. The patterns, one at a time

### 3.1 Rich domain entities

An anemic model is a bag of public setters with the rules living in "service" classes. This project
does the opposite: **entities protect their own invariants**.

```csharp
public void Withdraw(Money amount)
{
    EnsurePositive(amount);

    if (IsBlockedForSaving)
        throw new DomainException($"Account '{Name}' is blocked for saving. Unblock it before taking money out.");

    if (amount > Balance)
        throw new DomainException($"Insufficient funds in '{Name}'. Available: {Balance}, requested: {amount}.");

    Balance = Balance - amount;
    Touch();
}
```

Three things to notice:

- `Balance` has a **private setter**. Nothing outside the entity can assign it. The only way money
  leaves an account is `Withdraw`, which means the overdraft check cannot be bypassed.
- The constructor validates too, so an `Account` cannot exist in an invalid state — not even briefly.
- Failures throw `DomainException`, which the middleware maps to **400**. A broken business rule is
  a bad request, not a server error.

Every entity follows this shape, plus a private parameterless constructor with `= null!;`
assignments that exists solely for EF Core's materializer (and silences CS8618).

### 3.2 The `Money` value object

`Money` is a `readonly record struct` of amount + currency:

```csharp
public static Money operator +(Money a, Money b)
{
    EnsureSameCurrency(a, b);
    return new Money(a.Amount + b.Amount, a.Currency);
}
```

Adding USD to MXN throws. That single guard is why a whole category of bug cannot occur here: you
cannot accidentally sum a peso card into a dollar total. The dashboard's currency filtering exists
*because* `Money` would have thrown otherwise — the type forced an honest design.

Rounding happens once, in the constructor — 2 decimals, `MidpointRounding.AwayFromZero` — so every
amount in the system is already at cent precision.

**Away from zero, not to even.** Banker's rounding avoids drift across many roundings and was the
original choice, but it sends 418.525 to 418.52, and nobody checking a figure against a payslip
expects that. Every other rounding site in the system now says `AwayFromZero` explicitly, including
three that were silently inheriting `decimal.Round`'s default — goal progress, the debt ratio and
category share percentages.

EF Core maps it with `ComplexProperty`, producing two real columns (`balance`, `currency`) — not a
JSON blob. Values stay queryable and check-constrainable.

### 3.3 CQRS with a hand-built mediator

Every operation is a **request object** plus a **handler**:

```
Features/Accounts/Commands/UpdateAccount/
    UpdateAccountCommand.cs      ← the request (a record)
    UpdateAccountHandler.cs      ← the behaviour
    UpdateAccountValidator.cs    ← the input rules
```

Commands change state and return the updated DTO; queries read and return DTOs. Both flow through
`ISender`.

**How the mediator actually works.** `Sender.Send` is ~30 lines of reflection:

1. Build the closed handler type: `IRequestHandler<UpdateAccountCommand, AccountDto>`.
2. Resolve it from DI. Missing registration throws immediately with a clear message.
3. Resolve all `IPipelineBehavior<,>` for that pair and **reverse** them.
4. Fold them into a chain of delegates, innermost being the handler itself.
5. Invoke the outermost delegate.

The reverse is the subtle part. Behaviors are folded outward-in, so reversing the list makes the
*first* registered behavior the *outermost* wrapper — the one that sees the request first and the
response last. Middleware semantics, exactly as you would expect.

Handlers and validators are registered by assembly scanning in `Application/DependencyInjection.cs`,
so a new feature folder wires itself up with no DI edits.

**Why not MediatR?** MediatR is excellent and in a production shop you would probably use it. It was
deliberately avoided here because the whole point of the exercise is to understand the machinery.
Once you have written `Send`, MediatR stops being magic and becomes "the same idea, better tested."
The cost is ~60 lines to maintain; the benefit is that nothing in the request pipeline is opaque.

### 3.4 Pipeline behaviors

`ValidationBehavior<TRequest, TResponse>` sits in front of every handler. It resolves every
`IValidator<TRequest>`, runs them, and if any fail, groups the errors by property name and throws
`ValidationException` — which the middleware renders as a 400 with a field-keyed error object:

```json
{ "title": "Validation failed", "status": 400,
  "errors": { "PageSize": ["Page size must be between 1 and 100."] } }
```

Handlers therefore never validate input shape. By the time a handler runs, the request is
structurally sound and it can concentrate on business logic.

This is where the **validation vs. domain rules** split lives, and it is worth being precise:

- **Validator** = is this request *well formed*? Amount positive, name non-empty, currency 3 letters.
  Answerable without touching the database.
- **Domain** = is this operation *legal right now*? Sufficient funds, card limit, deductions not
  exceeding gross. Requires state.

Some rules appear in both, intentionally. `amount > 0` is in the validator (so the user gets a clean
field error) and in the entity (so the rule holds even if a future caller skips the pipeline). The
entity is the source of truth; the validator is the good error message.

### 3.5 Repositories and the Unit of Work

`IRepository<T>` gives `GetByIdAsync`, `AddAsync`, `Update`, `Remove`. Per-aggregate interfaces add
the user-scoped queries:

```csharp
Task<Account?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default);
```

**Every** query is user-scoped. Not "most". The id alone is never enough to reach a row.

`IUnitOfWork` owns the transaction boundary:

```csharp
await _unitOfWork.ExecuteInTransactionAsync(async () =>
{
    from.Withdraw(amount);
    to.Deposit(amount);
    await _movements.AddAsync(outMovement, ct);
    await _movements.AddAsync(inMovement, ct);
}, ct);
```

One `BeginTransaction`, one `SaveChanges`, one `Commit`, rollback on any exception. A transfer that
fails halfway cannot leave money destroyed or duplicated. Every multi-entity write in the codebase —
transfers, card payments, debit purchases, installment payments, linked goal contributions — goes
through this.

Note what this buys you with rich entities: `from.Withdraw(amount)` may throw *inside* the
transaction, and the rollback is automatic. Domain rules and atomicity compose for free.

### 3.6 DTOs with static `FromEntity`

No AutoMapper. Every DTO exposes:

```csharp
public static AccountDto FromEntity(Account account) => new(
    account.Id, account.Name, /* … */ account.Balance.Amount, account.Balance.Currency, /* … */);
```

**Why not AutoMapper?** Convention-based mapping fails silently and at runtime: rename a property
and a field quietly becomes null, discovered in production. `FromEntity` fails at *compile* time,
is trivially greppable, and lets you flatten (`Balance.Amount` → `balance`) or compute
(`IsOwnedBy(currentUserId)` → `isMine`) without configuration. The cost is a few lines per DTO; the
benefit is that mapping is code you can read and the compiler checks.

### 3.7 Authentication and user scoping

Registration hashes the password with PBKDF2 (via ASP.NET Identity's hasher) and returns a JWT
carrying `sub` (user id), `email`, and `name`, signed HS256. `Program.cs` validates issuer,
audience, signing key and lifetime, with `ClockSkew = TimeSpan.Zero` so expiry means expiry.

Controllers read the caller's identity **only** from the token:

```csharp
var userId = User.GetUserId();   // from the JWT's sub claim
```

Never from the request body. If the user id were a body field, anyone could pass someone else's
id — the single most common way this class of app leaks data.

**"Not yours" returns 404, never 403.** A 403 confirms the resource exists, which leaks information:
an attacker enumerating GUIDs learns which ones are real. A 404 tells them nothing. From the
caller's perspective, other users' data does not merely refuse access — it does not exist.

### 3.8 Exceptions to HTTP

`ExceptionHandlingMiddleware` is the only place that decides status codes:

| Exception | Status | Body |
|---|---|---|
| `ValidationException` | 400 | field-keyed error dictionary |
| `DomainException` | 400 | the rule's message |
| `NotFoundException` | 404 | resource + id |
| anything else | 500 | generic message; details logged server-side |0.

Handlers never touch `IActionResult`, and controllers contain no try/catch. The last case matters
for security: an unexpected exception is logged in full but the client gets only "An unexpected
error occurred" — no stack traces, no SQL, no schema hints.

### 3.9 Store facts, compute conclusions

The most consequential principle in the codebase. **Facts are stored. Conclusions are computed.**

| Computed on read | Never a column |
|---|---|
| `AvailableCredit` | = limit − used |
| `NetMonthly`, `NetPerDeposit` | from gross and deductions |
| Goal `ProgressPercentage`, `Status`, `RequiredMonthlyContribution` | from amounts, deadline, elapsed time |
| Installment `RemainingBalance`, `RemainingMonths`, `EndDate` | from unpaid child rows |
| Dashboard totals, `SafeToSpend`, `DebtRatio` | from the loaded snapshot |
| Report opening/closing balances | today's balance rewound through movements |

A stored `available_credit` column can disagree with `limit − used` after one missed update, and
then you have two numbers and no way to know which is right. A computed property cannot drift.

The cost is CPU on read and the inability to index on a derived value. At personal-finance data
volumes that cost is irrelevant, and correctness is not.

---

## 4. The modules

### 4.1 Users & authentication
`POST /api/v1/auth/register`, `POST /api/v1/auth/login`. Registration stores email (normalized
lowercase), PBKDF2 hash, name, country, currency. The profile currency is the reporting currency
used by the dashboard and monthly report.

### 4.2 Accounts
`GET|POST /api/v1/accounts`, `GET|PUT|DELETE /{id}`, `POST /{id}/block`, `POST /{id}/unblock`.

Checking or Savings. A savings account can be **blocked for saving**: deposits still work,
withdrawals throw. It is a commitment device, not a security feature.

Balance is not editable through `PUT` — only movements change money. Type and currency are immutable
after creation, because changing either would invalidate the entire movement history.

**`DELETE` archives; it does not delete** (§6.11). `IsArchived` takes the account out of every list,
dropdown and total, while its movements — and the purchases, payments and jobs pointing at it — stay
exactly as they were. `GET /{id}` and `GET /{id}/movements` still resolve afterwards so links from
history keep working, but `ExistsForUserAsync` excludes archived rows, so nothing new can be pointed
at one: creating a job or income against an archived account is a 404.

### 4.3 Movements & transactions
`POST /{id}/deposit`, `POST /{id}/withdraw`, `POST /accounts/transfer`, `GET /{id}/movements`.

Every balance change writes an immutable `AccountMovement` recording amount, type, description,
`BalanceAfter`, and timestamp. `BalanceAfter` is a stored *fact* (what the balance was at that
instant), not a conclusion — which is exactly why the monthly report can rewind history.

- **Deposit** accepts only `Deposit` (2) or `Bonus` (3). `SalaryDeposit` and `TransferIn` are
  system-generated; letting a client claim a salary deposit would corrupt income reporting.
- **Withdraw** is always `AtmWithdrawal` with optional location — it is the only manual outbound
  type, because cash leaving the system is the only manual way money exits.
- **Transfer** writes a paired `TransferOut` + `TransferIn`, each with `RelatedEntityId` pointing at
  the other account, atomically.

Request bodies carry no currency: the amount is denominated in the account's own currency, so a
mismatch is impossible by construction.

### 4.4 Credit cards
`GET|POST /api/v1/credit-cards`, `GET|PUT|DELETE /{id}`, `PUT /{id}/limit`, `POST /{id}/payments`.

`AvailableCredit = CreditLimit − UsedCredit`, computed. `Charge` refuses to exceed the limit;
`RegisterPayment` refuses to exceed the balance owed; `UpdateCreditLimit` refuses to drop below
current debt. There is no public charge endpoint — purchases and installments call it internally.

**Payments use the source-selection pattern** (§6.5).

**`DELETE` archives, as with accounts** (§6.11). Purchases, installment plans and payments survive.
A card with an outstanding balance can still be archived, deliberately: archiving is not a way to
settle debt, and the debt itself remains visible in the payment and purchase history. What changes is
that the balance stops counting toward the dashboard credit totals.

### 4.5 Jobs & salary
`GET|POST /api/v1/jobs`, `GET|PUT|DELETE /{id}`, nested `POST|PUT|DELETE /{jobId}/deductions/{id}`,
and `/api/v1/additional-incomes` CRUD.

`Job` is an **aggregate root**: payment days and deductions are child rows reachable only through
job methods, so every mutation re-checks the invariants (1–3 distinct payment days; deductions can
never push net below zero).

```
NetMonthly    = gross − Σ(fixed) − gross × Σ(percentage) / 100
NetPerDeposit = NetMonthly ÷ number of payment days
```

**Deductions are monthly, and each payday carries an equal share of them.** A 10% deduction on a job
paid twice a month takes 5% at each payday — not 10% at both. That falls out of the formula above
rather than being special-cased, and it is the question people ask first.

`NetPerDeposit` is the nominal share and is what the UI shows. `NetForPayday(date)` is what actually
gets paid: it settles the month's rounding remainder on the month's **last** payday, because an even
split does not add up. 1000 across three paydays rounds to 333.33 each and pays 999.99 — a cent lost
every month, forever, once posting is automatic.

Month-end clamping lives in `PaymentSchedule.ClampToMonth` as a pure function: the "30th" in
February becomes the 28th or 29th. `Math.Min(day, DateTime.DaysInMonth(year, month))` — one line,
and the reason `NextPaymentDates` is trivially testable. `PaydaysInMonth` is the single source of a
month's paydays, shared by the split and the posting schedule so the two cannot disagree about how
many paydays a month has. Two payment days that clamp onto the same date — 30 and 31 in June — are
one payday, not two, in both.

`Deduction.Value` is a plain `decimal`, not `Money`, with a Fixed/Percentage discriminator. A
percentage is not money; forcing it into `Money` would mean inventing a currency for "20%".

#### Automatic salary posting

`SalaryPostingService` credits the deposit account on each payment day. `SalaryPostingRunner`, a
hosted service, runs it on startup and every 24 hours.

It is written as **catch-up, not a clock tick** (§6.12): each run asks which paydays are *due and
unpaid* and settles all of them, so the app being off on payday delays the deposit rather than
losing it. A `salary_deposits` row records each settled payday, and a unique index on
`(job_id, scheduled_date)` makes exactly-once a database guarantee rather than a hope — two
instances can pass an "already posted?" check at the same instant.

`Job.SalaryPostingStartsOn` stops a new job backfilling months of salary into an account and
inventing a balance that never existed. Salary is **held, not dropped**, when it cannot be paid —
archived or missing deposit account, or a job currency the account does not hold, which is never
converted. Fixing the cause pays the held paydays on the next run.

`IJobRepository.GetAllForPostingAsync` is the one query in the system that is not user-scoped, and
says so in its own doc comment: the runner has no signed-in user and has to pay everybody.

### 4.6 Stores
`GET|POST /api/v1/stores`, `PUT /{id}`.

The one deliberate exception to user scoping: a **shared catalog**. Everyone reads all stores;
only the creator may edit theirs (and a non-owner's `PUT` returns 404, consistent with §3.7).
`created_by_user_id` is nullable with `ON DELETE SET NULL`, so a catalog entry outlives its creator.
No delete endpoint — purchases reference stores, and deleting one would orphan history.

The DTO exposes `isMine` rather than the creator's id, so other users' ids never leak.

### 4.7 Purchases
`GET|POST /api/v1/purchases`, `GET /{id}`. Filters: `?year=&month=&category=&page=&pageSize=`.

The constructor enforces method-specific invariants: debit requires an account and forbids a card,
credit the inverse, cash forbids both. Then:

- **Debit** → `account.Withdraw` + `Purchase` movement + purchase row, one transaction.
- **Credit** → `card.Charge` + purchase row, one transaction.
- **Cash** → purchase row only; cash is outside tracking.

Currency comes from the instrument for debit/credit. Cash requires an explicit currency — there is
no instrument to inherit it from.

`?creditCardId=` narrows to one card's charges, which is what the card detail screen uses.

`PurchaseDto` carries `StoreName` beside `StoreId`, so a list can name where a purchase happened
without a request per row. It is passed **into** `FromEntity` rather than read off the entity: stores
are a shared catalogue behind their own repository and a `Purchase` holds only the id. The list
handler resolves a whole page in one lookup, and create returns the name too — it already had the
store in hand to prove it existed. `null` stays meaningful: cash purchases frequently name no store,
and a store leaving the catalogue does not invalidate the purchases that referenced it.

### 4.8 Installment purchases (tasa 0)
`GET|POST /api/v1/installment-purchases`, `GET /{id}`, `POST /{id}/pay`.

Creating a plan **charges the card the full price immediately**, because that is how an
interest-free plan actually consumes a credit line. The card's limit guard therefore rejects plans
that do not fit, and `availableCredit` stays truthful.

The n child rows are generated up front with due dates `purchaseDate + i months`. **The last
installment absorbs the rounding remainder**, so the schedule sums to exactly the total:
1000 ÷ 12 → 11 × 83.33 + 83.37.

`POST /{id}/pay` takes no installment id — it always pays the oldest unpaid row, using the same
source-selection pattern.

### 4.9 Debts
`GET|POST /api/v1/debts`, `GET|PUT|DELETE /{id}`, `POST /{id}/payments`, `POST /{id}/default`.

Status transitions live in the entity: born `Active`; reaching zero → `PaidOff`; `POST /default` →
`Defaulted` (only from Active); **paying a defaulted debt reactivates it**.

The spec's "due date" is modeled as `monthlyDueDay` (1–31) rather than a stored date, matching cards
and jobs. A stored date would need advancing after every payment; a day-of-month never goes stale,
and `nextDueDate` is computed with the same clamping function.

`remainingAmount` may be set below `originalAmount` at creation, for debts that already existed.

### 4.10 Goals
`/api/v1/savings-goals` and `/api/v1/product-goals`, both with CRUD + `POST /{id}/contribute`.

All progress math lives in `GoalMath` as pure functions shared by both types. A **contribution
opportunity** is a calendar month from the current one through the deadline month *inclusive* — a
deadline this month leaves exactly one.

```
required monthly = (target − current) ÷ opportunities remaining
```

`null` when there is no deadline or the deadline has passed; `0` once the target is reached.
`BehindSchedule` compares the saved fraction against the elapsed-time fraction on a straight line
from creation to deadline.

The savings-goal contribution has two modes:

- **Linked** (savings account, matching currency): a real transfer — source withdraw + linked
  deposit + paired movements, atomically.
- **Unlinked**: tracking only; passing a source account is rejected.

Product goals are tracking-only and their deadline is optional.

### 4.11 Dashboard & intelligence
`GET /api/v1/dashboard`, `GET /api/v1/alerts`, `/api/v1/notifications` (+ `POST /sync`, `POST /{id}/read`).

`FinancialSnapshotLoader` loads the user's whole picture once; `FinancialSnapshot` aggregates it;
both the dashboard and the alert rules read from that same object, so **they can never disagree**.

Two subtleties that took care to get right:

- **Double counting.** Installment balances are already inside `totalUsedCredit` (the card was
  charged). `installmentRemaining` is reported separately as informational, and
  `totalDebt = usedCredit + loanDebt` only.
- **Obligations exclude revolving card debt.** You choose how much of a card balance to pay, so it
  is not a committed outflow. Obligations = loan monthly payments + next installment of each plan.

`safeToSpend = monthlyNetIncome − monthlyObligations`.

Alert rules (in `AlertRules`, pure: same snapshot in, same alerts out): card/debt/installment due
within 7 days, insufficient checking for upcoming card payments (with a "move from savings"
suggestion when savings covers the gap), debt ratio above 40%, spending above income this month,
goals behind schedule or missed, goals reached.

`POST /notifications/sync` persists currently-true alerts, skipping any already sitting unread, so
re-running never piles up duplicates. Marking one read is an acknowledgement, not a permanent mute —
if the condition is still true later, it will be raised again.

**An alert carries the parts it was built from, not only the finished sentence** (§6.13). Alongside
the English `Title` and `Message`, `Params` holds the names, amounts, currency, dates and day counts
that went into them — raw, as invariant decimals and ISO dates, so a client can format them in its
own locale. Notifications persist those parts in a `jsonb` column beside the text, which is what lets
a row raised today be read in a language chosen next month. Rows written before the column exists
have `{}` and fall back to their stored English.

`GoalDeadlinePassed` is its own `AlertType`. It used to share `GoalBehindSchedule`, which left "is
trailing its pace" and "missed its deadline" indistinguishable to anything rendering from the type.

### 4.12 Monthly report
`GET /api/v1/reports/monthly/{yyyy-MM}` (JSON) and `.../pdf`.

Data assembly is an Application query returning `MonthlyReportDto`; rendering is Infrastructure
behind `IPdfReportGenerator`. The generator receives only computed figures — it decides layout,
never what a number means.

**Opening and closing balances are derived**, not stored: today's balance rewound through movements.

```
opening = currentBalance − Σ signed(movements from month start onward)
closing = currentBalance − Σ signed(movements after month end)
```

Accounts and cards created after the period ended are excluded — they did not exist during it.

The report deliberately excludes ATM cash from the net result: `netResult = income − purchases`.
Whatever that cash buys is already recorded as a Cash purchase, so counting both would
double-count it. Cash withdrawn is shown separately as "left your accounts".

Internal transfers are not income — `TransferIn` is filtered out, so moving money to savings does
not inflate earnings.

### 4.13 Payments ledger
`GET /api/v1/payments` (paged, `?from=&to=&targetType=`), `GET /api/v1/credit-cards/{id}/payments`,
`GET /api/v1/debts/{id}/payments`.

Every payment against a card, debt or installment plan writes an immutable `Payment` row —
**whatever its source**. This closes a real gap: before it, an `External` payment reduced a balance
and left nothing behind. The balance was right, but *why* it changed was unknowable, and the
monthly report could not count the payment at all.

The row is written **inside the same transaction** as the balance change, on both paths. An
account-sourced payment therefore produces two records — an `AccountMovement` (money left this
account) and a `Payment` (this debt was reduced) — which are different facts, not duplicates. An
external payment produces only the second.

`TargetId` is **polymorphic**: it points at a card, a debt or a plan depending on `TargetType`, so
it carries no foreign key. That is a deliberate trade — one ledger for all three payment kinds, at
the cost of referential integrity on that column. The alternative (three near-identical tables, or a
nullable FK per target type) buys FK enforcement and pays for it in triplicated queries and a table
that grows a column every time a new payable thing appears. `SourceAccountId` *does* carry an FK
with `RESTRICT`, and a check constraint enforces that `Account` payments name an account while
`External` ones do not.

Per-target endpoints verify ownership before answering, so another user's card id returns 404 rather
than an empty list — an empty list would confirm the id exists.

**Installment payments count toward their card.** Paying an installment calls `RegisterPayment` on
the plan's card, so the report's per-card "Paid" figure includes payments whose target is a plan on
that card.

---

## 5. The database schema

PostgreSQL on Neon, `snake_case` via `UseSnakeCaseNamingConvention()`, money as `numeric(18,2)`,
timestamps as `timestamptz` (UTC always), ids as `uuid`.

| Table | Purpose | Notable columns / constraints |
|---|---|---|
| `users` | identity + profile | `email` unique, `currency` = reporting currency |
| `accounts` | where money lives | `balance` + `currency`, `is_blocked_for_saving`, `is_archived` + `archived_at` |
| `account_movements` | immutable audit trail | `type`, `amount`, `balance_after`, `related_entity_id`; indexes `(user_id, occurred_at)`, `(account_id, occurred_at)` |
| `credit_cards` | cards | `credit_limit`, `used_credit`, `is_archived` + `archived_at`; CHECK `used_credit <= credit_limit`, day ranges 1–31 |
| `jobs` | one per user | `gross_monthly_salary`, `salary_posting_starts_on`; FK deposit account `RESTRICT` |
| `salary_deposits` | one row per settled payday | `scheduled_date`, `amount`, `account_movement_id`; **unique `(job_id, scheduled_date)`** — the exactly-once guarantee |
| `job_payment_days` | 1–3 per job | CHECK `day_of_month BETWEEN 1 AND 31` |
| `deductions` | payslip deductions | `type` (Fixed/Percentage), CHECK `value > 0` |
| `additional_incomes` | recurring extras | `frequency` |
| `stores` | shared catalog | `created_by_user_id` **nullable**, `ON DELETE SET NULL` |
| `purchases` | what you bought | `payment_method`, nullable `account_id`/`credit_card_id`/`store_id`; indexes `(user_id, occurred_at)`, `(user_id, category)` |
| `installment_purchases` | tasa 0 plans | `total_price`, `months_count`; CHECK 1–120 |
| `installment_payments` | generated schedule | unique `(installment_purchase_id, number)` |
| `debts` | loans | CHECK `remaining_amount <= original_amount` |
| `savings_goals` | targets | nullable `linked_account_id`, `ON DELETE SET NULL` |
| `product_goals` | product targets | nullable `deadline` |
| `notifications` | persisted alerts | `type`, `severity`, `is_read`, `params` (`jsonb`); index `(user_id, is_read)` |
| `payments` | every payment, any source | `target_type` + `target_id` (polymorphic, no FK), `source_type`, nullable `source_account_id` (FK `RESTRICT`); CHECK source and account agree; indexes `(user_id, occurred_at)`, `(target_type, target_id)` |

Twelve migrations, `InitialCreate` through `AddPayments`.

`AddPayments` carries a **data backfill**, not just a schema change. Reading "Paid" from an empty
new table would have silently zeroed the report's historical figures — a regression introduced by an
improvement. The migration therefore replays account-sourced payments out of the movement history
(`type = 7`, `related_entity_id` naming the target) into the new table. External payments made
before the table existed left no trace anywhere and are unrecoverable, which is the very gap being
closed.

**Delete behaviors are chosen, not defaulted.** `Cascade` from `users` (deleting a user removes
their data). `Restrict` on job/income deposit accounts (an account referenced by a salary cannot
vanish). `SetNull` on stores and linked goal accounts (the record survives, the link does not).

**Check constraints duplicate domain rules on purpose.** `used_credit <= credit_limit` is enforced
in `CreditCard.Charge` *and* in the database. The entity gives a good error message; the constraint
means a bad row cannot exist even if written by a script, a migration, or a bug. Defense in depth.

### One EF Core subtlety worth remembering

`BaseEntity` assigns ids client-side with `Guid.CreateVersion7()`. EF's default convention for Guid
keys assumes the *database* generates them, so a new child entity discovered through a tracked
parent's navigation looked like an *existing* row — EF issued an `UPDATE` that matched zero rows and
threw `DbUpdateConcurrencyException` (surfacing as a 500 when adding a deduction). The fix, in
`WealthMapDbContext.OnModelCreating`:

```csharp
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    var id = entityType.FindProperty(nameof(BaseEntity.Id));
    if (id is not null)
        id.ValueGenerated = ValueGenerated.Never;
}
```

"The application always supplies keys." Pure change-tracking behavior, no schema impact — and it
pre-empts the same bug in every future aggregate with children.

*Why Guid v7?* Random v4 GUIDs scatter B-tree inserts across the index. Version 7 is
timestamp-prefixed, so ids sort roughly by creation time and inserts stay near the right edge of the
index — the clustering benefit of an auto-increment key without the global sequence.

---

## 6. Decisions and their reasoning

### 6.1 PostgreSQL over MongoDB
The data is deeply relational — accounts have movements, jobs have deductions, plans have
installments — and the invariants are financial. Postgres gives real foreign keys, real check
constraints, and ACID transactions across multiple tables, which is exactly what "withdraw and
record a movement, or do neither" requires. A document store would push referential integrity into
application code, which is the code most likely to have the bug.

### 6.2 No MediatR, no AutoMapper
Both are good libraries; both were avoided *for this project* because they hide the two mechanisms
most worth understanding. Writing `Sender` makes the request pipeline concrete; writing `FromEntity`
makes mapping compile-checked. See §3.3 and §3.6 for the full trade-offs. In a team codebase with
deadlines, taking MediatR is a defensible call — the point is to make it knowingly.

### 6.3 Stored vs derived data
Covered in §3.9. The rule: if a value can be recomputed from other stored values, it is not a
column. Applied to available credit, net salary, goal progress, installment remainders, and report
balances.

### 6.4 Model B — cash leaves tracking
There is no cash wallet. Money withdrawn at an ATM leaves the tracked system; a cash purchase is
recorded for spending analysis but debits nothing.

*Alternative considered:* a virtual "cash" account that ATM withdrawals credit and cash purchases
debit. Rejected because it demands discipline the user will not sustain — every coffee must be
logged or the balance drifts, and a wrong number is worse than an absent one. The consequence,
handled explicitly in the report: ATM cash is excluded from the net result to avoid double-counting
what the Cash purchases already record.

### 6.5 Source selection for payments
Paying a card, a debt, or an installment requires choosing a source: **`Account`** (real withdrawal
+ `Payment` movement, atomically) or **`External`** (cash or someone else paid — debt shrinks, no
account touched, no movement, `accountMovement: null` in the response).

Without `External`, a user paying their card in cash would have to invent a fake account movement,
corrupting the audit trail to record something true. The pattern is identical across all three
payment types, so learning it once is enough.

Both sources write a row to the `payments` ledger (§4.13). The distinction that matters is
**movement vs. payment**: a movement says *money left this account*, a payment says *this balance
was reduced*. An account-sourced payment is both facts at once; an external one is only the second.
Conflating them is what made external payments invisible before the ledger existed.

### 6.6 Option B — user-declared tax
The app does arithmetic; it does not know tax law. Deductions are rows the user copies from their
payslip, as a fixed amount or a percentage.

*Alternative considered:* built-in tax tables per country. Rejected — tax rules change annually,
vary by regime and bracket, and being subtly wrong about someone's net salary is worse than asking
them to type what their payslip already says.

### 6.7 snake_case naming
`UseSnakeCaseNamingConvention()` maps `IsBlockedForSaving` → `is_blocked_for_saving` automatically.
Postgres folds unquoted identifiers to lowercase, so PascalCase tables require quoting everywhere —
`SELECT * FROM "Accounts"`. snake_case is idiomatic, quote-free in psql, and costs one line of
configuration.

### 6.8 User scoping as 404
Covered in §3.7. Applies uniformly, including to stores you do not own.

### 6.9 Single-currency aggregation
`Money` refuses cross-currency arithmetic, so the dashboard cannot silently mix a peso card into a
dollar total. Rather than invent FX rates the app has no source for, totals cover the profile
currency and everything else is surfaced in `excludedCurrencies`. An incomplete number the user can
see the shape of beats a wrong number that looks complete.

### 6.10 API versioning from `/api/v1/`
Every route is versioned from day one. Adding `/v2/` later is additive; retrofitting a version
segment onto live clients is a breaking change. The cost today is five characters.

### 6.11 Archiving instead of deleting
Accounts and cards had no delete endpoint at all, which meant a mistyped account was permanent. A
hard delete was never actually available: it would either cascade `account_movements` away or be
refused outright by the `RESTRICT` foreign keys from purchases, payments and jobs. Both outcomes are
worse than the problem.

Archiving keeps every referencing row intact and removes the item from lists, dropdowns and totals —
which is what "delete" means to someone using the app. The wording in the UI says the history is
kept, because "delete" otherwise implies it goes too.

The one consequence worth naming: an archived item is unreachable from the interface. `Restore()`
exists on both entities and the data is all still there, but nothing is wired to it.

### 6.12 Salary posting as catch-up, not a clock tick
A daily timer only works if the process is up on the day. This app is not expected to run
continuously, so a timer would silently skip any payday the machine slept through — and a missing
deposit is invisible in a way a duplicate is not.

Each run instead asks which paydays are due and unpaid, and settles all of them. The app being off
delays the deposit; it does not lose it. That framing makes the run naturally idempotent, which is
the property that matters, because the alternative to exactly-once here is inventing money.

Exactly-once is enforced by the database, not by the check. The service reads which paydays are
already settled and skips them, but two instances can pass that check in the same instant; the
unique index on `(job_id, scheduled_date)` is what actually decides.

### 6.13 Alerts store the parts, not the sentence
An alert used to ship a finished English sentence with the figures interpolated into it. That is a
conclusion, and this codebase stores facts (§3.9) — given only the sentence there is nothing to
rebuild it from, so it could never be said in another language.

Alerts now carry `Params` alongside the text, and notifications persist them. Storing only the
finished text would have frozen each row in the language it was raised in, no matter what the reader
later chose. The English stays as the fallback: the PDF needs it, and a client that does not know an
alert type should say something rather than nothing.

Values go over the wire raw — invariant decimals, ISO dates — so the client formats them in the
user's locale rather than inheriting the server's culture. Where the server makes a decision the
wording depends on, it sends the decision: `savingsCover: "true"`, not the suggestion sentence.

---

## 7. Running, testing, migrating

See [README.md](../README.md) for the quickstart. Beyond it:

**Migrations** run from `WealthMap_Back-End`:

```powershell
dotnet ef migrations add YourName --project src/WealthMap.Infrastructure --startup-project src/WealthMap.Api
dotnet ef database update --project src/WealthMap.Infrastructure --startup-project src/WealthMap.Api
```

Migration *generation* is offline and always works. `database update` needs network reach to the
host; on a network that blocks Postgres (port 5432), generate now and apply later. Nothing is lost.

**Read what EF generates before applying it.** Two of this project's migrations would have been
wrong as written. `AddArchivingToAccountsAndCards` needed a backfill — the generated default for a
new `DateOnly` column is `0001-01-01`, which would have made every payday in the catch-up window look
unpaid and dumped a year of back-salary into accounts on first run. `AddNotificationParams` defaulted
a `jsonb` column to `""`, which is not valid JSON and would have failed outright on Postgres. Neither
is exotic; both were caught by reading the file.

**Testing** has been manual through Postman, module by module. The natural next step is automated
tests, and the architecture is already shaped for them:

- **Domain unit tests** need no mocks at all — `PaymentSchedule`, `GoalMath`, `Money`, and every
  entity rule are pure or in-memory. This is where the highest value per line is.
- **Handler tests** need repository fakes only, because handlers depend on interfaces.
- **Integration tests** would use `WebApplicationFactory` against a test database.

**A note on file locking:** if `dotnet build` fails with "file is locked by WealthMap.Api", the API
is running. Stop it, or build to a different output directory with `-p:OutDir=...`.

---

## 8. Known limitations

Stated plainly, because knowing the edges is part of knowing the system.

- **Payments made before the `payments` table existed are partly unrecoverable.** The `AddPayments`
  migration backfills account-sourced history from movements, but external payments from that era
  left no trace and cannot be reconstructed. Everything from this module onward is complete.
- **No FX.** Multi-currency holdings are excluded from totals rather than converted (§6.9).
- **Card balances in the report are current, not month-end.** Reconstructing a historical card
  balance would need charge/payment events on the card itself, which are not recorded as such.
- **Salary posts automatically; other recurring income does not.** `SalaryPostingRunner` credits the
  deposit account on each payment day (§4.5). `AdditionalIncome` still describes *expected* income
  only — money appears there when a deposit movement is created by hand.
- **Notifications are pull-based.** `POST /sync` is called by a client; there is no background job
  and no email yet, though the entity is shaped for it. The salary runner is the only scheduled work.
- **One job per user.** Enforced at creation. Multiple jobs would make `NetPerDeposit` ambiguous.
- **Archived accounts and cards cannot be restored from the UI.** `Restore()` exists on both
  entities and no data is lost, but nothing calls it (§6.11).
- **The monthly report is UTC throughout.** Its period is bounded in UTC and the PDF prints times in
  UTC, labelled. The web client shows local time, so the same purchase can read at a different hour
  in each. Making the report zone-aware means moving the period boundaries too, which changes which
  purchases fall in which month.
- **Alert bodies raised before `params` existed stay English.** Those rows have `{}` and fall back to
  the text they were stored with; only re-raised alerts can be translated (§6.13).
- **Field-level validation messages are English.** They name specific rules and lengths that a
  client-side pattern match would mangle rather than translate.

---

## 9. Glossary

| Term | Meaning |
|---|---|
| **Aggregate root** | An entity owning child rows that outsiders may only touch through it (`Job` → deductions, payment days; `InstallmentPurchase` → installments). Keeps invariants in one place. |
| **Value object** | A type defined by its values, not an id, and immutable — `Money`. Two `Money(100, "USD")` are the same money. |
| **CQRS** | Commands (change state) and queries (read) as separate objects with separate handlers. |
| **Mediator** | The indirection between a controller and its handler; the controller sends a request without knowing who handles it. |
| **Pipeline behavior** | Middleware for the mediator — wraps every handler. Validation is the one implemented here. |
| **Unit of Work** | The transaction boundary: several changes commit together or not at all. |
| **DTO** | Data Transfer Object — the API's shape, decoupled from entities so the domain can change without breaking clients. |
| **Movement** | An immutable record of one balance change. The audit trail. |
| **BalanceAfter** | The account's balance immediately after a movement. A stored fact, which is what makes historical reconstruction possible. |
| **Source selection** | Choosing `Account` or `External` when paying something (§6.5). |
| **Payment (ledger row)** | A record that a balance was reduced, whatever the money's source. Distinct from a movement, which records money leaving an account. An account-sourced payment produces both; an external one only the payment. |
| **Polymorphic target** | `payments.target_id` points at a card, debt or plan depending on `target_type`. One ledger for all three, at the cost of a foreign key on that column (§4.13). |
| **Backfill** | A data migration that reconstructs history for a newly added table, so an improvement does not silently erase past figures. |
| **Tasa 0** | Interest-free installments. The card is charged the full price up front; installments repay it. |
| **Contribution opportunity** | A calendar month from now through the deadline month inclusive; the denominator of a goal's required monthly contribution. |
| **Safe to spend** | Net monthly income minus committed obligations (loan payments + next installments). |
| **Clamping** | Snapping a day-of-month to a month's real length — the "30th" in February. |
| **Snapshot** | `FinancialSnapshot`: the user's whole financial picture loaded once, shared by the dashboard and alerts so they cannot disagree. |
| **Archiving** | A delete that keeps the row. The item leaves every list and total; everything referencing it is untouched (§6.11). |
| **Catch-up** | Doing all the work that is due and unpaid, rather than only today's. What makes salary posting survive downtime (§6.12). |
| **Payday** | One date salary is due, after clamping. Two payment days landing on the same date in a short month are one payday. |
| **Exactly-once** | Each payday paid once regardless of restarts, retries or concurrent instances. Guaranteed by a unique index, not by a check. |
| **Alert params** | The names, amounts and dates an alert sentence was built from, stored beside it so it can be re-expressed in another language (§6.13). |
