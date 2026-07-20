using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiStudyOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiTelemetryAndRecommendationMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                table: "planner_recommendations",
                newName: "generated_at");

            migrationBuilder.AddColumn<double>(
                name: "confidence_score",
                table: "planner_recommendations",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "expires_at",
                table: "planner_recommendations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "generation_time_ms",
                table: "planner_recommendations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTime>(
                name: "invalidated_at",
                table: "planner_recommendations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "prompt_version",
                table: "planner_recommendations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider",
                table: "planner_recommendations",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "recommendation_reason",
                table: "planner_recommendations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ai_telemetry_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    provider_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    prompt_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    prompt_tokens = table.Column<int>(type: "integer", nullable: false),
                    completion_tokens = table.Column<int>(type: "integer", nullable: false),
                    estimated_cost_usd = table.Column<decimal>(type: "numeric(12,6)", nullable: false),
                    latency_ms = table.Column<long>(type: "bigint", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    json_repair_count = table.Column<int>(type: "integer", nullable: false),
                    tool_call_count = table.Column<int>(type: "integer", nullable: false),
                    success = table.Column<bool>(type: "boolean", nullable: false),
                    error_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_telemetry_events", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_telemetry_events_agent_type_created_at_utc",
                table: "ai_telemetry_events",
                columns: new[] { "agent_type", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_ai_telemetry_events_correlation_id",
                table: "ai_telemetry_events",
                column: "correlation_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_telemetry_events");

            migrationBuilder.DropColumn(
                name: "confidence_score",
                table: "planner_recommendations");

            migrationBuilder.DropColumn(
                name: "expires_at",
                table: "planner_recommendations");

            migrationBuilder.DropColumn(
                name: "generation_time_ms",
                table: "planner_recommendations");

            migrationBuilder.DropColumn(
                name: "invalidated_at",
                table: "planner_recommendations");

            migrationBuilder.DropColumn(
                name: "prompt_version",
                table: "planner_recommendations");

            migrationBuilder.DropColumn(
                name: "provider",
                table: "planner_recommendations");

            migrationBuilder.DropColumn(
                name: "recommendation_reason",
                table: "planner_recommendations");

            migrationBuilder.RenameColumn(
                name: "generated_at",
                table: "planner_recommendations",
                newName: "created_at_utc");
        }
    }
}
