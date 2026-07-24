using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiStudyOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningRoadmaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "learning_roadmaps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    career_goal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    estimated_weeks = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    model_used = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    prompt_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_learning_roadmaps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roadmap_topics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    roadmap_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_topic_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    estimated_hours = table.Column<double>(type: "double precision", nullable: false),
                    difficulty = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    prerequisite_topic_ids_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    resources_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    suggested_projects_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    linked_mastery_topic = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    manually_completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roadmap_topics", x => x.id);
                    table.ForeignKey(
                        name: "fk_roadmap_topics_learning_roadmaps_roadmap_id",
                        column: x => x.roadmap_id,
                        principalTable: "learning_roadmaps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_learning_roadmaps_user_id_updated_at_utc",
                table: "learning_roadmaps",
                columns: new[] { "user_id", "updated_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_roadmap_topics_roadmap_id_parent_topic_id_order",
                table: "roadmap_topics",
                columns: new[] { "roadmap_id", "parent_topic_id", "order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "roadmap_topics");

            migrationBuilder.DropTable(
                name: "learning_roadmaps");
        }
    }
}
