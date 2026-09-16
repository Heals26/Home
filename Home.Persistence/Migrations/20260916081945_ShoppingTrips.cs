using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ShoppingTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ShoppingTripID",
                schema: "home",
                table: "ShoppingListItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShoppingTripID",
                schema: "home",
                table: "ShoppingItemPrice",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShoppingTrip",
                schema: "home",
                columns: table => new
                {
                    ShoppingTripID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EndedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastActivityOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShoppingListID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingTrip", x => x.ShoppingTripID);
                    table.ForeignKey(
                        name: "FK_ShoppingTrip_ShoppingList",
                        column: x => x.ShoppingListID,
                        principalSchema: "home",
                        principalTable: "ShoppingList",
                        principalColumn: "ShoppingListID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingItemPrice_ShoppingListItemID_ShoppingTripID",
                schema: "home",
                table: "ShoppingItemPrice",
                columns: new[] { "ShoppingListItemID", "ShoppingTripID" });

            migrationBuilder.CreateIndex(
                name: "IX_ShoppingTrip_ShoppingListID_EndedOnUTC",
                schema: "home",
                table: "ShoppingTrip",
                columns: new[] { "ShoppingListID", "EndedOnUTC" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShoppingTrip",
                schema: "home");

            migrationBuilder.DropIndex(
                name: "IX_ShoppingItemPrice_ShoppingListItemID_ShoppingTripID",
                schema: "home",
                table: "ShoppingItemPrice");

            migrationBuilder.DropColumn(
                name: "ShoppingTripID",
                schema: "home",
                table: "ShoppingListItem");

            migrationBuilder.DropColumn(
                name: "ShoppingTripID",
                schema: "home",
                table: "ShoppingItemPrice");
        }
    }
}
