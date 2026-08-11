using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivingToAccountsAndCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                table: "credit_cards",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                table: "credit_cards",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "archived_at",
                table: "accounts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_archived",
                table: "accounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "archived_at",
                table: "credit_cards");

            migrationBuilder.DropColumn(
                name: "is_archived",
                table: "credit_cards");

            migrationBuilder.DropColumn(
                name: "archived_at",
                table: "accounts");

            migrationBuilder.DropColumn(
                name: "is_archived",
                table: "accounts");
        }
    }
}
