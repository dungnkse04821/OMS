using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OMS.Migrations
{
    /// <inheritdoc />
    public partial class AddCarriers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Carriers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TrackingUrlPattern = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carriers", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Carriers",
                columns: new[] { "Id", "IsDeleted", "IsSystem", "Name", "SortOrder", "TrackingUrlPattern" },
                values: new object[,]
                {
                    { 1, false, true, "GHTK", 1, "https://i.ghtk.vn/{code}" },
                    { 2, false, true, "GHN", 2, "https://donhang.ghn.vn/?order_code={code}" },
                    { 3, false, true, "Viettel Post", 3, "https://viettelpost.com.vn/tra-cuu-hanh-trinh-don/?billCode={code}" },
                    { 4, false, true, "Vietnam Post", 4, "https://www.vnpost.vn/vi-vn/dinh-vi/buu-pham?key={code}" },
                    { 5, false, true, "J&T Express", 5, "https://jtexpress.vn/tracking?bills={code}" },
                    { 6, false, true, "Best Express", 6, "https://best-inc-vn.com/track?bills={code}" },
                    { 7, false, true, "Ninja Van", 7, "https://www.ninjavan.co/vi-vn/tracking?id={code}" },
                    { 8, false, true, "SPX Express", 8, "https://spx.vn/tracking/?trackingNumber={code}" },
                    { 9, false, true, "Lazada Logistics", 9, "https://www.lazada.vn/order/{code}/" },
                    { 10, false, true, "Tiki Now", 10, "https://tiki.vn/tracking?orderCode={code}" },
                    { 11, false, true, "Khác", 99, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Carriers");
        }
    }
}
