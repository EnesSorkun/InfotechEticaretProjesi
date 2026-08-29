using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixOrderDetailColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "ProductImage",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "OrderDetails");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "UserGuid" },
                values: new object[] { new DateTime(2026, 8, 28, 14, 38, 53, 517, DateTimeKind.Utc).AddTicks(9900), new Guid("b6f6cbe8-a597-491d-bc07-e0458b0bc98d") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2026, 8, 28, 14, 38, 53, 518, DateTimeKind.Utc).AddTicks(7020));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2026, 8, 28, 14, 38, 53, 518, DateTimeKind.Utc).AddTicks(7130));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "OrderDetails",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductImage",
                table: "OrderDetails",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderDetails",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "OrderDetails",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "UserGuid" },
                values: new object[] { new DateTime(2026, 8, 28, 14, 19, 15, 583, DateTimeKind.Utc).AddTicks(6980), new Guid("30cbeef9-4ff5-4a9c-94d5-ecf830e01246") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2026, 8, 28, 14, 19, 15, 584, DateTimeKind.Utc).AddTicks(4400));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2026, 8, 28, 14, 19, 15, 584, DateTimeKind.Utc).AddTicks(4500));
        }
    }
}
