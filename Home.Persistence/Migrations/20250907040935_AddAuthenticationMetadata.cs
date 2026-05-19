using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthenticationMetadata : Migration
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
                defaultValue: new DateTime(2025, 9, 7, 4, 9, 35, 512, DateTimeKind.Utc).AddTicks(3916),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 9, 3, 10, 30, 52, 343, DateTimeKind.Utc).AddTicks(9273));

            migrationBuilder.CreateTable(
                name: "AuthenticationMetadata",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthenticationMetadata",
                schema: "home");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 3, 10, 30, 52, 343, DateTimeKind.Utc).AddTicks(9273),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 9, 7, 4, 9, 35, 512, DateTimeKind.Utc).AddTicks(3916));
        }
    }
}
