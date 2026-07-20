using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiStudyOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTelemetryOperationalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "cached",
                table: "ai_telemetry_events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "ai_telemetry_events",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "circuit_breaker_state",
                table: "ai_telemetry_events",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "response_size_bytes",
                table: "ai_telemetry_events",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "stream",
                table: "ai_telemetry_events",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cached",
                table: "ai_telemetry_events");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "ai_telemetry_events");

            migrationBuilder.DropColumn(
                name: "circuit_breaker_state",
                table: "ai_telemetry_events");

            migrationBuilder.DropColumn(
                name: "response_size_bytes",
                table: "ai_telemetry_events");

            migrationBuilder.DropColumn(
                name: "stream",
                table: "ai_telemetry_events");
        }
    }
}
