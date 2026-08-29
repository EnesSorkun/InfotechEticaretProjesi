using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Eticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsMain = table.Column<bool>(type: "boolean", nullable: false),
                    OrderNo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "UserGuid" },
                values: new object[] { new DateTime(2026, 8, 29, 13, 18, 29, 776, DateTimeKind.Utc).AddTicks(2550), new Guid("6fb79ae8-f4ba-48a8-8c64-c2e064da110f") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2026, 8, 29, 13, 18, 29, 776, DateTimeKind.Utc).AddTicks(9890));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2026, 8, 29, 13, 18, 29, 776, DateTimeKind.Utc).AddTicks(9990));

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "UserGuid" },
                values: new object[] { new DateTime(2026, 8, 29, 11, 10, 14, 639, DateTimeKind.Utc).AddTicks(6110), new Guid("fe62ef83-7e2f-41f0-ac80-5169a978ae42") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2026, 8, 29, 11, 10, 14, 640, DateTimeKind.Utc).AddTicks(3190));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2026, 8, 29, 11, 10, 14, 640, DateTimeKind.Utc).AddTicks(3330));
        }
    }
}
