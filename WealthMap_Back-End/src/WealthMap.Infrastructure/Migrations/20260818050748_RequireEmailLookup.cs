using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <summary>
    /// Locks in the blind index: NOT NULL and unique. Run only after the
    /// encryption pass has filled every row.
    /// </summary>
    /// <remarks>
    /// Third and last of the sequence. It exists separately from EncryptPiiColumns
    /// for one reason: a unique NOT NULL constraint cannot be added to a column
    /// that is empty, and the column is empty until the data runner fills it.
    ///
    /// This is also where the uniqueness of an email address comes back. Between
    /// the first migration and this one there is nothing stopping a duplicate
    /// registration at the database level — the application check in
    /// RegisterHandler still holds, but the guarantee is briefly the
    /// application's alone. Keep the window short.
    /// </remarks>
    public partial class RequireEmailLookup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Refuses to run before the data pass has filled the column.
            //
            // This replaces the AlterColumn EF generated, which carried
            // defaultValue: "" and would have quietly rewritten every unconverted
            // row to a blank blind index. Those users would then fail to sign in —
            // not with an error, but by appearing not to exist — and the second
            // blank row would collide on the unique index below. Stopping here
            // costs a re-run; the alternative costs accounts.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE missing bigint;
                BEGIN
                    SELECT count(*) INTO missing FROM users WHERE email_lookup IS NULL;

                    IF missing > 0 THEN
                        RAISE EXCEPTION
                            'email_lookup is still null for % user(s). Run the encryption pass first: dotnet run --project src/WealthMap.Api -- --encrypt-pii',
                            missing;
                    END IF;
                END $$;");

            migrationBuilder.Sql("ALTER TABLE users ALTER COLUMN email_lookup SET NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "ix_users_email_lookup",
                table: "users",
                column: "email_lookup",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_email_lookup",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "email_lookup",
                table: "users",
                type: "character(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character(64)",
                oldFixedLength: true,
                oldMaxLength: 64);
        }
    }
}
