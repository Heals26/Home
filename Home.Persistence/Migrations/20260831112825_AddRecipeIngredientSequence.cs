using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeIngredientSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Sequence",
                schema: "home",
                table: "RecipeIngredient",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Every existing row would otherwise sit at 0 and the list would come back in whatever
            // order the database felt like — which is the very thing this column exists to stop.
            // IngredientID ascending is the order they were added, so a recipe keeps the order the
            // household actually wrote it in.
            migrationBuilder.Sql(@"
                WITH _Ordered AS
                (
                    SELECT
                        Sequence,
                        ROW_NUMBER() OVER (PARTITION BY RecipeID ORDER BY IngredientID) AS _Position
                    FROM home.RecipeIngredient
                )
                UPDATE _Ordered SET Sequence = _Position;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sequence",
                schema: "home",
                table: "RecipeIngredient");
        }
    }
}
