using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<int>(type: "integer", nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<int>(type: "integer", nullable: false),
                    source_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payments", x => x.id);
                    table.CheckConstraint("ck_payments_source_account", "(source_type = 1 AND source_account_id IS NOT NULL) OR (source_type = 2 AND source_account_id IS NULL)");
                    table.ForeignKey(
                        name: "fk_payments_accounts_source_account_id",
                        column: x => x.source_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payments_source_account_id",
                table: "payments",
                column: "source_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_payments_target_type_target_id",
                table: "payments",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payments_user_id_occurred_at",
                table: "payments",
                columns: new[] { "user_id", "occurred_at" });

            // Backfill from the movement history. Without this, the monthly report's
            // "Paid" figures for past months would drop to zero the moment they start
            // reading from this table. Account-sourced payments are recoverable because
            // each left a Payment movement (type 7) whose related_entity_id names its
            // target; external payments left no trace and cannot be recovered — that
            // absence is exactly the gap this table closes going forward.
            migrationBuilder.Sql("""
                INSERT INTO payments (
                    id, user_id, target_type, target_id, amount, currency,
                    source_type, source_account_id, occurred_at, notes, created_at)
                SELECT
                    gen_random_uuid(),
                    m.user_id,
                    CASE
                        WHEN EXISTS (SELECT 1 FROM credit_cards c WHERE c.id = m.related_entity_id) THEN 1
                        WHEN EXISTS (SELECT 1 FROM debts d WHERE d.id = m.related_entity_id) THEN 2
                        ELSE 3
                    END,
                    m.related_entity_id,
                    m.amount,
                    m.currency,
                    1,
                    m.account_id,
                    m.occurred_at,
                    'Backfilled from movement history',
                    m.created_at
                FROM account_movements m
                WHERE m.type = 7
                  AND m.related_entity_id IS NOT NULL
                  AND (
                        EXISTS (SELECT 1 FROM credit_cards c WHERE c.id = m.related_entity_id)
                     OR EXISTS (SELECT 1 FROM debts d WHERE d.id = m.related_entity_id)
                     OR EXISTS (SELECT 1 FROM installment_purchases i WHERE i.id = m.related_entity_id)
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payments");
        }
    }
}
