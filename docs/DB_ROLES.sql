-- =============================================================================
-- WealthMap — least-privilege database role for the running application
-- =============================================================================
--
-- ⚠ DO NOT COMMIT A REAL PASSWORD INTO THIS FILE.
--
-- The placeholder in STEP 1 is meant to be replaced in the SQL editor, not
-- here. This file is tracked in git; a password typed into it and committed
-- lives in the history even after it is deleted from the file. Copy the block
-- into the Neon SQL Editor and put the password in there.
--
--
-- WHY THIS EXISTS
--
-- Encrypting the identifying columns raises the cost of a stolen database dump.
-- It does nothing about a compromised application connection, which by default
-- holds the same rights as the person who created the schema: DROP TABLE, ALTER
-- TABLE, and ownership of everything in it. An injection flaw or a leaked
-- connection string in that setup is not a data breach, it is an erased
-- database.
--
-- This script creates a role that can read and write rows and nothing else. It
-- cannot create, alter or drop a table, cannot change a column type, and cannot
-- grant itself anything. Applying a migration deliberately requires the other,
-- privileged connection — which is the point: schema change becomes an act, not
-- an accident.
--
-- WHAT IT DOES NOT PROTECT AGAINST
--
-- This role can still SELECT every row in every table, which is what the
-- application needs to function. It reduces the blast radius of a compromise;
-- it does not make the data unreadable to whoever holds the connection. The
-- encryption keys live in application configuration, not in the database, so
-- this role alone cannot decrypt anything — but the application it serves can.
--
--
-- HOW TO RUN IT
--
--   WHERE:  Neon Console → your project → SQL Editor.
--           Pick the branch you are targeting in the selector at the top.
--           Roles belong to a branch, so this must be run on each branch the
--           application will connect to.
--
--   AS:     The owner role, which is whoever the console connects you as by
--           default. A restricted role cannot create another restricted role.
--
--   WHAT:   Paste STEP 1 into the editor, replace the password there, run it.
--           Then paste and run STEP 2, then STEP 3. Nothing else needs
--           substituting: the script asks the database for its own name and
--           for the migrating role.
--
--   NOT THE CONSOLE'S ROLES PAGE. A role created through the console, CLI or
--   API is granted membership in neon_superuser, which carries broadly the
--   privileges this file exists to withhold. CREATE ROLE in SQL inherits
--   nothing it is not given. STEP 3b proves which kind you ended up with.
--
--   The console will not show a connection string for a SQL-created role
--   either. Build it from the owner's by swapping two fields:
--
--     Host=<same>;Database=<same>;Username=wealthmap_app;Password=<yours>;SSL Mode=Require
--
-- AFTERWARDS
--
-- Run STEP 2 again after any migration that creates a table. Grants apply to
-- tables that exist at the time; the default privileges below cover only tables
-- created later by the role named in them.
--
-- No connection string in this repository is changed by this script. Switching
-- the application over is a deployment decision, made once, by hand.
-- =============================================================================


-- =============================================================================
-- STEP 1 — Create the role
-- =============================================================================
-- Generate a password:
--
--   $b=[byte[]]::new(24); [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b); [Convert]::ToBase64String($b)
--
-- Paste it over the placeholder IN THE SQL EDITOR, not in this file. Do not
-- reuse the owner's password. Store it in a password manager — the console
-- cannot show it to you again.
--
-- NOSUPERUSER, NOCREATEDB and NOCREATEROLE are already the defaults, and are
-- spelled out because a role definition is read by people deciding whether to
-- trust it.

CREATE ROLE wealthmap_app WITH
    LOGIN
    PASSWORD 'PASTE_PASSWORD_IN_THE_SQL_EDITOR'
    NOSUPERUSER
    NOCREATEDB
    NOCREATEROLE
    NOINHERIT
    NOREPLICATION
    NOBYPASSRLS;


-- =============================================================================
-- STEP 2 — Grant exactly what the application needs
-- =============================================================================
-- Run this whole block in one go. It fills in the database name and the
-- migrating role by asking for them, because both differ per project — Neon's
-- default database is `neondb`, not `wealthmap` — and a hardcoded guess either
-- fails outright or, worse, succeeds while granting nothing.

