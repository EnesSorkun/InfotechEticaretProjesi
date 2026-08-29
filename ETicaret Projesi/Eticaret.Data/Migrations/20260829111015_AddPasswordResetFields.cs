using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "AppUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpireDate",
                table: "AppUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "PasswordResetToken", "PasswordResetTokenExpireDate", "UserGuid" },
                values: new object[] { new DateTime(2026, 8, 29, 11, 10, 14, 639, DateTimeKind.Utc).AddTicks(6110), null, null, new Guid("fe62ef83-7e2f-41f0-ac80-5169a978ae42") });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpireDate",
                table: "AppUsers");

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
    }
}
