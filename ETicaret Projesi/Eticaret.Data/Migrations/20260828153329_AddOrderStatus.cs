using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "UserGuid" },
                values: new object[] { new DateTime(2026, 8, 28, 15, 33, 28, 619, DateTimeKind.Utc).AddTicks(7580), new Guid("e4eac477-8b97-4f5f-b1f2-fb40fc83057c") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2026, 8, 28, 15, 33, 28, 620, DateTimeKind.Utc).AddTicks(4660));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2026, 8, 28, 15, 33, 28, 620, DateTimeKind.Utc).AddTicks(4770));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Orders");

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
    }
}
