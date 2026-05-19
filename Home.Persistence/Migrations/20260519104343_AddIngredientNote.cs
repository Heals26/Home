using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIngredientNote : Migration
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
                defaultValue: new DateTime(2026, 5, 19, 10, 43, 42, 443, DateTimeKind.Utc).AddTicks(7419),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 18, 12, 55, 47, 738, DateTimeKind.Utc).AddTicks(6489));

            migrationBuilder.CreateTable(
                name: "IngredientNote",
                schema: "home",
                columns: table => new
                {
                    NoteID = table.Column<long>(type: "bigint", nullable: false),
                    IngredientID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IngredientNote", x => new { x.NoteID, x.IngredientID });
                    table.ForeignKey(
                        name: "FK_IngredientNote_Ingredient",
                        column: x => x.IngredientID,
                        principalSchema: "home",
                        principalTable: "Ingredient",
                        principalColumn: "IngredientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IngredientNote_Note",
                        column: x => x.NoteID,
                        principalSchema: "home",
                        principalTable: "Note",
                        principalColumn: "NoteID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IngredientNote_IngredientID",
                schema: "home",
                table: "IngredientNote",
                column: "IngredientID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IngredientNote",
                schema: "home");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 12, 55, 47, 738, DateTimeKind.Utc).AddTicks(6489),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 19, 10, 43, 42, 443, DateTimeKind.Utc).AddTicks(7419));
        }
    }
}
