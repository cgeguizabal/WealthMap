# WealthMap — API Reference

Complete endpoint reference. For *why* the API behaves this way, see
[PROJECT_GUIDE.md](PROJECT_GUIDE.md); for setup, see [README.md](../README.md).

Base URL in development: `http://localhost:5015`. Every route is prefixed **`/api/v1/`**.

---

## Conventions

**Authentication.** Everything except `/auth/register` and `/auth/login` requires:

```
Authorization: Bearer <token>
```

The caller's identity always comes from the token. **No endpoint accepts a user id in its body or
query string** — sending one has no effect.

**Currency is usually implicit.** Amounts are denominated in the currency of the account or card
involved, so most request bodies omit it. The exceptions, where there is no instrument to inherit
from, are: account creation, card creation, job creation, additional income, debts, goals, and
**cash** purchases.

**Dates.** `DateOnly` fields use `yyyy-MM-dd`. Timestamps are UTC ISO-8601. Day-of-month fields
(`paymentDueDay`, `monthlyDueDay`, job payment days) are integers 1–31 and clamp to short months —
the "31st" resolves to the 30th in November and the 28th/29th in February.

**Paging.** Paged endpoints take `?page=1&pageSize=20`. `pageSize` max is **100**. The envelope:

```json
{ "items": [ ... ], "page": 1, "pageSize": 20, "totalCount": 57,
  "totalPages": 3, "hasNextPage": true, "hasPreviousPage": false }
```

**Status codes.**

| Code | When |
|---|---|
| `200` | Success |
| `201` | Created — includes a `Location` header |
| `204` | Deleted |
| `400` | Validation failure or a broken business rule |
| `401` | Missing, malformed or expired token |
| `404` | Not found **or not yours** — the two are deliberately indistinguishable |

**Two shapes of 400.** Validation errors are field-keyed:

```json
{ "title": "Validation failed", "status": 400,
  "errors": { "Amount": ["Deposit amount must be greater than zero."] } }
```

Business-rule violations carry a single message:

```json
{ "title": "Business rule violation", "status": 400,
  "detail": "Insufficient funds in 'Checking'. Available: 120.00 USD, requested: 500.00 USD." }
```

**Enums in requests are integers; in responses they are strings.** A request sends
`"type": 2`; the response returns `"type": "Savings"`. The exception is payment `sourceType`, which
is a string both ways (`"Account"` / `"External"`, case-insensitive on input).

### Enum values

| Enum | Values |
|---|---|
| Account `type` | `1` Checking · `2` Savings |
| Movement `type` | `1` SalaryDeposit · `2` Deposit · `3` Bonus · `4` TransferIn · `5` TransferOut · `6` Purchase · `7` Payment · `8` AtmWithdrawal |
| Deduction `type` | `1` Fixed · `2` Percentage |
| Income `frequency` | `1` Weekly · `2` Biweekly · `3` Monthly · `4` Yearly |
| Purchase `paymentMethod` | `1` DebitAccount · `2` CreditCard · `3` Cash |
| Debt `status` (response) | `Active` · `PaidOff` · `Defaulted` |
| Goal `status` (response) | `OnTrack` · `BehindSchedule` · `DeadlinePassed` · `Completed` |
| Payment `targetType` | `CreditCard` · `Debt` · `Installment` |
| Payment `sourceType` | `Account` · `External` |
| `trackingMode` | `1` Manual · `2` EmailSync |
| `debitCardType` | `1` None · `2` Physical · `3` Digital |
| Bank default `direction` | `1` Inbound · `2` Outbound |

---

## Authentication

### `POST /api/v1/auth/register`

```json
{ "email": "you@example.com", "password": "at-least-8-chars",
  "fullName": "Your Name", "country": "US", "currency": "USD" }
```

**200** → `{ "userId", "email", "fullName", "token" }`

`currency` becomes your reporting currency: the dashboard and monthly report aggregate in it, and
holdings in other currencies are excluded from totals rather than converted.

### `POST /api/v1/auth/login`

```json
{ "email": "you@example.com", "password": "..." }
```

**200** → same shape as register. **400** if either field is empty or the credentials are wrong —
the message does not distinguish the two, so it cannot be used to discover which emails exist.

---

## Accounts

### `POST /api/v1/accounts` → **201**

```json
{ "name": "Checking", "bankName": "BBVA", "type": 1,
  "openingBalance": 1000, "currency": "USD",
  "lastFour": "6868", "trackingMode": 1,
  "debitCardType": 2, "debitCardLastFour": "4417" }
```

All four identifying fields are optional. Omitted, they default to `null`, `1` (Manual), `1` (None)
and `null`.

### `GET /api/v1/accounts` · `GET /api/v1/accounts/{id}`

```json
{ "id": "...", "name": "Checking", "bankName": "BBVA", "type": "Checking",
  "balance": 1000.00, "currency": "USD", "isBlockedForSaving": false,
  "lastFour": "6868", "trackingMode": "Manual",
  "debitCardType": "Physical", "debitCardLastFour": "4417",
  "notes": null, "createdAt": "2026-08-01T12:00:00Z" }
```

