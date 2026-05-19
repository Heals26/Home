using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixClientApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiAuditEntry_ClientApplication_ClientApplicationID",
                schema: "home",
                table: "ApiAuditEntry");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 7, 12, 14, 25, 873, DateTimeKind.Utc).AddTicks(4692),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 9, 7, 4, 9, 35, 512, DateTimeKind.Utc).AddTicks(3916));

            migrationBuilder.AddForeignKey(
                name: "FK_ApiAuditEntry_ClientApplication",
                schema: "home",
                table: "ApiAuditEntry",
                column: "ClientApplicationID",
                principalSchema: "home",
                principalTable: "ClientApplication",
                principalColumn: "ClientApplicationID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiAuditEntry_ClientApplication",
                schema: "home",
                table: "ApiAuditEntry");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 7, 4, 9, 35, 512, DateTimeKind.Utc).AddTicks(3916),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 9, 7, 12, 14, 25, 873, DateTimeKind.Utc).AddTicks(4692));

            migrationBuilder.AddForeignKey(
                name: "FK_ApiAuditEntry_ClientApplication_ClientApplicationID",
                schema: "home",
                table: "ApiAuditEntry",
                column: "ClientApplicationID",
                principalSchema: "home",
                principalTable: "ClientApplication",
                principalColumn: "ClientApplicationID");
        }
    }
}
