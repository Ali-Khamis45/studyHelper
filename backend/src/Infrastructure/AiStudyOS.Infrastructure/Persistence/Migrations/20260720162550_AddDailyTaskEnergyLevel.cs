using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AiStudyOS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyTaskEnergyLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "energy_level",
                table: "daily_tasks",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "energy_level",
                table: "daily_tasks");
        }
    }
}
