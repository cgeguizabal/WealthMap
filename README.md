# WealthMap

A personal finance API: what you have, what you owe, what is safe to spend, and what is due next.

Backend: **.NET 10**, Clean Architecture, PostgreSQL (Neon) via EF Core, JWT auth,
hand-built CQRS mediator, FluentValidation, QuestPDF.

## Quickstart

```powershell
# 1. Restore and build (run from the repo root)
dotnet build WealthMap_Back-End\WealthMap.slnx

# 2. Configure secrets (once) — the API will not start without a connection string
cd WealthMap_Back-End\src\WealthMap.Api
dotnet user-secrets set "ConnectionStrings:WealthMapDb" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
dotnet user-secrets set "Jwt:Secret" "a-long-random-string-at-least-32-chars"

# 3. Apply migrations (from WealthMap_Back-End)
cd ..\..
dotnet ef database update --project src/WealthMap.Infrastructure --startup-project src/WealthMap.Api

# 4. Run
dotnet run --project src\WealthMap.Api
```

The API listens on the port shown in the console (typically `http://localhost:5015`).

### Adding a migration

```powershell
# from WealthMap_Back-End
dotnet ef migrations add YourMigrationName --project src/WealthMap.Infrastructure --startup-project src/WealthMap.Api
```

If `database update` fails to reach the host, the migration is still generated — apply it
from a network that can reach the database.

## Using the API

Every route is versioned under `/api/v1/`. Everything except registration and login
requires `Authorization: Bearer <token>`.

```bash
# Register, then use the returned token for everything else
POST /api/v1/auth/register   { "fullName", "email", "password", "country", "currency" }
POST /api/v1/auth/login      { "email", "password" }
```

| Area | Routes |
|---|---|
| Accounts | `/api/v1/accounts` · `{id}/deposit` · `{id}/withdraw` · `transfer` · `{id}/movements` · `{id}/block` · `{id}/unblock` |
| Credit cards | `/api/v1/credit-cards` · `{id}/limit` · `{id}/payments` |
| Jobs & income | `/api/v1/jobs` · `{jobId}/deductions` · `/api/v1/additional-incomes` |
| Stores | `/api/v1/stores` (shared catalog: everyone reads, only the creator edits) |
| Purchases | `/api/v1/purchases` (debit / credit / cash) |
| Installments | `/api/v1/installment-purchases` · `{id}/pay` |
| Debts | `/api/v1/debts` · `{id}/payments` · `{id}/default` |
| Goals | `/api/v1/savings-goals` · `/api/v1/product-goals` · `{id}/contribute` |
| Intelligence | `/api/v1/dashboard` · `/api/v1/alerts` · `/api/v1/notifications` |
| Reports | `/api/v1/reports/monthly/{yyyy-MM}` · `.../pdf` |

## Conventions worth knowing before you read the code

- **Money is a value object.** Amounts carry their currency and refuse to mix. Request bodies
  usually omit currency: it comes from the account or card involved.
- **Store facts, compute conclusions.** Available credit, goal progress, net salary, safe-to-spend
  and report balances are all computed on read — there are no columns for them.
- **Not yours means 404, never 403.** Every repository query is scoped to the caller's user id.
- **Multi-entity writes are transactional** via `IUnitOfWork.ExecuteInTransactionAsync`.
- **Totals are single-currency.** There are no FX rates; the dashboard aggregates your profile
  currency and reports anything else under `excludedCurrencies` rather than mixing it in.
- **Paging is uniform**: `?page=1&pageSize=20`, max 100, on movements, purchases and notifications.

## Layout

```
WealthMap_Back-End/src/
  WealthMap.Domain/          entities, value objects, enums, pure domain services — no dependencies
  WealthMap.Application/     CQRS handlers, DTOs, validators, repository interfaces
  WealthMap.Infrastructure/  EF Core, repositories, JWT, password hashing, PDF rendering
  WealthMap.Api/             controllers, middleware, DI composition
```

Dependencies point inward only: Api → Infrastructure → Application → Domain.

A full walkthrough of the architecture, patterns and decisions lives in
[docs/PROJECT_GUIDE.md](docs/PROJECT_GUIDE.md).