### `PUT /api/v1/accounts/{id}`

```json
{ "name": "Checking", "bankName": "BBVA", "notes": "main account" }
```

Only these three fields are editable. Balance changes only through movements; type and currency are
immutable because changing them would invalidate the movement history.

### `DELETE /api/v1/accounts/{id}` → **204**

Archives the account rather than deleting the row. It stops appearing in `GET /accounts`, in the
dashboard totals and in every account dropdown, but its movements stay, and the purchases, payments
and jobs that reference it are untouched — a hard delete would either cascade that history away or
be refused by the `RESTRICT` foreign keys pointing at it.

`GET /accounts/{id}` and `GET /accounts/{id}/movements` still resolve afterwards, so links from
history keep working. Nothing new can be pointed at it: creating a job or income with an archived
`depositAccountId` → **404**. Archiving an already-archived account → **400**.

The archived balance is simply excluded from totals; archiving does not move or zero the money.

### `PUT /api/v1/accounts/{id}/tracking`

```json
{ "trackingMode": 1, "lastFour": "6868" }
```

**200** → the full account DTO.

Sets both fields together, because they constrain each other. `lastFour` must match `^\d{4}$` or be
`null`/omitted; `trackingMode` must be `1` or `2`.

**`trackingMode: 2` with no `lastFour` → 400**, keyed to the `lastFour` field: *"Last 4 digits are
required to enable email sync."* The same message comes back for clearing `lastFour` on an
instrument already in `EmailSync` — an instrument can never be synced without identifying digits
(§4.2 of the project guide). A check constraint repeats the rule in the database.

Nothing consumes these fields yet; see "Planned: automatic transaction sync" (§6.14 of the project
guide).

### `PUT /api/v1/accounts/{id}/debit-card`

```json
{ "debitCardType": 2, "debitCardLastFour": "4417" }
```

**200** → the full account DTO.

The card's own last four, **not** the account's — `lastFour` is the account number. A notification
about a card purchase quotes the card; one about a transfer quotes the account.

The type governs the digits: sending `1` (None) **clears** `debitCardLastFour` whatever was passed,
so a number cannot outlive the card it belonged to. The digits stay optional for a card that does
exist — a user may know they have one without knowing its number.

An undefined type or digits that are not exactly four → **400**.

### `POST /api/v1/accounts/{id}/block` · `POST /api/v1/accounts/{id}/unblock`

No body. A blocked account still accepts deposits but refuses withdrawals. Blocking an
already-blocked account → **400**.

### `POST /api/v1/accounts/{id}/deposit`

```json
{ "amount": 500, "description": "cash deposit", "type": 2 }
```

`type` accepts only **2** (Deposit) or **3** (Bonus). SalaryDeposit and TransferIn are
system-generated. Returns the movement.

### `POST /api/v1/accounts/{id}/withdraw`

```json
{ "amount": 100, "description": "groceries", "location": "ATM Reforma 222" }
```

`location` is optional. Always recorded as `AtmWithdrawal` — the only manual outbound type.
**400** on insufficient funds or a blocked account.

### `POST /api/v1/accounts/transfer`

```json
{ "fromAccountId": "...", "toAccountId": "...", "amount": 250, "description": null }
```

**200** → `{ "fromAccount": {...}, "toAccount": {...}, "amount", "currency", "occurredAt" }`

Atomic: both balances and both movements commit together or not at all. Same account for both
sides → **400**.

### `GET /api/v1/accounts/{id}/movements?page=1&pageSize=20`

Paged, newest first. Each item:

```json
{ "id": "...", "accountId": "...", "type": "Purchase", "amount": 85.50,
  "currency": "USD", "balanceAfter": 914.50, "description": "Purchase: Groceries",
  "location": null, "relatedEntityId": "...", "isInbound": false,
  "occurredAt": "2026-08-08T14:22:01Z" }
```

---

## Credit cards

### `POST /api/v1/credit-cards` → **201**

```json
{ "cardName": "Gold", "bankName": "BBVA", "creditLimit": 5000, "currency": "USD",
  "annualInterestRate": 45.9, "paymentDueDay": 15, "statementCutoffDay": 28,
  "lastFour": "7765", "trackingMode": 1 }
```

`lastFour` and `trackingMode` are optional; omitted, they default to `null` and `1` (Manual). A card
has no debit-card fields — those belong to an account.

### `GET /api/v1/credit-cards` · `GET /api/v1/credit-cards/{id}`

```json
{ "id": "...", "cardName": "Gold", "bankName": "BBVA", "creditLimit": 5000.00,
  "usedCredit": 1800.00, "availableCredit": 3200.00, "currency": "USD",
  "annualInterestRate": 45.900, "paymentDueDay": 15, "statementCutoffDay": 28,
  "lastFour": "7765", "trackingMode": "Manual",
  "nextCutoffDate": "2026-08-28", "nextDueDate": "2026-09-17",
  "daysUntilCutoff": 14, "daysUntilDue": 34,
  "lastCutoffDate": "2026-07-28",
  "statementBalance": 50.00, "currentCycleCharges": 50.00, "futureInstallments": 0.00,
  "notes": null, "createdAt": "..." }
```

