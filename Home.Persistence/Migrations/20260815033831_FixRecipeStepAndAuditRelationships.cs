using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixRecipeStepAndAuditRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Audit_User_UserID",
                schema: "home",
                table: "Audit");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeStep_Recipe_RecipeID",
                schema: "home",
                table: "RecipeStep");

            // A step belonging to no recipe can never be reached or displayed, and the scaffolded
            // default would have pointed it at recipe 0, which does not exist — failing the key
            // below. There are none in the live database; this covers every other environment.
            migrationBuilder.Sql("DELETE FROM home.RecipeStep WHERE RecipeID IS NULL;");

            migrationBuilder.AlterColumn<long>(
                name: "RecipeID",
                schema: "home",
                table: "RecipeStep",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Audit_User",
                schema: "home",
                table: "Audit",
                column: "UserID",
                principalSchema: "home",
                principalTable: "User",
                principalColumn: "UserID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeStep_Recipe",
                schema: "home",
                table: "RecipeStep",
                column: "RecipeID",
                principalSchema: "home",
                principalTable: "Recipe",
                principalColumn: "RecipeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Audit_User",
                schema: "home",
                table: "Audit");

            migrationBuilder.DropForeignKey(
                name: "FK_RecipeStep_Recipe",
                schema: "home",
                table: "RecipeStep");

            migrationBuilder.AlterColumn<long>(
                name: "RecipeID",
                schema: "home",
                table: "RecipeStep",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 8, 15, 2, 10, 47, 390, DateTimeKind.Utc).AddTicks(4811),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 8, 15, 3, 38, 30, 668, DateTimeKind.Utc).AddTicks(9131));

            migrationBuilder.AddForeignKey(
                name: "FK_Audit_User_UserID",
                schema: "home",
                table: "Audit",
                column: "UserID",
                principalSchema: "home",
                principalTable: "User",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeStep_Recipe_RecipeID",
                schema: "home",
                table: "RecipeStep",
                column: "RecipeID",
                principalSchema: "home",
                principalTable: "Recipe",
                principalColumn: "RecipeID");
        }
    }
}
