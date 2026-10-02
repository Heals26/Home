using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Feedback",
                schema: "home",
                columns: table => new
                {
                    FeedbackID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeviceLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Page = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ScreenHeight = table.Column<int>(type: "int", nullable: true),
                    ScreenWidth = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.FeedbackID);
                    table.ForeignKey(
                        name: "FK_Feedback_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_User",
                        column: x => x.UserID,
                        principalSchema: "home",
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_HouseholdID",
                schema: "home",
                table: "Feedback",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_UserID",
                schema: "home",
                table: "Feedback",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Feedback",
                schema: "home");
        }
    }
}
