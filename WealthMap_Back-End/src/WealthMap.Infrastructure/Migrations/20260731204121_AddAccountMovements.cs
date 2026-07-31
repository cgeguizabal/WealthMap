using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_movements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    balance_after = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    balance_after_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_account_movements", x => x.id);
                    table.ForeignKey(
                        name: "fk_account_movements_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_account_movements_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_account_movements_account_occurred",
                table: "account_movements",
                columns: new[] { "account_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "ix_account_movements_user_occurred",
                table: "account_movements",
                columns: new[] { "user_id", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_movements");
        }
    }
}