DO $$
BEGIN
    -- Reach the database and the schema, but do not own them. USAGE explicitly
    -- does not include CREATE, so this role cannot add a table to public.
    EXECUTE format(
        'GRANT CONNECT ON DATABASE %I TO wealthmap_app', current_database());

    EXECUTE 'GRANT USAGE ON SCHEMA public TO wealthmap_app';

    -- Postgres 15 and later already revoke this from PUBLIC; older versions do
    -- not, and on those every role could create tables in public.
    EXECUTE 'REVOKE CREATE ON SCHEMA public FROM PUBLIC';
    EXECUTE 'REVOKE CREATE ON SCHEMA public FROM wealthmap_app';

    -- Rows: yes. Structure: no.
    --
    -- TRUNCATE and REFERENCES are deliberately absent. TRUNCATE empties a table
    -- in one statement and does not fire the row-level rules a DELETE would;
    -- REFERENCES allows creating foreign keys, which is schema work.
    EXECUTE 'GRANT SELECT, INSERT, UPDATE, DELETE '
            'ON ALL TABLES IN SCHEMA public TO wealthmap_app';

    -- Tables created by future migrations.
    --
    -- Grants are per table, so a table added next month would be invisible to
    -- the application until someone remembered to grant it — a failure that
    -- shows up in production as "permission denied for table X" long after the
    -- migration looked successful.
    --
    -- current_user is the role running this script, which must also be the role
    -- that runs migrations: default privileges attach to the creator of a
    -- table, not to the grantee.
    EXECUTE format(
        'ALTER DEFAULT PRIVILEGES FOR ROLE %I IN SCHEMA public '
        'GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO wealthmap_app',
        current_user);

    RAISE NOTICE 'Granted on database % for migrating role %',
        current_database(), current_user;
END $$;

-- Every key in this schema is a Guid generated in C# (BaseEntity, version 7), so
-- there are no sequences to grant. If one is ever introduced, add:
--   GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO wealthmap_app;


-- =============================================================================
-- STEP 3 — Verify
-- =============================================================================
-- Run each query and check it against what it says to expect. A role that looks
-- created but was granted nothing behaves exactly like a correct one until the
-- application tries to read.

-- 3a. Expect one row, with rolsuper / rolcreatedb / rolcreaterole all false.

SELECT rolname, rolsuper, rolcreatedb, rolcreaterole, rolbypassrls
FROM   pg_roles
WHERE  rolname = 'wealthmap_app';

-- 3b. Expect ZERO rows. The Neon-specific check: a role made through the
--     console, CLI or API is a member of neon_superuser and would carry back
--     everything this script withholds. If a row comes back, the role was not
--     created by STEP 1 — drop it and run STEP 1 here instead.

SELECT r.rolname AS member_of
FROM   pg_auth_members m
JOIN   pg_roles r ON r.oid = m.roleid
JOIN   pg_roles u ON u.oid = m.member
WHERE  u.rolname = 'wealthmap_app';

-- 3c. Expect can_use_schema = true, can_create_tables = FALSE.
--     The second is the whole point of the exercise.

SELECT has_schema_privilege('wealthmap_app', 'public', 'USAGE')  AS can_use_schema,
       has_schema_privilege('wealthmap_app', 'public', 'CREATE') AS can_create_tables;

-- 3d. Expect ZERO rows: no table may be missing any of the four privileges.
--     This is the quick version of "check every table by eye".
--
--     Reads pg_catalog rather than information_schema, and checks privileges by
--     OID rather than by name. information_schema failed on Neon's SQL editor
--     with "relation schemata does not exist", and passing the OID also removes
--     any dependence on search_path or on quoting the identifier correctly.

SELECT c.relname AS table_missing_a_privilege
FROM   pg_class c
JOIN   pg_namespace n ON n.oid = c.relnamespace
WHERE  n.nspname = 'public'
  AND  c.relkind = 'r'
  AND  NOT (has_table_privilege('wealthmap_app', c.oid, 'SELECT')
        AND has_table_privilege('wealthmap_app', c.oid, 'INSERT')
        AND has_table_privilege('wealthmap_app', c.oid, 'UPDATE')
        AND has_table_privilege('wealthmap_app', c.oid, 'DELETE'))
ORDER  BY c.relname;

-- 3e. Sanity check on the count — expect every table the app uses, including
--     freelance_jobs, plus __EFMigrationsHistory.

SELECT count(*) AS tables_granted
FROM   pg_class c
JOIN   pg_namespace n ON n.oid = c.relnamespace
WHERE  n.nspname = 'public'
  AND  c.relkind = 'r'
  AND  has_table_privilege('wealthmap_app', c.oid, 'SELECT');


-- =============================================================================
-- STEP 4 — The negative test (optional, and the one that actually proves it)
-- =============================================================================
-- Reconnect AS wealthmap_app, using the connection string built from the
-- owner's, then run:
--
--   CREATE TABLE should_not_exist (id int);
--
-- It must fail with "permission denied for schema public". If it succeeds, the
-- role still holds something it should not, and nothing above has restricted it.
