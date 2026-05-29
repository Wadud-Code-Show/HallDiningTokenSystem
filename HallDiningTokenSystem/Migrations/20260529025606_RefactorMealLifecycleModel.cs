using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HallDiningTokenSystem.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMealLifecycleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "MealSessions");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "MealSessions",
                newName: "TokenSellStartDateTime");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "MealSessions",
                newName: "TokenSellEndDateTime");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "MealSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "MealServingEndDateTime",
                table: "MealSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "MealServingStartDateTime",
                table: "MealSessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SoldCount",
                table: "MealSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "MealSessions");

            migrationBuilder.DropColumn(
                name: "MealServingEndDateTime",
                table: "MealSessions");

            migrationBuilder.DropColumn(
                name: "MealServingStartDateTime",
                table: "MealSessions");

            migrationBuilder.DropColumn(
                name: "SoldCount",
                table: "MealSessions");

            migrationBuilder.RenameColumn(
                name: "TokenSellStartDateTime",
                table: "MealSessions",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "TokenSellEndDateTime",
                table: "MealSessions",
                newName: "EndTime");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "MealSessions",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
