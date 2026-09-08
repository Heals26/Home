using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuditHouseholdAndSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "HouseholdID",
                schema: "home",
                table: "Audit",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                schema: "home",
                table: "Audit",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            // Every existing row belongs to the household of the member who wrote it. Rows whose member
            // has since been removed stay unowned and never reach a feed.
            migrationBuilder.Sql(@"UPDATE a SET a.HouseholdID = u.HouseholdID
FROM [home].[Audit] a
INNER JOIN [home].[User] u ON u.UserID = a.UserID
WHERE a.HouseholdID IS NULL AND u.HouseholdID IS NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_HouseholdID_AuditID",
                schema: "home",
                table: "Audit",
                columns: new[] { "HouseholdID", "AuditID" });

            migrationBuilder.AddForeignKey(
                name: "FK_Audit_Household",
                schema: "home",
                table: "Audit",
                column: "HouseholdID",
                principalSchema: "home",
                principalTable: "Household",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Audit_Household",
                schema: "home",
                table: "Audit");

            migrationBuilder.DropIndex(
                name: "IX_Audit_HouseholdID_AuditID",
                schema: "home",
                table: "Audit");

            migrationBuilder.DropColumn(
                name: "HouseholdID",
                schema: "home",
                table: "Audit");

            migrationBuilder.DropColumn(
                name: "Summary",
                schema: "home",
                table: "Audit");
        }
    }
}
