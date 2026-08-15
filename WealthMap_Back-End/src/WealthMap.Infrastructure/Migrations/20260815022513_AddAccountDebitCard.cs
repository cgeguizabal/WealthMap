using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountDebitCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "debit_card_last_four",
                table: "accounts",
                type: "character(4)",
                fixedLength: true,
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "debit_card_type",
                table: "accounts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "ck_accounts_no_card_no_digits",
                table: "accounts",
                sql: "(debit_card_type <> 1) OR (debit_card_last_four IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_accounts_no_card_no_digits",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "debit_card_last_four",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "debit_card_type",
                table: "accounts");
        }
    }
}
