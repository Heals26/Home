using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClientApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 3, 10, 30, 52, 343, DateTimeKind.Utc).AddTicks(9273),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 9, 3, 10, 16, 40, 762, DateTimeKind.Utc).AddTicks(4553));

            migrationBuilder.AddColumn<long>(
                name: "ClientApplicationID",
                schema: "home",
                table: "ApiAuditEntry",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientApplication",
                schema: "home",
                columns: table => new
                {
                    ClientApplicationID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientApplication", x => x.ClientApplicationID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiAuditEntry_ClientApplicationID",
                schema: "home",
                table: "ApiAuditEntry",
                column: "ClientApplicationID");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiAuditEntry_ClientApplication_ClientApplicationID",
                schema: "home",
                table: "ApiAuditEntry",
                column: "ClientApplicationID",
                principalSchema: "home",
                principalTable: "ClientApplication",
                principalColumn: "ClientApplicationID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiAuditEntry_ClientApplication_ClientApplicationID",
                schema: "home",
                table: "ApiAuditEntry");

            migrationBuilder.DropTable(
                name: "ClientApplication",
                schema: "home");

            migrationBuilder.DropIndex(
                name: "IX_ApiAuditEntry_ClientApplicationID",
                schema: "home",
                table: "ApiAuditEntry");

            migrationBuilder.DropColumn(
                name: "ClientApplicationID",
                schema: "home",
                table: "ApiAuditEntry");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 3, 10, 16, 40, 762, DateTimeKind.Utc).AddTicks(4553),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 9, 3, 10, 30, 52, 343, DateTimeKind.Utc).AddTicks(9273));
        }
    }
}
