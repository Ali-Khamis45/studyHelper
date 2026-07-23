using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiStudyOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMentorModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mentor_conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: false),
                    message_count = table.Column<int>(type: "integer", nullable: false),
                    total_prompt_tokens = table.Column<int>(type: "integer", nullable: false),
                    total_completion_tokens = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_message_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mentor_conversations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mentor_memory_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    topic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    content = table.Column<string>(type: "text", nullable: false),
                    salience = table.Column<double>(type: "double precision", nullable: false),
                    source_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mentor_memory_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mentor_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    agent_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    model_used = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    prompt_tokens = table.Column<int>(type: "integer", nullable: true),
                    completion_tokens = table.Column<int>(type: "integer", nullable: true),
                    correlation_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mentor_messages", x => x.id);
                    table.ForeignKey(
                        name: "fk_mentor_messages_mentor_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "mentor_conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mentor_conversations_user_id_is_pinned",
                table: "mentor_conversations",
                columns: new[] { "user_id", "is_pinned" });

            migrationBuilder.CreateIndex(
                name: "ix_mentor_conversations_user_id_last_message_at_utc",
                table: "mentor_conversations",
                columns: new[] { "user_id", "last_message_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_mentor_memory_records_user_id_type_salience",
                table: "mentor_memory_records",
                columns: new[] { "user_id", "type", "salience" });

            migrationBuilder.CreateIndex(
                name: "ix_mentor_messages_conversation_id_created_at_utc",
                table: "mentor_messages",
                columns: new[] { "conversation_id", "created_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mentor_memory_records");

            migrationBuilder.DropTable(
                name: "mentor_messages");

            migrationBuilder.DropTable(
                name: "mentor_conversations");
        }
    }
}
