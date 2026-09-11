using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ShoppingAislesAndPriceMemory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GroupByAisle",
                schema: "home",
                table: "ShoppingList",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ShoppingCategory",
                schema: "home",
                columns: table => new
                {
                    ShoppingCategoryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCategory", x => x.ShoppingCategoryID);
                    table.ForeignKey(
                        name: "FK_ShoppingCategory_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingItemMemory",
                schema: "home",
                columns: table => new
                {
                    ShoppingItemMemoryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShoppingCategoryID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingItemMemory", x => x.ShoppingItemMemoryID);
                    table.ForeignKey(
                        name: "FK_ShoppingItemMemory_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShoppingItemMemory_ShoppingCategory",
                        column: x => x.ShoppingCategoryID,
                        principalSchema: "home",
                        principalTable: "ShoppingCategory",
                        principalColumn: "ShoppingCategoryID");
                });

            migrationBuilder.CreateTable(
                name: "ShoppingItemPrice",
                schema: "home",
                columns: table => new
                {
                    ShoppingItemPriceID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    BoughtOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ShoppingItemMemoryID = table.Column<long>(type: "bigint", nullable: false),
                    ShoppingListItemID = table.Column<long>(type: "bigint", nullable: false),
                    Unit = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingItemPrice", x => x.ShoppingItemPriceID);
                    table.ForeignKey(
                        name: "FK_ShoppingItemPrice_ShoppingItemMemory",
                        column: x => x.ShoppingItemMemoryID,
                        principalSchema: "home",
                        principalTable: "ShoppingItemMemory",
                        principalColumn: "ShoppingItemMemoryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingCategory_HouseholdID_Name",
                schema: "home",
                table: "ShoppingCategory",
                columns: new[] { "HouseholdID", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingItemMemory_HouseholdID_NameKey",
                schema: "home",
                table: "ShoppingItemMemory",
                columns: new[] { "HouseholdID", "NameKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingItemMemory_ShoppingCategoryID",
                schema: "home",
                table: "ShoppingItemMemory",
                column: "ShoppingCategoryID");

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingItemPrice_ShoppingItemMemoryID_BoughtOnUTC",
                schema: "home",
                table: "ShoppingItemPrice",
                columns: new[] { "ShoppingItemMemoryID", "BoughtOnUTC" });

            // The same names HouseholdSetupLogic gives a new household, for every household that
            // already exists. The two lists must stay in step.
            migrationBuilder.Sql(@"
                INSERT INTO home.ShoppingCategory (Name, HouseholdID, Sequence)
                SELECT v.Name, h.HouseholdID, v.Sequence
                FROM home.Household h
                CROSS JOIN (VALUES
                    ('Fruit and veg', 0),
                    ('Dairy', 1),
                    ('Meat', 2),
                    ('Bakery', 3),
                    ('Frozen', 4),
                    ('Pantry', 5),
                    ('Drinks', 6),
                    ('Household', 7)) AS v(Name, Sequence);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingItemPrice",
                schema: "home");

            migrationBuilder.DropTable(
                name: "ShoppingItemMemory",
                schema: "home");

            migrationBuilder.DropTable(
                name: "ShoppingCategory",
                schema: "home");

            migrationBuilder.DropColumn(
                name: "GroupByAisle",
                schema: "home",
                table: "ShoppingList");
        }
    }
}