`availableCredit` is computed, never stored.

`nextDueDate` is when **today's balance** must be paid — the first due day *after* the next cutoff,
not simply the next occurrence of `paymentDueDay`. It is the same date the dashboard's safe-to-spend
projection reserves against.

**`usedCredit` splits three ways**, and the parts sum back to it:

| Field | Meaning | Deadline |
|---|---|---|
| `statementBalance` | closed on `lastCutoffDate` | due `nextDueDate` |
| `currentCycleCharges` | spent since then | billed at `nextCutoffDate` |
| `futureInstallments` | plan balance beyond this cycle | on no statement yet |

Owing 100 with 50 due on the 15th and 50 not billed for another month is a different obligation from
owing 100 at once, which is why the total alone is not enough.

There is no statement history, so the split is reconstructed from charge dates. Payments are not
read — what is still owed *is* the unpaid part, and payments settle the oldest debt first, so the
open cycle is the smaller of "charged since the cutoff" and "still owed". A balance the purchase
records cannot explain falls into `statementBalance`, the older and more urgent reading.

These fields appear on **every** response carrying a card, reads and writes alike.

### `PUT /api/v1/credit-cards/{id}/tracking`

```json
{ "trackingMode": 1, "lastFour": "7765" }
```

**200** → the full card DTO. Identical rules and errors to the account endpoint above.

### `PUT /api/v1/credit-cards/{id}`

```json
{ "cardName": "Gold", "bankName": "BBVA", "annualInterestRate": 39.9,
  "paymentDueDay": 10, "statementCutoffDay": 25, "notes": "main card" }
```

### `DELETE /api/v1/credit-cards/{id}` → **204**

Archives the card, on the same terms as an account: it leaves `GET /credit-cards` and the dashboard
credit totals, while its purchases, installment plans and payments stay on record and
`GET /credit-cards/{id}` still resolves. Archiving an already-archived card → **400**.

Outstanding debt is not settled or written off — it is only excluded from the totals. Archiving a
card that still owes money is allowed, and deliberately so: the debt lives in the payment and
purchase history, which archiving leaves intact.

### `PUT /api/v1/credit-cards/{id}/limit`

```json
{ "newLimit": 8000 }
```

**400** if the new limit is below current `usedCredit`.

### `POST /api/v1/credit-cards/{id}/payments`

```json
{ "amount": 200, "sourceType": "Account", "sourceAccountId": "...", "notes": null }
```

```json
{ "amount": 200, "sourceType": "External", "sourceAccountId": null, "notes": "paid in cash" }
```

**200** → `{ "card": {...}, "accountMovement": {...} | null, "settledInstallments": [...] }`

```json
"settledInstallments": [
  { "installmentPurchaseId": "...", "productName": "TV", "number": 2, "monthsCount": 12, "amount": 41.67 }
]
```

`Account` withdraws from the named account and writes a `Payment` movement; `External` (cash or a
third party paid) touches no account and returns `accountMovement: null`. **Both** write a row to
the payments ledger. `sourceAccountId` is required for `Account` and must be absent for `External`.
Paying more than is owed → **400**.

**The payment advances any installment plans on the card.** A plan's installment for the month is
part of the statement being paid, so it is marked paid — oldest due date first across every plan,
whole installments only, and never beyond what the statement had already billed however large the
payment. `settledInstallments` reports what moved, so a client can say so rather than leaving the
user to notice a plan advanced on its own.

Two things deliberately do *not* happen: no separate `Payment` row is written for a settled
installment, and the card balance is not reduced twice. The money left the account once, and the
installment being marked paid is the consequence of that payment rather than another one. So an
installment settled this way appears in `GET /payments` as a **card** payment.

Paying a plan early is still its own action — `POST /installment-purchases/{id}/pay` — which reduces
both the plan and the card balance by one month.

There is **no charge endpoint** — cards are charged by purchases and installment plans.

### `GET /api/v1/credit-cards/{id}/payments`

