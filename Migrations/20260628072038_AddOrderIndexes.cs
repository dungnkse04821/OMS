using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OMS.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingCarrier",
                table: "Orders",
                column: "ShippingCarrier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ShippingCarrier",
                table: "Orders");
        }
    }
}
