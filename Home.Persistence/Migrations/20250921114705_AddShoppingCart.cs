using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShoppingCart : Migration
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
                defaultValue: new DateTime(2025, 9, 21, 11, 47, 5, 7, DateTimeKind.Utc).AddTicks(4980),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 9, 7, 12, 14, 25, 873, DateTimeKind.Utc).AddTicks(4692));

            migrationBuilder.CreateTable(
                name: "ShoppingCart",
                schema: "home",
                columns: table => new
                {
                    ShoppingCartID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCart", x => x.ShoppingCartID);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCartItem",
                schema: "home",
                columns: table => new
                {
                    ShoppingCartItemID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cost = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    InBasket = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Sequence = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    ShoppingListID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCartItem", x => x.ShoppingCartItemID);
                    table.ForeignKey(
                        name: "FK_ShoppingListItem_ShoppingList",
                        column: x => x.ShoppingListID,
                        principalSchema: "home",
                        principalTable: "ShoppingCart",
                        principalColumn: "ShoppingCartID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItem_ShoppingListID",
                schema: "home",
                table: "ShoppingCartItem",
                column: "ShoppingListID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingCartItem",
                schema: "home");

            migrationBuilder.DropTable(
                name: "ShoppingCart",
                schema: "home");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 9, 7, 12, 14, 25, 873, DateTimeKind.Utc).AddTicks(4692),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 9, 21, 11, 47, 5, 7, DateTimeKind.Utc).AddTicks(4980));
        }
    }
}
