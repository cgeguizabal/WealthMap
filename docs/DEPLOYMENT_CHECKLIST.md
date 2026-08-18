# Deployment checklist

Everything that has to be true outside the code for WealthMap to run correctly. Written because the
encryption work introduced failure modes that a green build says nothing about: a missing key stops
the app, and a *wrong* key silently corrupts every write.

---

## 1. Encryption keys — before anything else

Two independent 256-bit keys, base64-encoded. They are not interchangeable and must not be the same
value.

| Setting | Purpose |
|---|---|
| `Encryption:Key` | AES-256-GCM key for the encrypted columns |
| `Encryption:BlindIndexKey` | HMAC-SHA256 key for `users.email_lookup` |

Generate each one separately (PowerShell 5.1 or 7):

```powershell
$b=[byte[]]::new(32); [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
```

Development uses user-secrets:

```powershell
dotnet user-secrets set "Encryption:Key" "<base64>" --project src/WealthMap.Api
dotnet user-secrets set "Encryption:BlindIndexKey" "<base64>" --project src/WealthMap.Api
```

Production uses environment variables — `Encryption__Key`, `Encryption__BlindIndexKey` — or the
host's secret store. **Never commit a key.** `appsettings.json` ships the section with empty strings
so the shape is discoverable; empty is treated as missing and the app refuses to start.

### Losing or changing a key

There is no recovery. These keys are not derived from anything and are not stored in the database.

- **Lose `Encryption:Key`** and every encrypted column is permanently unreadable. The rows remain,
  the plaintext does not.
- **Lose `Encryption:BlindIndexKey`** and no one can sign in — every lookup hashes to a value that
  matches no row — until every user's `email_lookup` is recomputed, which requires the *encryption*
  key to read the addresses back.
- **Change either quietly** and the failure is worse than an outage: new writes use the new key,
  old rows do not, and nothing announces the split until someone opens an old record.

Back both up, separately from the database backup. A backup holding the ciphertext *and* the key
protects against nothing.

---

## 2. Applying the encryption migrations

Three steps, in this order, with a check between each. Run against a Neon branch first.

```powershell
# 1. Widen the columns and add email_lookup (nullable). Nothing is encrypted yet;
#    the running app is unaffected by this one.
dotnet ef database update EncryptPiiColumns --project src/WealthMap.Infrastructure --startup-project src/WealthMap.Api

# 2. Encrypt the existing rows and fill the blind index. Supervised, and safe to re-run.
dotnet run --project src/WealthMap.Api -- --encrypt-pii

# 3. Make email_lookup NOT NULL and unique. Refuses to run if step 2 was skipped.
dotnet ef database update RequireEmailLookup --project src/WealthMap.Infrastructure --startup-project src/WealthMap.Api
```

Step 2 prints a row count per table. Re-running it is a genuine no-op — each table is asked which
rows still lack the `v1:` envelope, and a converted database returns none. That matters because the
realistic failure is an interrupted run.

**Verify between steps 2 and 3**, connected to the database:

```sql
-- Expect 0. Anything else means step 2 did not finish.
SELECT count(*) FROM users WHERE email_lookup IS NULL OR email NOT LIKE 'v1:%';

-- Expect every row to start with v1:.
SELECT count(*) FILTER (WHERE name NOT LIKE 'v1:%') AS plaintext_names FROM accounts;

-- Expect 0. A duplicate here will block step 3, and means two accounts share an address.
SELECT count(*) FROM (SELECT email_lookup FROM users GROUP BY 1 HAVING count(*) > 1) d;
```

Then sign in as an existing user before running step 3. If the blind index is wrong, this is where
it shows, and step 3 is the point after which fixing it is harder.

**Rolling back** is only possible before step 2. Afterwards ciphertext will not fit back into
`char(4)`, and the way back is a restore from backup.

---

## 2b. Rotating the encryption keys

Do this if a key is exposed, or on whatever schedule you decide. It runs while the app is serving
traffic — nothing needs to be taken down.

**Rotate both keys together.** Not because the encryption key and the blind-index key are linked,
but because the pass that rewrites the data selects rows by the *encryption* stamp. Change only the
blind-index key and the pass finds nothing to do, the indexes are never recomputed, and sign-in keeps
working purely because the old key is still configured — which is not a rotation, it is two keys
where there should be one.

**1. Generate a new pair** and keep the current pair — you need both during the rotation.

**2. Deploy with all four values set.** `KeyVersion` goes up by one:

