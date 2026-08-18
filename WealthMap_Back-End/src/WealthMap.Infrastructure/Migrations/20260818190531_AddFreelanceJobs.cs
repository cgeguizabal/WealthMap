using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFreelanceJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "freelance_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    client = table.Column<string>(type: "text", nullable: true),
                    due_on = table.Column<DateOnly>(type: "date", nullable: true),
                    delivered_on = table.Column<DateOnly>(type: "date", nullable: true),
                    paid_on = table.Column<DateOnly>(type: "date", nullable: true),
                    cancelled_on = table.Column<DateOnly>(type: "date", nullable: true),
                    deposit_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    agreed_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    amount_paid_currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_freelance_jobs", x => x.id);
                    table.CheckConstraint("ck_freelance_jobs_not_paid_and_cancelled", "(paid_on IS NULL) OR (cancelled_on IS NULL)");
                    table.CheckConstraint("ck_freelance_jobs_paid_together", "(paid_on IS NULL AND deposit_account_id IS NULL AND amount_paid = 0) OR (paid_on IS NOT NULL AND deposit_account_id IS NOT NULL AND amount_paid > 0)");
                    table.ForeignKey(
                        name: "fk_freelance_jobs_accounts_deposit_account_id",
                        column: x => x.deposit_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_freelance_jobs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_freelance_jobs_deposit_account_id",
                table: "freelance_jobs",
                column: "deposit_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_freelance_jobs_user_id",
                table: "freelance_jobs",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "freelance_jobs");
        }
    }
}
