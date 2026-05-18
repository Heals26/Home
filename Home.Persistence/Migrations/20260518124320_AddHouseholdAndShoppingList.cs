using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseholdAndShoppingList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingCartItem",
                schema: "home");

            migrationBuilder.DropTable(
                name: "ShoppingCart",
                schema: "home");

            migrationBuilder.DropColumn(
                name: "Volumne",
                schema: "home",
                table: "Ingredient");

            migrationBuilder.AddColumn<long>(
                name: "HouseholdID",
                schema: "home",
                table: "User",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                schema: "home",
                table: "Recipe",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 18, 12, 43, 19, 936, DateTimeKind.Utc).AddTicks(9574),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 10, 6, 1, 2, 48, 403, DateTimeKind.Utc).AddTicks(8210));

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Household",
                schema: "home",
                columns: table => new
                {
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Household", x => x.HouseholdID);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingList",
                schema: "home",
                columns: table => new
                {
                    ShoppingListID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingList", x => x.ShoppingListID);
                    table.ForeignKey(
                        name: "FK_ShoppingList_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingListItem",
                schema: "home",
                columns: table => new
                {
                    ShoppingListItemID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    InBasket = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Sequence = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    Volume = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ShoppingListID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingListItem", x => x.ShoppingListItemID);
                    table.ForeignKey(
                        name: "FK_ShoppingListItem_ShoppingList",
                        column: x => x.ShoppingListID,
                        principalSchema: "home",
                        principalTable: "ShoppingList",
                        principalColumn: "ShoppingListID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_HouseholdID",
                schema: "home",
                table: "User",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingList_HouseholdID",
                schema: "home",
                table: "ShoppingList",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingListItem_ShoppingListID",
                schema: "home",
                table: "ShoppingListItem",
                column: "ShoppingListID");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Household",
                schema: "home",
                table: "User",
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
                name: "FK_User_Household",
                schema: "home",
                table: "User");

            migrationBuilder.DropTable(
                name: "ShoppingListItem",
                schema: "home");

            migrationBuilder.DropTable(
                name: "ShoppingList",
                schema: "home");

            migrationBuilder.DropTable(
                name: "Household",
                schema: "home");

            migrationBuilder.DropIndex(
                name: "IX_User_HouseholdID",
                schema: "home",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HouseholdID",
                schema: "home",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Volume",
                schema: "home",
                table: "Ingredient");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                schema: "home",
                table: "Recipe",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 10, 6, 1, 2, 48, 403, DateTimeKind.Utc).AddTicks(8210),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 18, 12, 43, 19, 936, DateTimeKind.Utc).AddTicks(9574));

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Volumne",
                schema: "home",
                table: "Ingredient",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShoppingCart",
                schema: "home",
                columns: table => new
                {
                    ShoppingCartID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK_ShoppingCart_User = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCart", x => x.ShoppingCartID);
                    table.ForeignKey(
                        name: "FK_ShoppingCart_User",
                        column: x => x.FK_ShoppingCart_User,
                        principalSchema: "home",
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCartItem",
                schema: "home",
                columns: table => new
                {
                    ShoppingCartItemID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShoppingListID = table.Column<long>(type: "bigint", nullable: false),
                    Cost = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    InBasket = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Sequence = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L)
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
                name: "IX_ShoppingCart_FK_ShoppingCart_User",
                schema: "home",
                table: "ShoppingCart",
                column: "FK_ShoppingCart_User");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCartItem_ShoppingListID",
                schema: "home",
                table: "ShoppingCartItem",
                column: "ShoppingListID");
        }
    }
}
