using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credit_cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    bank_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    annual_interest_rate = table.Column<decimal>(type: "numeric(6,3)", nullable: false),
                    payment_due_day = table.Column<int>(type: "integer", nullable: false),
                    statement_cutoff_day = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    credit_limit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    used_credit = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    used_credit_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_credit_cards", x => x.id);
                    table.CheckConstraint("ck_credit_cards_cutoff_day", "statement_cutoff_day BETWEEN 1 AND 31");
                    table.CheckConstraint("ck_credit_cards_due_day", "payment_due_day BETWEEN 1 AND 31");
                    table.CheckConstraint("ck_credit_cards_used_within_limit", "used_credit <= credit_limit");
                    table.ForeignKey(
                        name: "fk_credit_cards_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_credit_cards_user_id",
                table: "credit_cards",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_cards");
        }
    }
}
