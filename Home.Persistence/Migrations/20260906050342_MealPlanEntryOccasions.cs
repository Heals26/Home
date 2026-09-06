using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MealPlanEntryOccasions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "RecipeID",
                schema: "home",
                table: "MealPlanEntry",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "HouseholdID",
                schema: "home",
                table: "MealPlanEntry",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                schema: "home",
                table: "MealPlanEntry",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            // Every row that already exists got here through a recipe, which is where its household
            // has been living. Copy it across before the foreign key goes on, or each one points at
            // a household 0 that does not exist and the constraint is rejected.
            migrationBuilder.Sql(@"
                UPDATE  e
                SET     e.HouseholdID = r.HouseholdID
                FROM    home.MealPlanEntry e
                        INNER JOIN home.Recipe r ON r.RecipeID = e.RecipeID;");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlanEntry_HouseholdID",
                schema: "home",
                table: "MealPlanEntry",
                column: "HouseholdID");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlanEntry_Household",
                schema: "home",
                table: "MealPlanEntry",
                column: "HouseholdID",
                principalSchema: "home",
                principalTable: "Household",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlanEntry_Household",
                schema: "home",
                table: "MealPlanEntry");

            migrationBuilder.DropIndex(
                name: "IX_MealPlanEntry_HouseholdID",
                schema: "home",
                table: "MealPlanEntry");

            migrationBuilder.DropColumn(
                name: "HouseholdID",
                schema: "home",
                table: "MealPlanEntry");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "home",
                table: "MealPlanEntry");

            migrationBuilder.AlterColumn<long>(
                name: "RecipeID",
                schema: "home",
                table: "MealPlanEntry",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
