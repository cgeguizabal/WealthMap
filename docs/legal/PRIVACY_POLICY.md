# WealthMap Privacy Policy

**Version 1.0** · **Effective 18 August 2026**

---

## 1. In short

WealthMap is a personal finance tracker. You type in your accounts, cards,
purchases and goals, and it works out what you can safely spend.

- Everything you enter is entered by you. WealthMap does not connect to your
  bank, read your email, or import transactions from anywhere.
- **Your name, email and the details that identify your accounts and cards are
  encrypted** before they are stored, so a stolen copy of the database does not
  show whose money it is describing.
- **The operator of WealthMap holds the encryption keys and can therefore read
  your data.** This is described honestly in section 5, because a policy that
  implied otherwise would be false.
- Nothing is sold, and nothing is shared for advertising.

## 2. Who is responsible

WealthMap is operated by an individual developer ("the operator", "we").
Contact: **cgeguizabal@gmail.com**.

Because WealthMap is a personal project rather than a company, there is no data
protection officer and no formal privacy team. Requests go to the address above
and are handled by one person.

## 3. What is collected

### 3.1 What you give us

Everything in this list is typed in by you. There is no other source.

| Category | Examples | Why |
|---|---|---|
| Account identity | Full name, email address, country, display currency | To create your account, sign you in, and format money and dates |
| Credentials | Password | Stored only as a salted hash — see 5.4 |
| Bank accounts | Account name, bank, type, balance, last four digits, linked debit card, notes | The balances the app reasons about |
| Credit cards | Card name, bank, limit, balance owed, interest rate, cutoff and due days, last four digits, notes | To project what is owed and when |
| Spending | Purchases, amounts, dates, categories, stores, installment plans, notes | To track spending and installment commitments |
| Income | Employer name, salary, pay days, deductions, additional income | To project money arriving |
| Obligations | Debts, payments made, savings and product goals | To work out what is committed |
| Consent | Which policy version you accepted, and when | To have a record that you agreed |

### 3.2 What the software records on its own

- **Session tokens.** A refresh token is stored as a hash so a session can be
  renewed and revoked. Tokens expire and rotate on every use.
- **Timestamps.** Rows carry created and updated times.
- **Server logs.** Errors are logged with the HTTP method, the path and the
  identifier of the record involved. Logs deliberately do not contain names,
  email addresses, notes or request bodies.

### 3.3 What is not collected

No analytics, no advertising identifiers, no tracking pixels, no third-party
scripts, no cookies beyond the single session cookie described in section 7.
There is no bank connection and no email ingestion — WealthMap cannot see any
account you have not typed in yourself.

## 4. What it is used for

Your data is used to run WealthMap for you: to show your balances, project your
liquidity, calculate what is safe to spend, generate your monthly report, and
send you in-app notifications about cutoffs and due dates.

It is not used to train machine learning models, build a profile of you, or
market anything to you.

## 5. How it is protected — and the limits of that

### 5.1 Encryption at rest

These columns are encrypted individually before being written, using AES-256-GCM
with a 256-bit key:

- your full name, email address and country;
- account names, notes, account digits and debit card digits;
- credit card names, notes and card digits;
- debt names, purchase notes, savings and product goal names;
- notification titles, messages and their contents.

GCM is an authenticated mode, which means a value altered directly in the
database fails to decrypt rather than quietly returning something plausible.

Your email address is additionally stored as a keyed hash, under a **separate**
key, so that sign-in can find your account without the database holding a
searchable copy of the address itself.

### 5.2 What encryption here does and does not do

**It protects against a stolen database.** A copy of the database — a leaked
backup, a compromised hosting account, a misconfigured snapshot — is not
readable without the keys, which are not stored in it.

**It does not put your data beyond the operator's reach.** The application
decrypts your data on every page you load, so the keys live in the
application's configuration, and the operator controls that configuration. This
is pseudonymisation, not end-to-end or zero-knowledge encryption.

Stated plainly: **the operator is technically able to read your data.** Anyone
who tells you a design like this one prevents that is describing a different
system. If you would not be comfortable with one person being able to read what
you enter, do not enter it.

### 5.3 What a stolen copy of the database would show

This is the scenario the encryption is actually for, so it is worth being exact
about.

**Amounts, dates and categories are not encrypted.** They cannot be — every
balance, projection and report is arithmetic over those values, and the database
has to be able to sort and filter them. What *is* encrypted is everything that
says who they belong to: your name, your email, your country, the names and
digits of your accounts and cards, your notes.

So someone who obtained a copy of the database **without** the keys would see
figures and dates attached to an anonymous identifier, and no readable name or
email address to attach them to. That is a real and deliberate limit on the
damage, and it is the main thing encryption at rest buys you.

**It is not anonymity, and it would be wrong to describe it that way.** Two
things qualify it:

- Not every field is encrypted. Bank names, spending categories and store names
  are stored as-is, because the app matches and groups by them. A determined
  analyst can sometimes work out who a person is from a pattern of ordinary
  details, and nothing here prevents that.
- Anyone holding the keys — the operator, or an attacker who took the
  application's configuration as well as its database — can link every row back
  to you immediately. The separation only holds while the keys are separate.

### 5.4 Passwords

Your password is stored only as a salted hash. It is not encrypted, because
encryption implies it could be reversed. The operator cannot read your password
and cannot tell you what it is — a forgotten password can only be replaced.

### 5.5 In transit

Traffic is served over HTTPS. Session cookies are marked HttpOnly and Secure,
so browser JavaScript cannot read them.

## 6. Where it is stored and who else touches it

The database is hosted on **Neon** (serverless PostgreSQL). Neon is a processor:
it stores the data on the operator's behalf and does not use it for anything
else. Encrypted columns arrive at Neon already encrypted.

Data is not shared with, sold to, or disclosed to anyone else, with two
exceptions any operator must state: to comply with a valid legal demand, and to
investigate abuse or a security incident.

Depending on the hosting region, data may be stored outside your country.

## 7. Cookies

One cookie: the refresh token that keeps you signed in. It is HttpOnly, Secure,
SameSite, and expires. There are no analytics or advertising cookies, so there
is nothing to opt out of.

## 8. How long it is kept

Your data is kept while your account exists. Delete your account and the
associated rows are deleted with it — accounts, cards, purchases, goals and the
rest are removed by cascade rather than left behind.

Refresh tokens expire on their own schedule. Server logs are kept only as long
as the hosting platform retains them.

## 9. Your choices

- **See it.** Every screen shows you your own data; the monthly report exports
  it as a PDF.
- **Correct it.** Every record in WealthMap can be edited.
- **Delete it.** Individual records can be deleted from the app. To delete your
  whole account, use Settings → Delete your account. It happens immediately:
  nothing is archived, there is no grace period, and it cannot be undone.
- **Take it with you.** Ask, and you will be sent your data in a machine-readable
  form.
- **Withdraw consent.** Stop using WealthMap and ask for deletion.

Depending on where you live, you may have additional statutory rights.

## 10. Children

WealthMap is not intended for anyone under 16, and is not knowingly used by
them.

## 11. Changes

Material changes raise the version number at the top of this document. The
version you accepted is recorded against your account, so it is always possible
to tell which text you agreed to.

## 12. Contact

**cgeguizabal@gmail.com**
