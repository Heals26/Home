using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseholdSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                schema: "home",
                table: "Household",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifxApiToken",
                schema: "home",
                table: "Household",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                schema: "home",
                table: "Household",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                schema: "home",
                table: "Household");

            migrationBuilder.DropColumn(
                name: "LifxApiToken",
                schema: "home",
                table: "Household");

            migrationBuilder.DropColumn(
                name: "Longitude",
                schema: "home",
                table: "Household");
        }
    }
}
