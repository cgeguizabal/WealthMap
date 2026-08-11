using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WealthMap.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "{}" and not the generated "": an empty string is not valid JSON, so
            // Postgres rejects it as a jsonb default and the migration fails. Rows
            // raised before this column existed keep no parts and fall back to the
            // English they were stored with.
            migrationBuilder.AddColumn<string>(
                name: "params",
                table: "notifications",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "params",
                table: "notifications");
        }
    }
}
