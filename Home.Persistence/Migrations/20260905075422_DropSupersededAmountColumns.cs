using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropSupersededAmountColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "home",
                table: "ShoppingListItem");

            migrationBuilder.DropColumn(
                name: "Volume",
                schema: "home",
                table: "ShoppingListItem");

            migrationBuilder.DropColumn(
                name: "Weight",
                schema: "home",
                table: "ShoppingListItem");

            migrationBuilder.DropColumn(
                name: "Quantity",
                schema: "home",
                table: "Ingredient");

            migrationBuilder.DropColumn(
                name: "Volume",
                schema: "home",
                table: "Ingredient");

            migrationBuilder.DropColumn(
                name: "Weight",
                schema: "home",
                table: "Ingredient");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                schema: "home",
                table: "ShoppingListItem",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                schema: "home",
                table: "ShoppingListItem",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                schema: "home",
                table: "ShoppingListItem",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }
    }
}
