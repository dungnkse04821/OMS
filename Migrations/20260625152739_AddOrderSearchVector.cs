using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace OMS.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderSearchVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Orders",
                type: "tsvector",
                nullable: true)
                .Annotation("Npgsql:TsVectorConfig", "simple")
                .Annotation("Npgsql:TsVectorProperties", new[] { "Id", "Code", "ProductName", "CustomerName", "PhoneNumber", "TrackingCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SearchVector",
                table: "Orders",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_SearchVector",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Orders");
        }
    }
}
