# Backend requests from the frontend

Gaps found while building the Vue client. Nothing here blocks the frontend — each has a working
workaround — but each costs something, noted below. The backend was not modified.

---

## 1. `AuthResultDto` does not return the user's currency

**Found:** building `useMoney`, which formats every monetary value in the user's profile currency.

`POST /api/v1/auth/register` and `/login` return:

```json
{ "userId": "...", "email": "...", "fullName": "...", "token": "..." }
```

The user's `currency` is stored on the `users` row and drives the dashboard and monthly report, but
it is never sent to the client. `src/stores/auth.store.js` already reads `response.currency`, which
is always `undefined`, so it silently falls back to `"USD"`.

**Impact:** a user whose profile currency is MXN would see every amount in the UI labelled USD until
they open the dashboard.

**Workaround in place:**
- On **register**, the currency is in the form the user just submitted, so the store persists that.
- On **login**, it is unknown. The dashboard response includes `currency`, so `dashboard.store.js`
  calls `auth.setCurrency(...)` once loaded, correcting it for the session.
- Until that first dashboard load, formatting falls back to `USD`.

**Ideal fix:** add `currency` to `AuthResultDto` (one field, no new endpoint), or add
`GET /api/v1/users/me` returning the full profile.

---

## 2. No endpoint to read or update the user profile

**Found:** planning the account/settings area.

There is no `GET /api/v1/users/me` and no way to update name, country, birth date or currency.
`User.UpdateProfile(...)` and `ChangePassword(...)` exist on the domain entity but no command,
handler or controller action exposes them.

**Impact:** the frontend cannot offer a profile or settings screen. The header shows the name and
initials from the login response, which is enough for navigation but cannot be edited.

**Workaround in place:** no profile screen is built. The user menu offers logout only.

**Ideal fix:** `GET /api/v1/users/me`, `PUT /api/v1/users/me`, `POST /api/v1/users/me/password`.

---

## 3. Deleting is unavailable for several resources — *resolved for accounts and cards*

**Found:** building list screens with row actions.

| Resource | Delete? |
|---|---|
| Accounts | **yes** — `DELETE /api/v1/accounts/{id}`, archives |
| Credit cards | **yes** — `DELETE /api/v1/credit-cards/{id}`, archives |
| Purchases | no |
| Installment plans | no |
| Stores | no (deliberate — purchases reference them) |
| Jobs, incomes, debts, goals | yes |

Purchases and plans still have no delete endpoint, which is arguably correct — they are history.

**Resolved:** accounts and cards archive rather than delete. The row keeps existing, so movements,
purchases, installment plans and payments that reference it are untouched; it simply stops appearing
in lists, dropdowns and totals. A hard delete was rejected: it would either cascade movement history
away or be refused outright by the `RESTRICT` foreign keys from purchases and payments.

Both endpoints return **204**, and **400** if the item is already archived. `GET /{id}` and
`GET /accounts/{id}/movements` still resolve for archived items, so links from history keep working.

The UI asks twice before calling either one, and says plainly that history is kept.

---

## 5. Purchases cannot be filtered by the card or account that paid — ✅ RESOLVED

**Found:** showing, on a credit card's detail screen, the purchases that created its balance.

`GET /api/v1/purchases` filtered by `year`, `month` and `category` only, so the card detail had to
fetch the most recent 100 purchases and filter client-side — incomplete for anyone with a longer
history.

**Resolved** by adding an optional `creditCardId` filter to `GET /api/v1/purchases`
(`GetPurchasesQuery` → `PurchaseRepository.Filter`). No schema change was needed: the foreign key to
`credit_cards` is already indexed. The frontend now asks for exactly the card's purchases.

**Still open:** `accountId` was not added. It is the same one-line change in the same three files if
a debit-purchase view ever wants it.

---

## 4. Dashboard and alerts are separate round trips

**Found:** building the dashboard, which renders both.

`GET /api/v1/dashboard` and `GET /api/v1/alerts` both build the same `FinancialSnapshot` server-side.
Rendering the dashboard therefore loads the user's entire financial picture twice per visit.

**Impact:** duplicated work per dashboard load. Not user-visible at this data size.

**Workaround in place:** both are requested in parallel with `Promise.all`, so the wall-clock cost
is one round trip rather than two.

**Ideal fix:** either embed alerts in the dashboard response, or accept the duplication — it is a
real cost but a small one, and separate endpoints keep the two concerns independently cacheable.
