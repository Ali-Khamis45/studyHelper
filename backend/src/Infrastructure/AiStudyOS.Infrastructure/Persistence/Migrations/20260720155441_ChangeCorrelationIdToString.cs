using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiStudyOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCorrelationIdToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "correlation_id",
                table: "ai_telemetry_events",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "correlation_id",
                table: "ai_telemetry_events",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
