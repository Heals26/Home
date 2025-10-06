using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DomainAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationMetadata",
                schema: "home");

            migrationBuilder.AddColumn<long>(
                name: "FK_ShoppingCart_User",
                schema: "home",
                table: "ShoppingCart",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 1, 2, 48, 403, DateTimeKind.Utc).AddTicks(8210),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 9, 21, 11, 47, 5, 7, DateTimeKind.Utc).AddTicks(4980));

            migrationBuilder.CreateTable(
                name: "UserAuthentication",
                schema: "home",
                columns: table => new
                {
                    AuthenticationMetadataID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateSetUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scopes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientApplicationID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: true),
                    ClientApplictionID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAuthentication", x => x.AuthenticationMetadataID);
                    table.ForeignKey(
                        name: "FK_AuthenticationMetadata_ClientApplication",
                        column: x => x.ClientApplicationID,
                        principalSchema: "home",
                        principalTable: "ClientApplication",
                        principalColumn: "ClientApplicationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthenticationMetadata_User",
                        column: x => x.UserID,
                        principalSchema: "home",
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCart_FK_ShoppingCart_User",
                schema: "home",
                table: "ShoppingCart",
                column: "FK_ShoppingCart_User");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuthentication_ClientApplicationID",
                schema: "home",
                table: "UserAuthentication",
                column: "ClientApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuthentication_UserID",
                schema: "home",
                table: "UserAuthentication",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ShoppingCart_User",
                schema: "home",
                table: "ShoppingCart",
                column: "FK_ShoppingCart_User",
                principalSchema: "home",
                principalTable: "User",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShoppingCart_User",
                schema: "home",
                table: "ShoppingCart");

            migrationBuilder.DropTable(
                name: "UserAuthentication",
                schema: "home");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCart_FK_ShoppingCart_User",
                schema: "home",
                table: "ShoppingCart");

            migrationBuilder.DropColumn(
                name: "FK_ShoppingCart_User",
                schema: "home",
                table: "ShoppingCart");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 21, 11, 47, 5, 7, DateTimeKind.Utc).AddTicks(4980),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 6, 1, 2, 48, 403, DateTimeKind.Utc).AddTicks(8210));

            migrationBuilder.CreateTable(
                name: "AuthenticationMetadata",
                schema: "home",
                columns: table => new
                {
                    AuthenticationMetadataID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientApplicationID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientApplictionID = table.Column<long>(type: "bigint", nullable: true),
                    DateSetUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scopes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationMetadata", x => x.AuthenticationMetadataID);
                    table.ForeignKey(
                        name: "FK_AuthenticationMetadata_ClientApplication",
                        column: x => x.ClientApplicationID,
                        principalSchema: "home",
                        principalTable: "ClientApplication",
                        principalColumn: "ClientApplicationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthenticationMetadata_User",
                        column: x => x.UserID,
                        principalSchema: "home",
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationMetadata_ClientApplicationID",
                schema: "home",
                table: "AuthenticationMetadata",
                column: "ClientApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_AuthenticationMetadata_UserID",
                schema: "home",
                table: "AuthenticationMetadata",
                column: "UserID");
        }
    }
}
