using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLightStateAndHouseholdScoping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activity_ActivityState_ActivityStateID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_Activity_ActivityStatus_ActivityStatusID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_Activity_User",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivityContent_ActivityRegion",
                schema: "home",
                table: "ActivityContent");

            migrationBuilder.DropForeignKey(
                name: "FK_LightGroup_LightLocation",
                schema: "home",
                table: "LightGroup");

            migrationBuilder.DropIndex(
                name: "IX_ActivityContent_FK_ActivityContent_ActivityRegion",
                schema: "home",
                table: "ActivityContent");

            migrationBuilder.DropIndex(
                name: "IX_Activity_ActivityStateID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropIndex(
                name: "IX_Activity_ActivityStatusID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropIndex(
                name: "IX_Activity_StateID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropIndex(
                name: "IX_Activity_StatusID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropColumn(
                name: "FK_ActivityContent_ActivityRegion",
                schema: "home",
                table: "ActivityContent");

            migrationBuilder.DropColumn(
                name: "ActivityStateID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropColumn(
                name: "ActivityStatusID",
                schema: "home",
                table: "Activity");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 8, 12, 11, 13, 21, 158, DateTimeKind.Utc).AddTicks(6836),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 5, 19, 10, 43, 42, 443, DateTimeKind.Utc).AddTicks(7419));

            migrationBuilder.AddColumn<long>(
                name: "HouseholdID",
                schema: "home",
                table: "LightLocation",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<string>(
                name: "ID",
                schema: "home",
                table: "LightGroup",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                schema: "home",
                table: "LightGroup",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Brightness",
                schema: "home",
                table: "Light",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Hue",
                schema: "home",
                table: "Light",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsConnected",
                schema: "home",
                table: "Light",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOn",
                schema: "home",
                table: "Light",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Kelvin",
                schema: "home",
                table: "Light",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Saturation",
                schema: "home",
                table: "Light",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StateUpdatedUTC",
                schema: "home",
                table: "Light",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "UserID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "StatusID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "StateID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "HouseholdID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_LightLocation_HouseholdID",
                schema: "home",
                table: "LightLocation",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_Light_ID",
                schema: "home",
                table: "Light",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityContent_RegionID",
                schema: "home",
                table: "ActivityContent",
                column: "RegionID");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_HouseholdID",
                schema: "home",
                table: "Activity",
                column: "HouseholdID");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_StateID",
                schema: "home",
                table: "Activity",
                column: "StateID");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_StatusID",
                schema: "home",
                table: "Activity",
                column: "StatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_Household",
                schema: "home",
                table: "Activity",
                column: "HouseholdID",
                principalSchema: "home",
                principalTable: "Household",
                principalColumn: "HouseholdID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_User",
                schema: "home",
                table: "Activity",
                column: "UserID",
                principalSchema: "home",
                principalTable: "User",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityContent_ActivityRegion",
                schema: "home",
                table: "ActivityContent",
                column: "RegionID",
                principalSchema: "home",
                principalTable: "ActivityRegion",
                principalColumn: "ActivityRegionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LightGroup_Location",
                schema: "home",
                table: "LightGroup",
                column: "LightLocationID",
                principalSchema: "home",
                principalTable: "LightLocation",
                principalColumn: "LightLocationID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LightLocation_Household",
                schema: "home",
                table: "LightLocation",
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
                name: "FK_Activity_Household",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_Activity_User",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropForeignKey(
                name: "FK_ActivityContent_ActivityRegion",
                schema: "home",
                table: "ActivityContent");

            migrationBuilder.DropForeignKey(
                name: "FK_LightGroup_Location",
                schema: "home",
                table: "LightGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_LightLocation_Household",
                schema: "home",
                table: "LightLocation");

            migrationBuilder.DropIndex(
                name: "IX_LightLocation_HouseholdID",
                schema: "home",
                table: "LightLocation");

            migrationBuilder.DropIndex(
                name: "IX_Light_ID",
                schema: "home",
                table: "Light");

            migrationBuilder.DropIndex(
                name: "IX_ActivityContent_RegionID",
                schema: "home",
                table: "ActivityContent");

            migrationBuilder.DropIndex(
                name: "IX_Activity_HouseholdID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropIndex(
                name: "IX_Activity_StateID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropIndex(
                name: "IX_Activity_StatusID",
                schema: "home",
                table: "Activity");

            migrationBuilder.DropColumn(
                name: "HouseholdID",
                schema: "home",
                table: "LightLocation");

            migrationBuilder.DropColumn(
                name: "Sequence",
                schema: "home",
                table: "LightGroup");

            migrationBuilder.DropColumn(
                name: "Brightness",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "Hue",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "IsConnected",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "IsOn",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "Kelvin",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "Saturation",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "StateUpdatedUTC",
                schema: "home",
                table: "Light");

            migrationBuilder.DropColumn(
                name: "HouseholdID",
                schema: "home",
                table: "Activity");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUTC",
                schema: "home",
                table: "Note",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2026, 5, 19, 10, 43, 42, 443, DateTimeKind.Utc).AddTicks(7419),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2026, 8, 12, 11, 13, 21, 158, DateTimeKind.Utc).AddTicks(6836));

            migrationBuilder.AlterColumn<string>(
                name: "ID",
                schema: "home",
                table: "LightGroup",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FK_ActivityContent_ActivityRegion",
                schema: "home",
                table: "ActivityContent",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "UserID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "StatusID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "StateID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ActivityStateID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ActivityStatusID",
                schema: "home",
                table: "Activity",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActivityContent_FK_ActivityContent_ActivityRegion",
                schema: "home",
                table: "ActivityContent",
                column: "FK_ActivityContent_ActivityRegion");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_ActivityStateID",
                schema: "home",
                table: "Activity",
                column: "ActivityStateID");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_ActivityStatusID",
                schema: "home",
                table: "Activity",
                column: "ActivityStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Activity_StateID",
                schema: "home",
                table: "Activity",
                column: "StateID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activity_StatusID",
                schema: "home",
                table: "Activity",
                column: "StatusID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_ActivityState_ActivityStateID",
                schema: "home",
                table: "Activity",
                column: "ActivityStateID",
                principalSchema: "home",
                principalTable: "ActivityState",
                principalColumn: "ActivityStateID");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_ActivityStatus_ActivityStatusID",
                schema: "home",
                table: "Activity",
                column: "ActivityStatusID",
                principalSchema: "home",
                principalTable: "ActivityStatus",
                principalColumn: "ActivityStatusID");

            migrationBuilder.AddForeignKey(
                name: "FK_Activity_User",
                schema: "home",
                table: "Activity",
                column: "UserID",
                principalSchema: "home",
                principalTable: "User",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityContent_ActivityRegion",
                schema: "home",
                table: "ActivityContent",
                column: "FK_ActivityContent_ActivityRegion",
                principalSchema: "home",
                principalTable: "ActivityRegion",
                principalColumn: "ActivityRegionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LightGroup_LightLocation",
                schema: "home",
                table: "LightGroup",
                column: "LightLocationID",
                principalSchema: "home",
                principalTable: "LightLocation",
                principalColumn: "LightLocationID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
