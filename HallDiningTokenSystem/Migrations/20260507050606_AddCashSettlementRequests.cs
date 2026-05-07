using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HallDiningTokenSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCashSettlementRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashSettlementRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashAdminId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RequestNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByAdminId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CashSettlementId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashSettlementRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashSettlementRequest_CashAdmin",
                        column: x => x.CashAdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashSettlementRequest_CashSettlement",
                        column: x => x.CashSettlementId,
                        principalTable: "CashSettlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CashSettlementRequest_ReviewedByAdmin",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashSettlementRequests_CashAdminId",
                table: "CashSettlementRequests",
                column: "CashAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_CashSettlementRequests_CashSettlementId",
                table: "CashSettlementRequests",
                column: "CashSettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_CashSettlementRequests_ReviewedByAdminId",
                table: "CashSettlementRequests",
                column: "ReviewedByAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashSettlementRequests");
        }
    }
}
