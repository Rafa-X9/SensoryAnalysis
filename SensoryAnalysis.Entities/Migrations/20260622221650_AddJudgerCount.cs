using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SensoryAnalysis.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddJudgerCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JudgerCount",
                table: "Tests",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JudgerCount",
                table: "Tests");
        }
    }
}
