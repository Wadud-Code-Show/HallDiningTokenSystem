using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HallDiningTokenSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMealArchiveSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "MealSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "MealSessions");
        }
    }
}
