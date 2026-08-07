using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInstallments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "installment_purchases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    store_id = table.Column<Guid>(type: "uuid", nullable: true),
                    credit_card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    months_count = table.Column<int>(type: "integer", nullable: false),
                    purchased_at = table.Column<DateOnly>(type: "date", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_installment_purchases", x => x.id);
                    table.CheckConstraint("ck_installment_purchases_months", "months_count BETWEEN 1 AND 120");
                    table.ForeignKey(
                        name: "fk_installment_purchases_credit_cards_credit_card_id",
                        column: x => x.credit_card_id,
                        principalTable: "credit_cards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_installment_purchases_stores_store_id",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_installment_purchases_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "installment_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    installment_purchase_id = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_paid = table.Column<bool>(type: "boolean", nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_installment_payments", x => x.id);
                    table.CheckConstraint("ck_installment_payments_number", "number >= 1");
                    table.ForeignKey(
                        name: "fk_installment_payments_installment_purchases_installment_purc",
                        column: x => x.installment_purchase_id,
                        principalTable: "installment_purchases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_installment_payments_installment_purchase_id_number",
                table: "installment_payments",
                columns: new[] { "installment_purchase_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_installment_purchases_credit_card_id",
                table: "installment_purchases",
                column: "credit_card_id");

            migrationBuilder.CreateIndex(
                name: "ix_installment_purchases_store_id",
                table: "installment_purchases",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "ix_installment_purchases_user_id",
                table: "installment_purchases",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "installment_payments");

            migrationBuilder.DropTable(
                name: "installment_purchases");
        }
    }
}
