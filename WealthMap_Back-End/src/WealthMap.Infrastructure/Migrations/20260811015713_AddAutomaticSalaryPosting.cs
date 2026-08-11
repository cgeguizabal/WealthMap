using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomaticSalaryPosting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "salary_posting_starts_on",
                table: "jobs",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            // Existing jobs begin posting from today, never from their creation date.
            // Without this they would inherit the 0001-01-01 default, every payday in
            // the catch-up window would look unpaid, and the first run would dump
            // roughly a year of back-salary into the deposit account.
            migrationBuilder.Sql("UPDATE jobs SET salary_posting_starts_on = CURRENT_DATE;");

            migrationBuilder.CreateTable(
                name: "salary_deposits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scheduled_date = table.Column<DateOnly>(type: "date", nullable: false),
                    posted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    account_movement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_salary_deposits", x => x.id);
                    table.ForeignKey(
                        name: "fk_salary_deposits_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_salary_deposits_job_id_scheduled_date",
                table: "salary_deposits",
                columns: new[] { "job_id", "scheduled_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_salary_deposits_user_id",
                table: "salary_deposits",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "salary_deposits");

            migrationBuilder.DropColumn(
                name: "salary_posting_starts_on",
                table: "jobs");
        }
    }
}
