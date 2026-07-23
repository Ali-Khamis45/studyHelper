using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiStudyOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "reschedule_count",
                table: "daily_tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "ai_telemetry_events",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "analytics_insights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    weekly_summary = table.Column<string>(type: "text", nullable: false),
                    monthly_summary = table.Column<string>(type: "text", nullable: false),
                    strengths_json = table.Column<string>(type: "text", nullable: false),
                    weaknesses_json = table.Column<string>(type: "text", nullable: false),
                    recommended_focus_areas_json = table.Column<string>(type: "text", nullable: false),
                    suggested_schedule_improvements_json = table.Column<string>(type: "text", nullable: false),
                    risk_detection = table.Column<string>(type: "text", nullable: false),
                    model_used = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    prompt_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    generated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    invalidated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_analytics_insights", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "topic_mastery_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    topic = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    mastery_score = table.Column<double>(type: "double precision", nullable: false),
                    recorded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_topic_mastery_history", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_telemetry_events_user_id_created_at_utc",
                table: "ai_telemetry_events",
                columns: new[] { "user_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_analytics_insights_user_id_generated_at_utc",
                table: "analytics_insights",
                columns: new[] { "user_id", "generated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_topic_mastery_history_user_id_recorded_at_utc",
                table: "topic_mastery_history",
                columns: new[] { "user_id", "recorded_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analytics_insights");

            migrationBuilder.DropTable(
                name: "topic_mastery_history");

            migrationBuilder.DropIndex(
                name: "ix_ai_telemetry_events_user_id_created_at_utc",
                table: "ai_telemetry_events");

            migrationBuilder.DropColumn(
                name: "reschedule_count",
                table: "daily_tasks");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "ai_telemetry_events");
        }
    }
}
