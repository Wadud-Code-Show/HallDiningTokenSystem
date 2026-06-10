using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HallDiningTokenSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRedemptionTrackingToMealToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RedeemedById",
                table: "MealTokens",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealTokens_RedeemedById",
                table: "MealTokens",
                column: "RedeemedById");

            migrationBuilder.AddForeignKey(
                name: "FK_MealTokens_AspNetUsers_RedeemedById",
                table: "MealTokens",
                column: "RedeemedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealTokens_AspNetUsers_RedeemedById",
                table: "MealTokens");

            migrationBuilder.DropIndex(
                name: "IX_MealTokens_RedeemedById",
                table: "MealTokens");

            migrationBuilder.DropColumn(
                name: "RedeemedById",
                table: "MealTokens");
        }
    }
}
