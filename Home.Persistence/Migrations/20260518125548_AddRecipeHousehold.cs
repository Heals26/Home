using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeHousehold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "HouseholdID",
                schema: "home",
                table: "Recipe",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 12, 55, 47, 738, DateTimeKind.Utc).AddTicks(6489),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 18, 12, 43, 19, 936, DateTimeKind.Utc).AddTicks(9574));

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_HouseholdID",
                schema: "home",
                table: "Recipe",
                column: "HouseholdID");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipe_Household",
                schema: "home",
                table: "Recipe",
                column: "HouseholdID",
                principalSchema: "home",
                principalTable: "Household",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipe_Household",
                schema: "home",
                table: "Recipe");

            migrationBuilder.DropIndex(
                name: "IX_Recipe_HouseholdID",
                schema: "home",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "HouseholdID",
                schema: "home",
                table: "Recipe");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 12, 43, 19, 936, DateTimeKind.Utc).AddTicks(9574),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 18, 12, 55, 47, 738, DateTimeKind.Utc).AddTicks(6489));
        }
    }
}
