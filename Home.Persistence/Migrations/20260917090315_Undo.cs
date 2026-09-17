using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Undo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tag_HouseholdID_Name",
                schema: "home",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCategory_HouseholdID_Name",
                schema: "home",
                table: "ShoppingCategory");

            migrationBuilder.DropIndex(
                name: "IX_RecipeImage_RecipeID",
                schema: "home",
                table: "RecipeImage");

            migrationBuilder.DropIndex(
                name: "IX_MealSlot_HouseholdID_Name",
                schema: "home",
                table: "MealSlot");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "User",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Tag",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ShoppingListItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ShoppingList",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ShoppingItemPrice",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ShoppingCategory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeStep",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeNote",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeMealSlot",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeIngredient",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeImage",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Recipe",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "MealSlot",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "MealPlanEntry",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "LightSchedule",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "LightScene",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "LightGroup",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "IngredientNote",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Ingredient",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "CardSection",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "CalendarSubscription",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "CalendarEventMember",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "CalendarEvent",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Announcement",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ActivityTag",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ActivityState",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ActivityRegion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ActivityContent",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Activity",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UndoableAction",
                schema: "home",
                columns: table => new
                {
                    UndoableActionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CanUndo = table.Column<bool>(type: "bit", nullable: false),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UndoneOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UndoableAction", x => x.UndoableActionID);
                    table.ForeignKey(
                        name: "FK_UndoableAction_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tag_HouseholdID_Name",
                schema: "home",
                table: "Tag",
                columns: new[] { "HouseholdID", "Name" },
                unique: true,
                filter: "[DeletedOnUTC] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCategory_HouseholdID_Name",
                schema: "home",
                table: "ShoppingCategory",
                columns: new[] { "HouseholdID", "Name" },
                unique: true,
                filter: "[DeletedOnUTC] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeImage_RecipeID",
                schema: "home",
                table: "RecipeImage",
                column: "RecipeID",
                unique: true,
                filter: "[DeletedOnUTC] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MealSlot_HouseholdID_Name",
                schema: "home",
                table: "MealSlot",
                columns: new[] { "HouseholdID", "Name" },
                unique: true,
                filter: "[DeletedOnUTC] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UndoableAction_CreatedOnUTC",
                schema: "home",
                table: "UndoableAction",
                column: "CreatedOnUTC");

            migrationBuilder.CreateIndex(
                name: "IX_UndoableAction_HouseholdID",
                schema: "home",
                table: "UndoableAction",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_UndoableAction_Token",
                schema: "home",
                table: "UndoableAction",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UndoableAction",
                schema: "home");

            migrationBuilder.DropIndex(
                name: "IX_Tag_HouseholdID_Name",
                schema: "home",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingCategory_HouseholdID_Name",
                schema: "home",
                table: "ShoppingCategory");

            migrationBuilder.DropIndex(
                name: "IX_RecipeImage_RecipeID",
                schema: "home",
                table: "RecipeImage");

            migrationBuilder.DropIndex(
                name: "IX_MealSlot_HouseholdID_Name",
                schema: "home",
                table: "MealSlot");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "User");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ShoppingListItem");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ShoppingList");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ShoppingItemPrice");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ShoppingCategory");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeStep");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeNote");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeMealSlot");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeIngredient");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "RecipeImage");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Note");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "MealSlot");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "MealPlanEntry");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "LightSchedule");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "LightScene");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "LightGroup");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "IngredientNote");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Ingredient");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "CardSection");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "CalendarSubscription");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "CalendarEventMember");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "CalendarEvent");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Announcement");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ActivityTag");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ActivityState");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ActivityRegion");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "ActivityContent");

            migrationBuilder.DropColumn(
                name: "DeletedOnUTC",
                schema: "home",
                table: "Activity");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_HouseholdID_Name",
                schema: "home",
                table: "Tag",
                columns: new[] { "HouseholdID", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCategory_HouseholdID_Name",
                schema: "home",
                table: "ShoppingCategory",
                columns: new[] { "HouseholdID", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeImage_RecipeID",
                schema: "home",
                table: "RecipeImage",
                column: "RecipeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MealSlot_HouseholdID_Name",
                schema: "home",
                table: "MealSlot",
                columns: new[] { "HouseholdID", "Name" },
                unique: true);
        }
    }
}
