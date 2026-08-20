using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipePhotosAndPreviousLook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ImageUpdatedOnUTC",
                schema: "home",
                table: "Recipe",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 8, 15, 3, 38, 30, 668, DateTimeKind.Utc).AddTicks(9131));

            migrationBuilder.AddColumn<bool>(
                name: "IsPreviousLook",
                schema: "home",
                table: "LightScene",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RecipeImage",
                schema: "home",
                columns: table => new
                {
                    RecipeImageID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecipeID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeImage", x => x.RecipeImageID);
                    table.ForeignKey(
                        name: "FK_RecipeImage_Recipe",
                        column: x => x.RecipeID,
                        principalSchema: "home",
                        principalTable: "Recipe",
                        principalColumn: "RecipeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeImage_RecipeID",
                schema: "home",
                table: "RecipeImage",
                column: "RecipeID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecipeImage",
                schema: "home");

            migrationBuilder.DropColumn(
                name: "ImageUpdatedOnUTC",
                schema: "home",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "IsPreviousLook",
                schema: "home",
                table: "LightScene");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 8, 15, 3, 38, 30, 668, DateTimeKind.Utc).AddTicks(9131),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "SYSUTCDATETIME()");
        }
    }
}
