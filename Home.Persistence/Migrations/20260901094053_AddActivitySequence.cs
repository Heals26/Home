using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddActivitySequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                schema: "home",
                table: "Activity",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Leaving every card at 0 would make the order inside a column arbitrary again, which
            // is the thing this column exists to fix. Numbered per column by title, which is the
            // order the board already happened to show them in.
            migrationBuilder.Sql(@"
                WITH _Ordered AS
                (
                    SELECT
                        Sequence,
                        ROW_NUMBER() OVER (PARTITION BY HouseholdID, StateID ORDER BY Title, ActivityID) AS _Position
                    FROM home.Activity
                )
                UPDATE _Ordered SET Sequence = _Position;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sequence",
                schema: "home",
                table: "Activity");
        }
    }
}
