using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLightCapabilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasColour",
                schema: "home",
                table: "Light",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasMatrix",
                schema: "home",
                table: "Light",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasMultizone",
                schema: "home",
                table: "Light",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasVariableColourTemp",
                schema: "home",
                table: "Light",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxKelvin",
                schema: "home",
                table: "Light",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinKelvin",
                schema: "home",
                table: "Light",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                schema: "home",
                table: "Light",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasColour",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "HasMatrix",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "HasMultizone",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "HasVariableColourTemp",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "MaxKelvin",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "MinKelvin",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "ProductName",
                schema: "home",
                table: "Light");

        }
    }
}
