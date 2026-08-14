using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMealPlanAnnouncementsAndSunSchedules : Migration
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
                defaultValue: new DateTime(2026, 8, 14, 11, 2, 20, 425, DateTimeKind.Utc).AddTicks(2714),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 8, 13, 10, 35, 11, 37, DateTimeKind.Utc).AddTicks(3956));

            migrationBuilder.AddColumn<int>(
                name: "OffsetMinutes",
                schema: "home",
                table: "LightSchedule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Trigger",
                schema: "home",
                table: "LightSchedule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Announcement",
                schema: "home",
                columns: table => new
                {
                    AnnouncementID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Announcement", x => x.AnnouncementID);
                    table.ForeignKey(
                        name: "FK_Announcement_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealPlanEntry",
                schema: "home",
                columns: table => new
                {
                    MealPlanEntryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecipeID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealPlanEntry", x => x.MealPlanEntryID);
                    table.ForeignKey(
                        name: "FK_MealPlanEntry_Recipe",
                        column: x => x.RecipeID,
                        principalSchema: "home",
                        principalTable: "Recipe",
                        principalColumn: "RecipeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Announcement_HouseholdID",
                schema: "home",
                table: "Announcement",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntry_Date",
                schema: "home",
                table: "MealPlanEntry",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntry_RecipeID",
                schema: "home",
                table: "MealPlanEntry",
                column: "RecipeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Announcement",
                schema: "home");

            migrationBuilder.DropTable(
                name: "MealPlanEntry",
                schema: "home");

            migrationBuilder.DropColumn(
                name: "OffsetMinutes",
                schema: "home",
                table: "LightSchedule");

            migrationBuilder.DropColumn(
                name: "Trigger",
                schema: "home",
                table: "LightSchedule");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 8, 13, 10, 35, 11, 37, DateTimeKind.Utc).AddTicks(3956),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 8, 14, 11, 2, 20, 425, DateTimeKind.Utc).AddTicks(2714));
        }
    }
}
