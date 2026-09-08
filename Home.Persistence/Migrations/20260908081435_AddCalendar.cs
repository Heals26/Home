using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarSubscription",
                schema: "home",
                columns: table => new
                {
                    CalendarSubscriptionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LastError = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastFetchedUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSubscription", x => x.CalendarSubscriptionID);
                    table.ForeignKey(
                        name: "FK_CalendarSubscription_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEvent",
                schema: "home",
                columns: table => new
                {
                    CalendarEventID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DaysOfWeek = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ExternalUID = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    IsAllDay = table.Column<bool>(type: "bit", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RepeatsOnWeekdayOfMonth = table.Column<bool>(type: "bit", nullable: false),
                    RepeatUntil = table.Column<DateOnly>(type: "date", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    TimeZoneID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false),
                    SubscriptionID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEvent", x => x.CalendarEventID);
                    table.ForeignKey(
                        name: "FK_CalendarEvent_CalendarSubscription",
                        column: x => x.SubscriptionID,
                        principalSchema: "home",
                        principalTable: "CalendarSubscription",
                        principalColumn: "CalendarSubscriptionID");
                    table.ForeignKey(
                        name: "FK_CalendarEvent_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEventException",
                schema: "home",
                columns: table => new
                {
                    CalendarEventExceptionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccurrenceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CalendarEventID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEventException", x => x.CalendarEventExceptionID);
                    table.ForeignKey(
                        name: "FK_CalendarEventException_CalendarEvent",
                        column: x => x.CalendarEventID,
                        principalSchema: "home",
                        principalTable: "CalendarEvent",
                        principalColumn: "CalendarEventID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarEventMember",
                schema: "home",
                columns: table => new
                {
                    CalendarEventID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarEventMember", x => new { x.CalendarEventID, x.UserID });
                    table.ForeignKey(
                        name: "FK_CalendarEventMember_CalendarEvent",
                        column: x => x.CalendarEventID,
                        principalSchema: "home",
                        principalTable: "CalendarEvent",
                        principalColumn: "CalendarEventID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CalendarEventMember_User",
                        column: x => x.UserID,
                        principalSchema: "home",
                        principalTable: "User",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvent_HouseholdID",
                schema: "home",
                table: "CalendarEvent",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvent_StartDate",
                schema: "home",
                table: "CalendarEvent",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEvent_SubscriptionID",
                schema: "home",
                table: "CalendarEvent",
                column: "SubscriptionID");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEventException_CalendarEventID_OccurrenceDate",
                schema: "home",
                table: "CalendarEventException",
                columns: new[] { "CalendarEventID", "OccurrenceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarEventMember_UserID",
                schema: "home",
                table: "CalendarEventMember",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarSubscription_HouseholdID",
                schema: "home",
                table: "CalendarSubscription",
                column: "HouseholdID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarEventException",
                schema: "home");

            migrationBuilder.DropTable(
                name: "CalendarEventMember",
                schema: "home");

            migrationBuilder.DropTable(
                name: "CalendarEvent",
                schema: "home");

            migrationBuilder.DropTable(
                name: "CalendarSubscription",
                schema: "home");
        }
    }
}