Every payment against this card, newest first, from any source. See [Payments](#payments).

---

## Jobs & income

### `POST /api/v1/jobs` → **201**

```json
{ "title": "Full-stack Dev", "employer": "Acme", "grossMonthlySalary": 4000,
  "currency": "USD", "depositAccountId": "...", "paymentDays": [15, 30] }
```

One job per user — a second create → **400**. `paymentDays` must hold 1–3 distinct days.

### `GET /api/v1/jobs` · `GET /api/v1/jobs/{id}`

```json
{ "id": "...", "title": "Full-stack Dev", "employer": "Acme",
  "grossMonthlySalary": 4000.00, "currency": "USD",
  "netMonthly": 3000.00, "netPerDeposit": 1500.00,
  "depositAccountId": "...", "paymentDays": [15, 30],
  "deductions": [ { "id": "...", "name": "Income tax", "type": "Percentage", "value": 20.00 } ],
  "nextPaymentDates": ["2026-08-15", "2026-08-30", "2026-09-15"], "createdAt": "..." }
```

`netMonthly = gross − Σfixed − gross × Σpercentage / 100`, computed. `nextPaymentDates` shows the
next three, month-end clamped.

### `PUT /api/v1/jobs/{id}` · `DELETE /api/v1/jobs/{id}` → **204**

```json
{ "title": "...", "employer": "...", "grossMonthlySalary": 4500,
  "depositAccountId": "...", "paymentDays": [15, 30] }
```

Currency cannot change. `paymentDays` replaces the existing set. Deleting a job deletes its
deductions and payment days.

### Automatic salary deposits

Salary is paid into `depositAccountId` on each of the job's `paymentDays`, with no request needed.
Each payday produces one `SalaryDeposit` movement of `netPerDeposit`, described as `Salary — {employer}`.

The runner works by catch-up, not by clock tick: every run asks which paydays are *due and unpaid*
and settles all of them. So the app being off on payday delays the deposit rather than losing it —
it posts on the next startup. It runs on startup and every 24 hours after.

A payday is paid exactly once. A `salary_deposits` row records each settled payday, and a unique
index on `(job_id, scheduled_date)` enforces it even if two instances run at the same moment.
Restarting the app, or calling the run endpoint repeatedly, posts nothing extra.

Posting never reaches back before the job's creation date, so adding a job cannot backfill months
of salary and invent a balance that never existed. Jobs that existed before this feature start from
the date it was deployed.

Salary is held, not dropped, when it cannot be paid — the deposit account is archived or missing, or
the job's currency differs from the account's (never converted). Fixing the cause pays the held
paydays on the next run.

### `GET /api/v1/jobs/{jobId}/salary-deposits`

```json
[ { "id": "...", "jobId": "...", "accountId": "...", "scheduledDate": "2026-08-15",
    "amount": 1500.00, "currency": "USD", "postedAt": "...", "accountMovementId": "..." } ]
```

Newest payday first. `scheduledDate` is the payday settled; `postedAt` is when it was written — they
differ when a deposit was caught up late.

### `POST /api/v1/jobs/{jobId}/salary-deposits/run` → **200**

```json
{ "posted": 1 }
```

Settles any due-but-unpaid payday now instead of waiting for the daily run. Safe to call repeatedly:
`posted` is `0` once everything due is settled.

### Deductions (nested)

`POST /api/v1/jobs/{jobId}/deductions` · `PUT .../deductions/{deductionId}` ·
`DELETE .../deductions/{deductionId}`

```json
{ "name": "Income tax", "type": 2, "value": 20 }
```

`type` **1** = Fixed (an amount in the salary's currency), **2** = Percentage (of gross, max 100).
All three return the **full updated job**, so you always see the recomputed net. Deductions that
would push net below zero → **400**.

### Additional incomes

`POST|GET /api/v1/additional-incomes` · `GET|PUT|DELETE /{id}`

```json
{ "name": "Freelance", "amount": 500, "currency": "USD",
  "frequency": 3, "depositAccountId": "..." }
```

Recurring extras only; one-off money is a `Bonus` deposit. Frequencies are normalized to a monthly
figure for the dashboard (weekly × 52/12, biweekly × 26/12, yearly ÷ 12).

---

## Bank defaults

Which account to assume when a bank's transfer notification names none. Stored now, consumed by
nothing — see "Planned: automatic transaction sync" (§6.14 of the project guide).

### `GET /api/v1/bank-defaults`

```json
[ { "id": "...", "bankName": "Banco Agricola", "direction": "Outbound",
    "defaultAccountId": "...", "defaultAccountName": "Cuenta Principal",
    "createdAt": "..." } ]
```

Ordered by bank, then direction. `defaultAccountName` is resolved server-side so a client does not
have to fetch the account list purely to render a row.

### `PUT /api/v1/bank-defaults`

```json
{ "bankName": "Banco Agricola", "direction": 2, "defaultAccountId": "..." }
```

**200** → the created or updated row.

An **upsert on `(bankName, direction)`**, matched case-insensitively — the key is the pair, not an
id, so saying the same thing twice leaves one row rather than failing on the unique index. `PUT`
rather than `POST` because it is idempotent.

**An archived account → 404.** A fallback pointing at an account that can no longer be transacted
with could never be honoured, so it is refused at the point of nomination rather than discovered
later. An account belonging to someone else is also 404, as everywhere.

### `DELETE /api/v1/bank-defaults/{id}` → **204**

A real delete, not an archive: a bank default holds no history and nothing references it, so there
is nothing to preserve. Not yours → **404**.

---

## Stores

A **shared catalog** — the one non-user-scoped resource. Everyone reads every store; only the
creator may edit theirs. Editing someone else's → **404**.

### `POST /api/v1/stores` → **201** · `GET /api/v1/stores` · `GET /{id}` · `PUT /{id}`

```json
{ "name": "Walmart", "category": "Groceries",
  "logoUrl": "https://logo.clearbit.com/walmart.com", "description": "Supermarket" }
```

Response adds `"isMine": true|false`. The creator's user id is never exposed. `logoUrl` must be a
valid absolute URL when present. There is no delete — purchases reference stores.

---

## Purchases

### `POST /api/v1/purchases` → **201**

The `paymentMethod` decides which fields are required:

```json
{ "productName": "Groceries", "amount": 85.50, "currency": null, "occurredAt": null,
  "storeId": "...", "category": "Food", "paymentMethod": 1,
  "accountId": "...", "creditCardId": null, "notes": null }
```

| `paymentMethod` | Requires | Must be null | Effect |
|---|---|---|---|
| `1` DebitAccount | `accountId` | `creditCardId` | withdraws + writes a `Purchase` movement |
| `2` CreditCard | `creditCardId` | `accountId` | charges the card |
| `3` Cash | `currency` | both ids | records the purchase only |

`occurredAt` defaults to now and cannot be in the future. Currency comes from the account or card;
only cash needs it explicitly. A debit purchase exceeding the balance, or a credit purchase
exceeding available credit → **400**, with nothing written.

### `GET /api/v1/purchases?year=2026&month=8&category=food&creditCardId=…&page=1&pageSize=20`

Paged, newest first. All filters are optional and compose. `category` is case-insensitive.
`month` requires `year` → **400** otherwise. `creditCardId` narrows to the purchases charged to one
card; a malformed GUID → **400**, an unknown one → an empty page.

```json
{ "id": "...", "productName": "Groceries", "amount": 85.50, "currency": "USD",
  "occurredAt": "...", "storeId": "...", "storeName": "Walmart", "category": "Food",
  "paymentMethod": "DebitAccount", "accountId": "...", "creditCardId": null,
  "notes": null, "createdAt": "..." }
```

`storeName` accompanies `storeId` so a list can show where a purchase was made without a second
request per row. It is `null` when the purchase named no store — cash purchases often do not — and
also if that store has since left the catalogue, which does not invalidate the purchase. Every
purchase response carries it, including the one returned by `POST /purchases`.

### `GET /api/v1/purchases/{id}`

### `PUT /api/v1/purchases/{id}`

Same body as `POST`. **200** → the corrected `PurchaseDto`.

Everything is editable, **including the payment method and the instrument** — "it went on the other
card" is the correction people actually need. The server reverses what the purchase did and applies
it afresh rather than adjusting by a difference, so a method change is handled by the same path as an
amount change.

Currency follows the instrument. Moving a purchase from a dollar card to a peso account re-denominates
the amount; only cash carries an explicit `currency`.

Same validation as creating one. **400** if the method and instrument disagree, the amount is not
positive, or the date is in the future.

### `DELETE /api/v1/purchases/{id}` → **204**

A **real delete**, unlike accounts and cards, which archive. The money is put back: a debit purchase
refunds the account and removes the movement it wrote, a credit purchase un-charges the card, a cash
purchase moves nothing.

Later movements on that account are **rebased**, so the running balance still adds up after the
removal. What is lost is the record that this purchase ever existed — accepted deliberately, because
a mistyped purchase is noise in every total rather than history worth keeping.

**400 when the card has been paid below the charge**: *"Cannot reverse 55.00 USD on 'Visa': only
20.00 USD is still owed."* Reversing would drive used credit negative, which the card cannot
represent. Pay history has moved on; the purchase can no longer be un-charged.

---

## Installment purchases (tasa 0)

Every plan response carries the card it was bought on and what it adds to that card's current
statement:

```json
{ "creditCardId": "...", "creditCardName": "Mastercard Gold", "creditCardBankName": "Banco Cuscatlan",
  "dueThisStatement": 100.00, "statementDueDate": "2026-09-05", … }
```

`dueThisStatement` is the plan's installments falling due on or before `statementDueDate`, which is
the **card's** next payment date — so this figure and the card's own `statementBalance` are computed
from the same rule and cannot disagree. It drops to `0.00` once the month's installment is paid.

`creditCardName` is null only when the card no longer exists; archived cards are still named, since a
plan outlives the archiving of the card it sits on and its debt is real either way.

### `POST /api/v1/installment-purchases` → **201**

```json
{ "productName": "TV", "totalPrice": 1200, "storeId": null,
  "creditCardId": "...", "monthsCount": 12, "purchasedAt": null }
```

Creating a plan **charges the card the full price immediately** — that is how an interest-free plan
consumes a credit line — so a plan exceeding available credit → **400**. `monthsCount` is 1–120;
`purchasedAt` defaults to today and cannot be in the future.

### `GET /api/v1/installment-purchases` · `GET /{id}`

```json
{ "id": "...", "productName": "TV", "totalPrice": 1200.00, "currency": "USD",
  "monthlyPayment": 100.00, "monthsCount": 12, "purchasedAt": "2026-08-10",
  "storeId": null, "creditCardId": "...",
  "remainingBalance": 1100.00, "remainingMonths": 11, "endDate": "2027-08-10",
  "isCompleted": false,
  "payments": [ { "id": "...", "number": 1, "amount": 100.00, "currency": "USD",
                  "dueDate": "2026-09-10", "isPaid": true, "paidAt": "..." } ],
  "createdAt": "..." }
```

The schedule is generated at creation. The **last installment absorbs the rounding remainder**, so
the rows sum to exactly the total (1000 ÷ 12 → 11 × 83.33 + 83.37).

### `POST /api/v1/installment-purchases/{id}/pay`

```json
{ "sourceType": "Account", "sourceAccountId": "...", "notes": null }
```

Takes **no installment id** — always pays the oldest unpaid row. Same source rules as card payments.
Paying a completed plan → **400**. Returns `{ "purchase": {...}, "accountMovement": {...} | null }`.

---

## Debts

### `POST /api/v1/debts` → **201**

```json
{ "name": "Car loan", "originalAmount": 5000, "remainingAmount": null,
  "currency": "USD", "monthlyPayment": 250, "monthlyDueDay": 5 }
```

`remainingAmount` defaults to `originalAmount`; pass a lower value to register a debt already
partly paid. It may not exceed the original → **400**.

### `GET /api/v1/debts` · `GET /{id}`

```json
{ "id": "...", "name": "Car loan", "originalAmount": 5000.00, "remainingAmount": 4750.00,
  "currency": "USD", "monthlyPayment": 250.00, "monthlyDueDay": 5,
  "nextDueDate": "2026-09-05", "status": "Active", "createdAt": "..." }
```

`nextDueDate` is computed and clamped; it is `null` once the debt is paid off.

### `PUT /api/v1/debts/{id}` · `DELETE /{id}` → **204**

```json
{ "name": "Car loan (refi)", "monthlyPayment": 200, "monthlyDueDay": 10 }
```

Amounts are not editable here — they change through payments.

### `POST /api/v1/debts/{id}/payments`

```json
{ "amount": 250, "sourceType": "Account", "sourceAccountId": "...", "notes": null }
```

Same source rules as cards. Overpaying the remaining balance → **400**. Returns
`{ "debt": {...}, "accountMovement": {...} | null }`.

**Paying a defaulted debt reactivates it** (`Defaulted` → `Active`).

### `POST /api/v1/debts/{id}/default`

No body. Marks the debt `Defaulted`; only an `Active` debt can default → **400** otherwise.

### `GET /api/v1/debts/{id}/payments`

Every payment against this debt, any source.

---

## Payments

A ledger of every payment against a card, debt or installment plan — **including external ones**,
which touch no account and therefore leave no movement.

### `GET /api/v1/payments`

Query: `?from=2026-08-01&to=2026-08-31&targetType=CreditCard&page=1&pageSize=20`

All filters optional. `to` **includes its whole day**. `targetType` is `CreditCard`, `Debt` or
`Installment` (case-insensitive); anything else → **400**, as does `to` earlier than `from`.

```json
{ "id": "...", "targetType": "CreditCard", "targetId": "...",
  "amount": 150.00, "currency": "USD", "sourceType": "External",
  "sourceAccountId": null, "occurredAt": "...", "notes": "paid in cash at branch" }
```

`targetId` is polymorphic — it names a card, debt or plan according to `targetType`.
`sourceAccountId` is set for `Account` payments and null for `External` ones.

### `GET /api/v1/credit-cards/{id}/payments` · `GET /api/v1/debts/{id}/payments`

Unpaged, newest first. A target that is not yours → **404**, not an empty array.

Installment payments are reachable through `GET /api/v1/payments?targetType=Installment`; they carry
an auto-note like `"Installment 3/12"` when you do not supply one.

> Payments recorded before this ledger existed were backfilled from movement history. External
> payments from that era left no trace and could not be recovered.

---

## Goals

### Savings goals — `/api/v1/savings-goals`

`POST` → **201** · `GET` · `GET /{id}` · `PUT /{id}` · `DELETE /{id}` → **204**

```json
{ "name": "Emergency fund", "targetAmount": 6000, "currency": "USD",
  "currentAmount": 0, "deadline": "2027-08-01", "linkedAccountId": null }
```

`linkedAccountId` must name a **savings** account in the goal's currency, or → **400**. Deadlines
cannot be in the past.

```json
{ "id": "...", "name": "Emergency fund", "targetAmount": 6000.00, "currentAmount": 500.00,
  "currency": "USD", "deadline": "2027-08-01", "linkedAccountId": null,
  "progressPercentage": 8.33, "monthsRemaining": 13,
  "requiredMonthlyContribution": 423.08, "status": "OnTrack", "createdAt": "..." }
```

All four derived fields are computed on read. `requiredMonthlyContribution` is `null` with no
deadline or once the deadline has passed, and `0` once the target is reached.

### `POST /api/v1/savings-goals/{id}/contribute`

```json
{ "amount": 500, "sourceAccountId": null }
```

**Unlinked goal** — tracking only; passing a `sourceAccountId` → **400**.
**Linked goal** — `sourceAccountId` is required and performs a real transfer into the linked
account (paired TransferOut/TransferIn movements, atomic). It may not be the linked account itself.

**200** → `{ "goal": {...}, "sourceMovement": {...} | null }`

### Product goals — `/api/v1/product-goals`

Same verbs. Deadline is **optional**; without one there is no required-monthly figure and no
schedule to fall behind.

```json
{ "name": "PlayStation 6", "targetAmount": 700, "currency": "USD",
  "currentAmount": 0, "deadline": "2027-02-28" }
```

`POST /{id}/contribute` takes `{ "amount": 200 }` only — product goals never touch real accounts.
Contributing past the target caps `progressPercentage` at 100 and sets `status: "Completed"`.

---

## Dashboard & alerts

### `GET /api/v1/dashboard`

```json
{ "currency": "USD",
  "totalAvailable": 4800.00, "totalInChecking": 2800.00, "totalInSavings": 2000.00,
  "totalCreditLimit": 5000.00, "totalUsedCredit": 1800.00, "totalAvailableCredit": 3200.00,
  "totalLoanDebt": 4000.00, "installmentRemaining": 1200.00, "totalDebt": 5800.00,
  "netWorth": -1000.00,
  "monthlyNetIncome": 5000.00, "monthlyObligations": 350.00, "safeToSpend": 4650.00,
  "monthSpending": 800.00, "debtRatioPercentage": 7.00,
  "upcomingDueDates": [
    { "kind": "Debt", "entityId": "...", "name": "Car loan",
      "dueDate": "2026-08-10", "daysUntil": 2, "amount": 250.00 } ],
  "goals": { "total": 1, "completed": 0, "behindSchedule": 0,
             "totalTargeted": 6000.00, "totalSaved": 100.00 },
  "excludedCurrencies": ["MXN"] }
```

Reading these correctly:

- **`installmentRemaining` is already inside `totalUsedCredit`** (plans charge the card). It is
  informational; `totalDebt` = `totalUsedCredit` + `totalLoanDebt` only.
- **`monthlyObligations` excludes revolving card balances** — you choose how much of those to pay.
  It is loan payments + the next installment of each active plan.
- **`excludedCurrencies`** lists holdings left out of every total. There are no FX rates; a card in
  another currency is reported here rather than silently mixed in.
- `upcomingDueDates` covers the next 30 days across cards, debts and installments.

### `GET /api/v1/alerts`

Computed live, ordered Critical → Warning → Info. Nothing is stored by this call.

```json
[ { "type": "InsufficientBalanceForCardPayment", "severity": "Critical",
    "title": "Checking balance will not cover upcoming card payments",
    "message": "1,800.00 USD is due within 7 days but checking holds 1,200.00 USD. You could move 600.00 USD from savings to cover it.",
    "relatedEntityId": null,
    "params": { "owed": "1800.00", "checking": "1200.00", "shortfall": "600.00",
                "currency": "USD", "days": "7", "savingsCover": "true" } } ]
```

`title` and `message` are composed in English. `params` carries the parts they were
built from, so a client can say the same thing in another language — given only the
sentence there is nothing to rebuild it from. Keys vary by `type`; values are raw, not
pre-formatted (amounts as invariant decimals, dates as ISO), so the client formats them
in its own locale. `savingsCover` is a decision, not a sentence: the server chooses which
suggestion applies and the client words it.

A client that does not know a `type` should fall back to `title` and `message`.

| `type` | Fires when |
|---|---|
| `CardPaymentDueSoon` | a card with a balance is due within 7 days (Critical within 2) |
| `DebtPaymentDueSoon` · `InstallmentDueSoon` | same window, Info |
| `InsufficientBalanceForCardPayment` | checking will not cover cards due within 7 days |
| `HighDebtRatio` | obligations above 40% of net income (Critical above 60%) |
| `OverspendingVsIncome` | this month's purchases exceed net monthly income |
| `GoalBehindSchedule` | a goal trails the pace needed for its deadline |
| `GoalDeadlinePassed` | a goal reached its deadline unfunded (Critical) |
| `GoalReached` | a goal is fully funded |

### Notifications

`GET /api/v1/notifications?unreadOnly=true&page=1&pageSize=20` — paged, newest first.

`POST /api/v1/notifications/sync` — persists currently-true alerts, **skipping any already unread**,
and returns only what it created. Calling it twice in a row returns `[]`.

`POST /api/v1/notifications/{id}/read` — marks one read; calling it twice is harmless. Marking read
is an acknowledgement, not a mute: if the condition is still true at the next sync, it is raised
again.

A notification carries the same `params` as the alert it was raised from, alongside the stored
English `title` and `message`. That is what lets an old notification be shown in a language chosen
long after it was raised — storing only the finished sentence would freeze each row in the language
it was written in. Rows created before `params` existed have `{}` and fall back to their English.

---

## Reports

### `GET /api/v1/reports/monthly/{yyyy-MM}`

e.g. `/api/v1/reports/monthly/2026-08`. Malformed months → **400**.

```json
{ "month": "2026-08", "currency": "USD",
  "periodStart": "2026-08-01", "periodEnd": "2026-08-31", "userFullName": "...",
  "income": { "total": 4000.00,
              "lines": [ { "type": "SalaryDeposit", "total": 3000.00, "count": 2 } ],
              "expectedSalaryNet": 3000.00 },
  "spending": { "totalPurchases": 800.00, "totalCashWithdrawn": 1600.00,
                "byCategory": [ { "category": "Electronics", "total": 600.00,
                                  "count": 1, "sharePercentage": 75.00 } ],
                "topExpenses": [ { "productName": "Monitor", "category": "Electronics",
                                   "amount": 600.00, "occurredAt": "2026-08-08T19:22:00Z",
                                   "paymentMethod": "CreditCard", "storeName": "Walmart" } ] },
  "accounts": [ { "accountId": "...", "name": "Checking", "type": "Checking",
                  "openingBalance": 3000.00, "closingBalance": 1200.00,
                  "totalIn": 0, "totalOut": 1800.00, "movementCount": 2 } ],
  "cards": [ { "cardId": "...", "cardName": "Gold", "creditLimit": 5000.00,
               "usedCredit": 1450.00, "availableCredit": 3550.00,
               "chargedThisMonth": 1800.00, "paidThisMonth": 350.00, "paymentDueDay": 15 } ],
  "goals": [ { "kind": "Savings", "name": "Emergency fund", "targetAmount": 6000.00,
               "currentAmount": 500.00, "progressPercentage": 8.33, "status": "OnTrack" } ],
  "netResult": 3200.00, "generatedAt": "..." }
```

`topExpenses[].storeName` is where the purchase was made, resolved from the shared store catalogue.
It is `null` when the purchase named no store — cash purchases often do not — so treat it as
optional rather than assuming every expense has one.

**Times.** Every timestamp in this API is UTC, and clients are expected to render in the viewer's
own zone — the web client does. `topExpenses[].occurredAt` was `occurredOn` (a bare date) and is now
the full instant, so the hour survives as far as the report.

The PDF is the exception: it prints times in **UTC**, labelled, in a `Date (UTC)` column. It does
not convert to a local zone because the report's month is bounded in UTC — converting only the
display could put a 31 July timestamp inside an August report. So the PDF and the web client can
show different hours for the same purchase. Making the report properly zone-aware means moving the
period boundaries too, which changes which purchases land in which month.

Reading these correctly:

- **`netResult` = income − purchases.** `totalCashWithdrawn` is reported but deliberately excluded:
  whatever that cash buys is already a Cash purchase, so counting both would double-count it.
- **Internal transfers are not income.** `TransferIn` is filtered out of the income section.
- **Opening/closing balances are derived** by rewinding today's balance through movements, so
  `opening + totalIn − totalOut = closing`.
- **`paidThisMonth` counts payments from any source**, including external ones, plus installment
  payments on plans belonging to that card.
- Accounts and cards created after the period ended are omitted — they did not exist during it.
- Only holdings in your profile currency appear.

### `GET /api/v1/reports/monthly/{yyyy-MM}/pdf`

Same data rendered as a PDF: `Content-Type: application/pdf`, filename `wealthmap-2026-08.pdf`.
In Postman use **Send and Download**. A month with no activity renders successfully with empty
sections.

---

## Quick reference

| Method | Route |
|---|---|
| POST | `/api/v1/auth/register` · `/api/v1/auth/login` |
| GET POST | `/api/v1/accounts` |
| GET PUT | `/api/v1/accounts/{id}` |
| POST | `/api/v1/accounts/{id}/deposit` · `/withdraw` · `/block` · `/unblock` · `/api/v1/accounts/transfer` |
| GET | `/api/v1/accounts/{id}/movements` |
| GET POST | `/api/v1/credit-cards` |
| GET PUT | `/api/v1/credit-cards/{id}` |
| PUT | `/api/v1/credit-cards/{id}/limit` |
| GET POST | `/api/v1/credit-cards/{id}/payments` |
| GET POST | `/api/v1/jobs` |
| GET PUT DELETE | `/api/v1/jobs/{id}` |
| POST | `/api/v1/jobs/{jobId}/deductions` |
| PUT DELETE | `/api/v1/jobs/{jobId}/deductions/{deductionId}` |
| GET POST | `/api/v1/additional-incomes` |
| GET PUT DELETE | `/api/v1/additional-incomes/{id}` |
| GET POST | `/api/v1/stores` |
| GET PUT | `/api/v1/stores/{id}` |
| GET POST | `/api/v1/purchases` |
| GET | `/api/v1/purchases/{id}` |
| GET POST | `/api/v1/installment-purchases` |
| GET | `/api/v1/installment-purchases/{id}` |
| POST | `/api/v1/installment-purchases/{id}/pay` |
| GET POST | `/api/v1/debts` |
| GET PUT DELETE | `/api/v1/debts/{id}` |
| GET POST | `/api/v1/debts/{id}/payments` |
| POST | `/api/v1/debts/{id}/default` |
| GET | `/api/v1/payments` |
| GET POST | `/api/v1/savings-goals` · `/api/v1/product-goals` |
| GET PUT DELETE | `/api/v1/savings-goals/{id}` · `/api/v1/product-goals/{id}` |
| POST | `/api/v1/savings-goals/{id}/contribute` · `/api/v1/product-goals/{id}/contribute` |
| GET | `/api/v1/dashboard` · `/api/v1/alerts` |
| GET | `/api/v1/notifications` |
| POST | `/api/v1/notifications/sync` · `/api/v1/notifications/{id}/read` |
| GET | `/api/v1/reports/monthly/{yyyy-MM}` · `/api/v1/reports/monthly/{yyyy-MM}/pdf` |
