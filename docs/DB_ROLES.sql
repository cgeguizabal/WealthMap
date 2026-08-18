-- =============================================================================
-- WealthMap — least-privilege database role for the running application
-- =============================================================================
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
-- HOW TO APPLY
--
--   1. Connect to the WealthMap database as the owner (the Neon default role).
--   2. Run section 0 and substitute both answers into sections 2 and 4.
--   3. Replace the password in section 1 with one you generate. Do not reuse
--      the owner's password.
--   4. Run this whole file.
--   5. Point the application's connection string at wealthmap_app, and keep the
--      owner connection for `dotnet ef database update` only.
--
-- CREATE THE ROLE HERE, NOT IN THE NEON CONSOLE
--
-- A role created through the Neon console, CLI or API is granted membership in
-- neon_superuser, which carries broadly the privileges this file exists to
-- withhold. A role created with CREATE ROLE in SQL inherits nothing it is not
-- given. Section 5 checks which kind you ended up with.
--
-- The console will not show a connection string for a SQL-created role either.
-- Build it from the owner's by swapping the user and password:
--
--   Host=<same>;Database=<same>;Username=wealthmap_app;Password=<yours>;SSL Mode=Require
--
-- Run it again after any migration that creates a table: grants apply to tables
-- that exist at the time, and the ALTER DEFAULT PRIVILEGES below only covers
-- tables created later by the role named in it.
--
-- No connection string in this repository is changed by this script. Switching
-- the application over is a deployment decision, made once, by hand.
-- =============================================================================


-- -----------------------------------------------------------------------------
-- 0. Find the two values this script cannot know
-- -----------------------------------------------------------------------------
-- Run these first, as the owner, and substitute the answers below. Guessing
-- either one produces a script that appears to succeed and grants nothing.

SELECT current_database();   -- Neon's default is neondb, not wealthmap
SELECT current_user;         -- the role that runs your migrations


-- -----------------------------------------------------------------------------
-- 1. The role
-- -----------------------------------------------------------------------------
-- NOSUPERUSER, NOCREATEDB, NOCREATEROLE are the defaults, and are spelled out
-- because a role definition is read by people deciding whether to trust it.

CREATE ROLE wealthmap_app WITH
    LOGIN
    PASSWORD 'REPLACE_ME_BEFORE_RUNNING'
    NOSUPERUSER
    NOCREATEDB
    NOCREATEROLE
    NOINHERIT
    NOREPLICATION
    NOBYPASSRLS;


-- -----------------------------------------------------------------------------
-- 2. Reach the database and the schema, but do not own them
-- -----------------------------------------------------------------------------
-- CONNECT and USAGE are the minimum to see the schema at all. USAGE explicitly
-- does not include CREATE, so this role cannot add a table to public.

-- Replace `wealthmap` with the answer to SELECT current_database() above.
GRANT CONNECT ON DATABASE wealthmap TO wealthmap_app;
GRANT USAGE   ON SCHEMA public      TO wealthmap_app;

-- Postgres 15 and later already revoke this from PUBLIC; older versions do not,
-- and on those every role could create tables in public. Harmless to repeat.
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
REVOKE CREATE ON SCHEMA public FROM wealthmap_app;


-- -----------------------------------------------------------------------------
-- 3. Rows: yes. Structure: no.
-- -----------------------------------------------------------------------------
-- TRUNCATE and REFERENCES are deliberately absent. TRUNCATE empties a table in
-- one statement and does not fire the row-level rules a DELETE would; REFERENCES
-- allows creating foreign keys, which is schema work.

GRANT SELECT, INSERT, UPDATE, DELETE
    ON ALL TABLES IN SCHEMA public
    TO wealthmap_app;

-- Every key in this schema is a Guid generated in C# (BaseEntity, version 7), so
-- there are no sequences to grant. If one is ever introduced, add:
--   GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO wealthmap_app;


-- -----------------------------------------------------------------------------
-- 4. Tables created by future migrations
-- -----------------------------------------------------------------------------
-- Grants are per table, so a table added next month would be invisible to the
-- application until someone remembered to grant it — a failure that shows up in
-- production as "permission denied for table X" long after the migration looked
-- successful.
--
-- Replace `neondb_owner` with the answer to SELECT current_user above. It must
-- be the migrating role: default privileges attach to the creator, not to the
-- grantee, so naming the wrong one means future tables are silently ungranted
-- and the failure surfaces in production as "permission denied for table X".

ALTER DEFAULT PRIVILEGES FOR ROLE neondb_owner IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO wealthmap_app;


-- -----------------------------------------------------------------------------
-- 5. Verify
-- -----------------------------------------------------------------------------
-- Expect: rolsuper, rolcreatedb, rolcreaterole all false.

SELECT rolname, rolsuper, rolcreatedb, rolcreaterole, rolbypassrls
FROM   pg_roles
WHERE  rolname = 'wealthmap_app';

-- Expect ZERO rows. This is the Neon-specific check: a role created through the
-- Neon console, CLI or API is granted membership in neon_superuser, which would
-- hand back everything this script withholds. A role created with CREATE ROLE in
-- SQL, as above, inherits nothing. If this returns neon_superuser, the role was
-- made in the console — drop it and run this file instead.

SELECT r.rolname AS member_of
FROM   pg_auth_members m
JOIN   pg_roles r ON r.oid = m.roleid
JOIN   pg_roles u ON u.oid = m.member
WHERE  u.rolname = 'wealthmap_app';

-- Expect: has_schema_privilege(..., 'CREATE') = false.

SELECT has_schema_privilege('wealthmap_app', 'public', 'USAGE')  AS can_use_schema,
       has_schema_privilege('wealthmap_app', 'public', 'CREATE') AS can_create_tables;

-- Expect one row per table, all four privileges true.

SELECT table_name,
       has_table_privilege('wealthmap_app', quote_ident(table_name), 'SELECT') AS can_select,
       has_table_privilege('wealthmap_app', quote_ident(table_name), 'INSERT') AS can_insert,
       has_table_privilege('wealthmap_app', quote_ident(table_name), 'UPDATE') AS can_update,
       has_table_privilege('wealthmap_app', quote_ident(table_name), 'DELETE') AS can_delete
FROM   information_schema.tables
WHERE  table_schema = 'public'
  AND  table_type = 'BASE TABLE'
ORDER  BY table_name;

-- A direct check that the restriction bites. Connected AS wealthmap_app, this
-- must fail with "permission denied for schema public":
--
--   CREATE TABLE should_not_exist (id int);
--
-- If it succeeds, the role still owns something it should not.
