using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseholdCardSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardSection",
                schema: "home",
                columns: table => new
                {
                    CardSectionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardSection", x => x.CardSectionID);
                    table.ForeignKey(
                        name: "FK_CardSection_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CardSection_HouseholdID",
                schema: "home",
                table: "CardSection",
                column: "HouseholdID");

            // Every existing household gets the three sections it already had, in the words a
            // family uses rather than the ones a ticket does. These names must match
            // HouseholdSetupLogic's defaults, which is what new households are seeded with.
            migrationBuilder.Sql(@"
                INSERT INTO home.CardSection (HouseholdID, Name, Sequence)
                SELECT h.HouseholdID, s.Name, s.Sequence
                FROM home.Household h
                CROSS JOIN (VALUES ('Details', 0), ('Steps', 1), ('Notes', 2)) AS s(Name, Sequence);");

            // The column holds the old RegionSE ordinal (1 Description, 2 AcceptanceCriteria,
            // 3 Notes). Renaming it keeps those numbers, so they are translated into real
            // CardSection keys immediately afterwards — reached through the card's household, so a
            // row can only ever land on its own household's section.
            migrationBuilder.RenameColumn(
                name: "Region",
                schema: "home",
                table: "ActivityRegion",
                newName: "CardSectionID");

            migrationBuilder.Sql(@"
                UPDATE ar
                SET ar.CardSectionID = cs.CardSectionID
                FROM home.ActivityRegion ar
                INNER JOIN home.Activity a ON a.ActivityID = ar.ActivityID
                INNER JOIN home.CardSection cs
                    ON cs.HouseholdID = a.HouseholdID
                    AND cs.Name = CASE ar.CardSectionID
                        WHEN 1 THEN 'Details'
                        WHEN 2 THEN 'Steps'
                        ELSE 'Notes'
                    END;");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRegion_CardSectionID",
                schema: "home",
                table: "ActivityRegion",
                column: "CardSectionID");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityRegion_CardSection",
                schema: "home",
                table: "ActivityRegion",
                column: "CardSectionID",
                principalSchema: "home",
                principalTable: "CardSection",
                principalColumn: "CardSectionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityRegion_CardSection",
                schema: "home",
                table: "ActivityRegion");

            migrationBuilder.DropTable(
                name: "CardSection",
                schema: "home");

            migrationBuilder.DropIndex(
                name: "IX_ActivityRegion_CardSectionID",
                schema: "home",
                table: "ActivityRegion");

            migrationBuilder.RenameColumn(
                name: "CardSectionID",
                schema: "home",
                table: "ActivityRegion",
                newName: "Region");
        }
    }
}