```
Encryption__Key                    = <new encryption key>
Encryption__BlindIndexKey          = <new blind index key>
Encryption__KeyVersion             = 2
Encryption__PreviousKey            = <the outgoing encryption key>
Encryption__PreviousBlindIndexKey  = <the outgoing blind index key>
```

From this moment everything written carries `v2:`, everything already stored still says `v1:` and is
still readable, and sign-in tries both blind indexes. Users notice nothing.

**3. Rewrite the stored data** — the same command that did the first encryption:

```powershell
dotnet run --project src/WealthMap.Api -- --encrypt-pii
```

It selects rows that lack the current stamp, reads each with the previous key and writes it back with
the new one. Safe to re-run, and safe to interrupt.

**4. Check nothing is left behind**, per encrypted column:

```sql
SELECT count(*) FROM users        WHERE email     NOT LIKE 'v2:%';
SELECT count(*) FROM accounts     WHERE name      NOT LIKE 'v2:%';
SELECT count(*) FROM credit_cards WHERE card_name NOT LIKE 'v2:%';
SELECT count(*) FROM notifications WHERE title    NOT LIKE 'v2:%';
```

All zero. Then sign in as a real user — that is what proves the blind indexes were recomputed rather
than merely still matching on the old key.

**5. Remove the two previous keys** and redeploy:

```
Encryption__PreviousKey            =
Encryption__PreviousBlindIndexKey  =
```

Until you do, the retired key is still live configuration, and a key you rotated away from because it
leaked is still useful to whoever has it. After this step, any row still on `v1:` fails loudly with a
message naming the missing setting — which is why step 4 comes first.

**Destroy the old keys** only once step 5 is deployed and the app has been exercised.

---

## 3. Database role

Apply `docs/DB_ROLES.sql` as the database owner, then point the application's connection string at
`wealthmap_app` and keep the owner connection for migrations only.

The application role can read and write rows but cannot create, alter or drop a table. Re-run the
grants after any migration that adds a table.

---

## 4. Configuration that must be set

| Setting | Notes |
|---|---|
| `ConnectionStrings:WealthMapDb` | The restricted role from section 3, not the owner |
| `Jwt:Secret` | Long random string; rotating it invalidates every access token |
| `Jwt:Issuer`, `Jwt:Audience` | Must match between issuing and validating |
| `Encryption:Key`, `Encryption:BlindIndexKey` | Section 1 |
| `Cors:AllowedOrigins` | Exact frontend origin. Empty means no cross-origin call succeeds |
| `Auth:Cookie:Secure` | `true` in production |
| `Auth:Cookie:SameSite` | `Lax` same-site; `None` if the frontend is on another domain, which then requires `Secure` |

The app fails at startup if the connection string or either encryption key is missing. That is
deliberate — an app that boots and fails later fails somewhere nobody is watching.

---

## 5. Before going public

- [ ] **A lawyer has reviewed `docs/legal/`.** Still outstanding. The draft banner was
      removed by product decision — the pages now carry the app's beta marker instead, which
      says nothing about who has read the text. Review is no longer visible to users, so it is
      only tracked here.
- [ ] Both Spanish translations reviewed too — `PRIVACY_POLICY.es.md` and
      `TERMS_OF_SERVICE.es.md` carry the same commitments and need the same scrutiny.
- [ ] `POLICY_VERSION` in `src/config/legal.js` matches the version headers in all four Markdown files.
- [ ] **A contact address is still missing from all four legal documents.** They carry a visible
      placeholder. A privacy policy needs a way to reach the operator for access and portability
      requests — deletion is self-service now, but those are not.
- [ ] The privacy policy still describes what the software does. It currently states that the
      operator holds the keys and can read user data — if that ever stops being true, say so; if it
      stays true, do not soften it.
- [ ] `/privacy` and `/terms` load while signed out.
- [ ] Registration refuses to submit without the consent box, and the stored user row has
      `terms_accepted_at` and `accepted_policy_version`.

---

## 6. Known gaps

Real, and not addressed by this work:

- **The app reasons in UTC outside the monthly report.** "Due in N days", goal countdowns and
  alert thresholds are computed against the UTC date, so they can read a day out for part of each
  day — six hours daily at UTC-6. It is cosmetic and self-correcting, but it is not right. Date
  validators no longer reject valid input over it (they allow a day either way); the display side
  would need the user's zone threaded through, the way `GET /reports/monthly` now does it.
