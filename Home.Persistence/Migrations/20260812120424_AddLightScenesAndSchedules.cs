using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLightScenesAndSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LightScene",
                schema: "home",
                columns: table => new
                {
                    LightSceneID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    HouseholdID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightScene", x => x.LightSceneID);
                    table.ForeignKey(
                        name: "FK_LightScene_Household",
                        column: x => x.HouseholdID,
                        principalSchema: "home",
                        principalTable: "Household",
                        principalColumn: "HouseholdID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LightSceneState",
                schema: "home",
                columns: table => new
                {
                    LightSceneStateID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Brightness = table.Column<double>(type: "float", nullable: false),
                    Hue = table.Column<double>(type: "float", nullable: false),
                    IsOn = table.Column<bool>(type: "bit", nullable: false),
                    Kelvin = table.Column<int>(type: "int", nullable: false),
                    Saturation = table.Column<double>(type: "float", nullable: false),
                    LightID = table.Column<long>(type: "bigint", nullable: false),
                    LightSceneID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightSceneState", x => x.LightSceneStateID);
                    table.ForeignKey(
                        name: "FK_LightSceneState_Light",
                        column: x => x.LightID,
                        principalSchema: "home",
                        principalTable: "Light",
                        principalColumn: "LightID");
                    table.ForeignKey(
                        name: "FK_LightSceneState_Scene",
                        column: x => x.LightSceneID,
                        principalSchema: "home",
                        principalTable: "LightScene",
                        principalColumn: "LightSceneID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LightSchedule",
                schema: "home",
                columns: table => new
                {
                    LightScheduleID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DaysOfWeek = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastRunUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TimeOfDay = table.Column<TimeSpan>(type: "time", nullable: false),
                    LightSceneID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightSchedule", x => x.LightScheduleID);
                    table.ForeignKey(
                        name: "FK_LightSchedule_Scene",
                        column: x => x.LightSceneID,
                        principalSchema: "home",
                        principalTable: "LightScene",
                        principalColumn: "LightSceneID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LightScene_HouseholdID",
                schema: "home",
                table: "LightScene",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_LightSceneState_LightID",
                schema: "home",
                table: "LightSceneState",
                column: "LightID");

            migrationBuilder.CreateIndex(
                name: "IX_LightSceneState_LightSceneID",
                schema: "home",
                table: "LightSceneState",
                column: "LightSceneID");

            migrationBuilder.CreateIndex(
                name: "IX_LightSchedule_IsEnabled",
                schema: "home",
                table: "LightSchedule",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_LightSchedule_LightSceneID",
                schema: "home",
                table: "LightSchedule",
                column: "LightSceneID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LightSceneState",
                schema: "home");

            migrationBuilder.DropTable(
                name: "LightSchedule",
                schema: "home");

            migrationBuilder.DropTable(
                name: "LightScene",
                schema: "home");

        }
    }
}
