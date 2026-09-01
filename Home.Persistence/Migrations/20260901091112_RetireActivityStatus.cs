using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RetireActivityStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_Status",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropTable(
                name: "ActivityStatus",
                schema: "home");

            migrationBuilder.DropIndex(
                name: "IX_Activity_StatusID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropColumn(
                name: "StatusID",
                schema: "home",
                table: "Activity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StatusID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ActivityStatus",
                schema: "home",
                columns: table => new
                {
                    ActivityStatusID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityStatus", x => x.ActivityStatusID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activity_StatusID",
                schema: "home",
                table: "Activity",
                column: "StatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_Status",
                schema: "home",
                table: "Activity",
                column: "StatusID",
                principalSchema: "home",
                principalTable: "ActivityStatus",
                principalColumn: "ActivityStatusID");
        }
    }
}
