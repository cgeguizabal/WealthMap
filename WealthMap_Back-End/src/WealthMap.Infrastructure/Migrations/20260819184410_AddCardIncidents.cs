using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCardIncidents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "block_reason",
                table: "credit_cards",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "blocked_on",
                table: "credit_cards",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "debit_card_block_reason",
                table: "accounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "debit_card_blocked_on",
                table: "accounts",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "card_incidents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    card_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: false),
                    reported_on = table.Column<DateOnly>(type: "date", nullable: false),
                    last_four_at_report = table.Column<string>(type: "text", nullable: true),
                    replaced_on = table.Column<DateOnly>(type: "date", nullable: true),
                    new_last_four = table.Column<string>(type: "text", nullable: true),
                    recovered_on = table.Column<DateOnly>(type: "date", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_card_incidents", x => x.id);
                    table.CheckConstraint("ck_card_incidents_one_outcome", "(replaced_on IS NULL) OR (recovered_on IS NULL)");
                    table.CheckConstraint("ck_card_incidents_outcome_after_report", "(replaced_on IS NULL OR replaced_on >= reported_on) AND (recovered_on IS NULL OR recovered_on >= reported_on)");
                    table.ForeignKey(
                        name: "fk_card_incidents_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_card_incidents_user_id_kind_card_id",
                table: "card_incidents",
                columns: new[] { "user_id", "kind", "card_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_incidents");

            migrationBuilder.DropColumn(
                name: "block_reason",
                table: "credit_cards");

            migrationBuilder.DropColumn(
                name: "blocked_on",
                table: "credit_cards");

            migrationBuilder.DropColumn(
                name: "debit_card_block_reason",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "debit_card_blocked_on",
                table: "accounts");
        }
    }
}
