using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <summary>
    /// Purely additive against a database that now holds a real family's data — no column is
    /// dropped. The board columns move from a shared global lookup to per-household rows, which
    /// needs its data moved in a fixed order; every other change is a new column or table.
    /// </summary>
    public partial class AddHouseholdBoardsMealSlotsRecipeDetailAndSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Sessions ────────────────────────────────────────────────────────────────
            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                schema: "home",
                table: "UserAuthentication",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DeviceLabel",
                schema: "home",
                table: "UserAuthentication",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresOnUTC",
                schema: "home",
                table: "UserAuthentication",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "SYSUTCDATETIME()");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedOnUTC",
                schema: "home",
                table: "UserAuthentication",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SupersededByAuthenticationMetadataID",
                schema: "home",
                table: "UserAuthentication",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupersededOnUTC",
                schema: "home",
                table: "UserAuthentication",
                type: "datetime2",
                nullable: true);

            // Unconditional: the column default stamps the migration time, which for an old row
            // is later than its DateSetUTC, so a conditional update would silently skip every row
            // and expire the family's live session — the exact bug this release exists to fix.
            // Dating expiry from issue instead means a session created today survives.
            migrationBuilder.Sql(@"
                UPDATE home.UserAuthentication
                SET ExpiresOnUTC = DATEADD(day, 90, DateSetUTC);");

            // ── Measurements (additive; the unitless columns stay until the move is proven) ──
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "home",
                table: "ShoppingListItem",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Unit",
                schema: "home",
                table: "ShoppingListItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Unit",
                schema: "home",
                table: "Ingredient",
                type: "bigint",
                nullable: true);

            // Weight beats volume beats a bare count. Unit values are MeasurementUnitSE:
            // 1 Pieces, 2 Grams, 4 Millilitres.
            migrationBuilder.Sql(@"
                UPDATE home.Ingredient
                SET Amount = COALESCE(Weight, Volume, Quantity),
                    Unit = CASE
                        WHEN Weight IS NOT NULL THEN 2
                        WHEN Volume IS NOT NULL THEN 4
                        ELSE 1 END
                WHERE Weight IS NOT NULL OR Volume IS NOT NULL OR Quantity IS NOT NULL;");

            migrationBuilder.Sql(@"
                UPDATE home.ShoppingListItem
                SET Amount = COALESCE(Weight, Volume, Quantity),
                    Unit = CASE
                        WHEN Weight IS NOT NULL THEN 2
                        WHEN Volume IS NOT NULL THEN 4
                        ELSE 1 END
                WHERE Weight IS NOT NULL OR Volume IS NOT NULL OR Quantity IS NOT NULL;");

            // ── Recipe detail ───────────────────────────────────────────────────────────
            migrationBuilder.AddColumn<long>(
                name: "Complexity",
                schema: "home",
                table: "Recipe",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CookMinutes",
                schema: "home",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "home",
                table: "Recipe",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrepMinutes",
                schema: "home",
                table: "Recipe",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Servings",
                schema: "home",
                table: "Recipe",
                type: "int",
                nullable: true);

            // ── Board: due time, then the columns move to their household ───────────────
            migrationBuilder.AddColumn<TimeSpan>(
                name: "DueTime",
                schema: "home",
                table: "Activity",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsComplete",
                schema: "home",
                table: "ActivityState",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                schema: "home",
                table: "ActivityState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Nullable first: the shared rows have no household, and a default of 0 would point
            // at a household that does not exist and fail the foreign key below.
            migrationBuilder.AddColumn<long>(
                name: "HouseholdID",
                schema: "home",
                table: "ActivityState",
                type: "bigint",
                nullable: true);

            // Software-process names on a family board. Renamed rather than replaced so every
            // card stays exactly where the family left it; spare columns can be deleted in the app.
            migrationBuilder.Sql(@"
                UPDATE home.ActivityState SET Name = 'To do'      WHERE HouseholdID IS NULL AND Name = 'Todo';
                UPDATE home.ActivityState SET Name = 'Planning'   WHERE HouseholdID IS NULL AND Name = 'Refining';
                UPDATE home.ActivityState SET Name = 'Doing'      WHERE HouseholdID IS NULL AND Name = 'Progressing';
                UPDATE home.ActivityState SET Name = 'Waiting on' WHERE HouseholdID IS NULL AND Name = 'Blocked';
                UPDATE home.ActivityState SET Name = 'Checking'   WHERE HouseholdID IS NULL AND Name = 'Testing';");

            // Order matters: clone per household, repoint that household's cards at its own
            // copy, and only then drop the shared originals.
            migrationBuilder.Sql(@"
                INSERT INTO home.ActivityState (Name, HouseholdID, Sequence, IsComplete)
                SELECT s.Name,
                       h.HouseholdID,
                       ROW_NUMBER() OVER (PARTITION BY h.HouseholdID ORDER BY s.ActivityStateID) - 1,
                       CASE WHEN s.Name = 'Done' THEN 1 ELSE 0 END
                FROM home.ActivityState s
                CROSS JOIN home.Household h
                WHERE s.HouseholdID IS NULL;");

            migrationBuilder.Sql(@"
                UPDATE a
                SET a.StateID = ns.ActivityStateID
                FROM home.Activity a
                INNER JOIN home.ActivityState os ON os.ActivityStateID = a.StateID AND os.HouseholdID IS NULL
                INNER JOIN home.ActivityState ns ON ns.HouseholdID = a.HouseholdID AND ns.Name = os.Name;");

            migrationBuilder.Sql("DELETE FROM home.ActivityState WHERE HouseholdID IS NULL;");

            migrationBuilder.AlterColumn<long>(
                name: "HouseholdID",
                schema: "home",
                table: "ActivityState",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            // ── Meals ───────────────────────────────────────────────────────────────────
            migrationBuilder.AddColumn<long>(
                name: "MealSlotID",
                schema: "home",
                table: "MealPlanEntry",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MealSlot",
                schema: "home",
                columns: table => new
                {
                    MealSlotID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    StartsAt = table.Column<TimeSpan>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealSlot", x => x.MealSlotID);
                    table.ForeignKey(
                        name: "FK_MealSlot_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                schema: "home",
                columns: table => new
                {
                    TagID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Colour = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.TagID);
                    table.ForeignKey(
                        name: "FK_Tag_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeMealSlot",
                schema: "home",
                columns: table => new
                {
                    MealSlotID = table.Column<long>(type: "bigint", nullable: false),
                    RecipeID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeMealSlot", x => new { x.RecipeID, x.MealSlotID });
                    table.ForeignKey(
                        name: "FK_RecipeMealSlot_MealSlot",
                        column: x => x.MealSlotID,
                        principalSchema: "home",
                        principalTable: "MealSlot",
                        principalColumn: "MealSlotID");
                    table.ForeignKey(
                        name: "FK_RecipeMealSlot_Recipe",
                        column: x => x.RecipeID,
                        principalSchema: "home",
                        principalTable: "Recipe",
                        principalColumn: "RecipeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityTag",
                schema: "home",
                columns: table => new
                {
                    ActivityID = table.Column<long>(type: "bigint", nullable: false),
                    TagID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityTag", x => new { x.ActivityID, x.TagID });
                    table.ForeignKey(
                        name: "FK_ActivityTag_Activity",
                        column: x => x.ActivityID,
                        principalSchema: "home",
                        principalTable: "Activity",
                        principalColumn: "ActivityID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityTag_Tag",
                        column: x => x.TagID,
                        principalSchema: "home",
                        principalTable: "Tag",
                        principalColumn: "TagID");
                });

            // Every existing household gets the same meal vocabulary a new one is seeded with.
            migrationBuilder.Sql(@"
                INSERT INTO home.MealSlot (HouseholdID, Name, Sequence, StartsAt)
                SELECT h.HouseholdID, v.Name, v.Sequence, v.StartsAt
                FROM home.Household h
                CROSS JOIN (VALUES
                    ('Breakfast', 0, CAST('07:00:00' AS time)),
                    ('Lunch',     1, CAST('12:00:00' AS time)),
                    ('Dinner',    2, CAST('18:00:00' AS time)),
                    ('Snack',     3, CAST('15:00:00' AS time))) AS v(Name, Sequence, StartsAt);");

            // Everything planned so far was planned as the evening meal.
            migrationBuilder.Sql(@"
                UPDATE e
                SET e.MealSlotID = s.MealSlotID
                FROM home.MealPlanEntry e
                INNER JOIN home.Recipe r ON r.RecipeID = e.RecipeID
                INNER JOIN home.MealSlot s ON s.HouseholdID = r.HouseholdID AND s.Name = 'Dinner';");

            // ── Indexes and keys ────────────────────────────────────────────────────────
            migrationBuilder.CreateIndex(
                name: "IX_UserAuthentication_ExpiresOnUTC",
                schema: "home",
                table: "UserAuthentication",
                column: "ExpiresOnUTC");

            migrationBuilder.CreateIndex(
                name: "IX_UserAuthentication_RefreshToken",
                schema: "home",
                table: "UserAuthentication",
                column: "RefreshToken");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntry_MealSlotID",
                schema: "home",
                table: "MealPlanEntry",
                column: "MealSlotID");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityState_HouseholdID",
                schema: "home",
                table: "ActivityState",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityTag_TagID",
                schema: "home",
                table: "ActivityTag",
                column: "TagID");

            migrationBuilder.CreateIndex(
                name: "IX_MealSlot_HouseholdID_Name",
                schema: "home",
                table: "MealSlot",
                columns: new[] { "HouseholdID", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeMealSlot_MealSlotID",
                schema: "home",
                table: "RecipeMealSlot",
                column: "MealSlotID");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_HouseholdID_Name",
                schema: "home",
                table: "Tag",
                columns: new[] { "HouseholdID", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityState_Household",
                schema: "home",
                table: "ActivityState",
                column: "HouseholdID",
                principalSchema: "home",
                principalTable: "Household",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntry_MealSlot",
                schema: "home",
                table: "MealPlanEntry",
                column: "MealSlotID",
                principalSchema: "home",
                principalTable: "MealSlot",
                principalColumn: "MealSlotID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityState_Household",
                schema: "home",
                table: "ActivityState");

            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntry_MealSlot",
                schema: "home",
                table: "MealPlanEntry");

            migrationBuilder.DropTable(
                name: "ActivityTag",
                schema: "home");

            migrationBuilder.DropTable(
                name: "RecipeMealSlot",
                schema: "home");

            migrationBuilder.DropTable(
                name: "Tag",
                schema: "home");

            migrationBuilder.DropTable(
                name: "MealSlot",
                schema: "home");

            migrationBuilder.DropIndex(
                name: "IX_UserAuthentication_ExpiresOnUTC",
                schema: "home",
                table: "UserAuthentication");

            migrationBuilder.DropIndex(
                name: "IX_UserAuthentication_RefreshToken",
                schema: "home",
                table: "UserAuthentication");

            migrationBuilder.DropIndex(
                name: "IX_MealPlanEntry_MealSlotID",
                schema: "home",
                table: "MealPlanEntry");

            migrationBuilder.DropIndex(
                name: "IX_ActivityState_HouseholdID",
                schema: "home",
                table: "ActivityState");

            migrationBuilder.DropColumn(
                name: "DeviceLabel",
                schema: "home",
                table: "UserAuthentication");

            migrationBuilder.DropColumn(
                name: "ExpiresOnUTC",
                schema: "home",
                table: "UserAuthentication");

            migrationBuilder.DropColumn(
                name: "LastUsedOnUTC",
                schema: "home",
                table: "UserAuthentication");

            migrationBuilder.DropColumn(
                name: "SupersededByAuthenticationMetadataID",
                schema: "home",
                table: "UserAuthentication");

            migrationBuilder.DropColumn(
                name: "SupersededOnUTC",
                schema: "home",
                table: "UserAuthentication");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "home",
                table: "ShoppingListItem");

            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "home",
                table: "ShoppingListItem");

            migrationBuilder.DropColumn(
                name: "Complexity",
                schema: "home",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "CookMinutes",
                schema: "home",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "home",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "PrepMinutes",
                schema: "home",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "Servings",
                schema: "home",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "MealSlotID",
                schema: "home",
                table: "MealPlanEntry");

            migrationBuilder.DropColumn(
                name: "Amount",
                schema: "home",
                table: "Ingredient");

            migrationBuilder.DropColumn(
                name: "Unit",
                schema: "home",
                table: "Ingredient");

            migrationBuilder.DropColumn(
                name: "HouseholdID",
                schema: "home",
                table: "ActivityState");

            migrationBuilder.DropColumn(
                name: "IsComplete",
                schema: "home",
                table: "ActivityState");

            migrationBuilder.DropColumn(
                name: "Sequence",
                schema: "home",
                table: "ActivityState");

            migrationBuilder.DropColumn(
                name: "DueTime",
                schema: "home",
                table: "Activity");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                schema: "home",
                table: "UserAuthentication",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
