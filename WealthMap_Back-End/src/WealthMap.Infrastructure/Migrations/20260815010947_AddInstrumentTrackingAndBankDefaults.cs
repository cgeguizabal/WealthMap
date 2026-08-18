using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstrumentTrackingAndBankDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "last_four",
                table: "credit_cards",
                type: "character(4)",
                fixedLength: true,
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tracking_mode",
                table: "credit_cards",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "last_four",
                table: "accounts",
                type: "character(4)",
                fixedLength: true,
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "tracking_mode",
                table: "accounts",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "bank_defaults",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    direction = table.Column<int>(type: "integer", nullable: false),
                    default_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_defaults", x => x.id);
                    table.ForeignKey(
                        name: "fk_bank_defaults_accounts_default_account_id",
                        column: x => x.default_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_bank_defaults_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddCheckConstraint(
                name: "ck_credit_cards_sync_requires_last_four",
                table: "credit_cards",
                sql: "(tracking_mode = 1) OR (last_four IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_accounts_sync_requires_last_four",
                table: "accounts",
                sql: "(tracking_mode = 1) OR (last_four IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_bank_defaults_default_account_id",
                table: "bank_defaults",
                column: "default_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_defaults_user_id_bank_name_direction",
                table: "bank_defaults",
                columns: new[] { "user_id", "bank_name", "direction" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank_defaults");

            migrationBuilder.DropCheckConstraint(
                name: "ck_credit_cards_sync_requires_last_four",
                table: "credit_cards");

            migrationBuilder.DropCheckConstraint(
                name: "ck_accounts_sync_requires_last_four",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "last_four",
                table: "credit_cards");

            migrationBuilder.DropColumn(
                name: "tracking_mode",
                table: "credit_cards");

            migrationBuilder.DropColumn(
                name: "last_four",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "tracking_mode",
                table: "accounts");
        }
    }
}
