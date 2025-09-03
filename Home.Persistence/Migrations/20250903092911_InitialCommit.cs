using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "home");

            migrationBuilder.CreateTable(
                name: "ActivityState",
                schema: "home",
                columns: table => new
                {
                    ActivityStateID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityState", x => x.ActivityStateID);
                });

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

            migrationBuilder.CreateTable(
                name: "Ingredient",
                schema: "home",
                columns: table => new
                {
                    IngredientID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Volumne = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredient", x => x.IngredientID);
                });

            migrationBuilder.CreateTable(
                name: "LightLocation",
                columns: table => new
                {
                    LightLocationID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightLocation", x => x.LightLocationID);
                });

            migrationBuilder.CreateTable(
                name: "Note",
                schema: "home",
                columns: table => new
                {
                    NoteID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2025, 9, 3, 9, 29, 11, 113, DateTimeKind.Utc).AddTicks(6110))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Note", x => x.NoteID);
                });

            migrationBuilder.CreateTable(
                name: "Recipe",
                schema: "home",
                columns: table => new
                {
                    RecipeID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipe", x => x.RecipeID);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "home",
                columns: table => new
                {
                    UserID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleNames = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordLastChanged = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserID);
                });

            migrationBuilder.CreateTable(
                name: "LightGroup",
                columns: table => new
                {
                    LightGroupID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    LightLocationID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LightGroup", x => x.LightGroupID);
                    table.ForeignKey(
                        name: "FK_LightGroup_LightLocation",
                        column: x => x.LightLocationID,
                        principalTable: "LightLocation",
                        principalColumn: "LightLocationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredient",
                schema: "home",
                columns: table => new
                {
                    IngredientID = table.Column<long>(type: "bigint", nullable: false),
                    RecipeID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredient", x => new { x.IngredientID, x.RecipeID });
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Ingredient",
                        column: x => x.IngredientID,
                        principalSchema: "home",
                        principalTable: "Ingredient",
                        principalColumn: "IngredientID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeIngredient_Recipe",
                        column: x => x.RecipeID,
                        principalSchema: "home",
                        principalTable: "Recipe",
                        principalColumn: "RecipeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeNote",
                schema: "home",
                columns: table => new
                {
                    NoteID = table.Column<long>(type: "bigint", nullable: false),
                    RecipeID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeNote", x => new { x.NoteID, x.RecipeID });
                    table.ForeignKey(
                        name: "FK_RecipeNote_Note",
                        column: x => x.NoteID,
                        principalSchema: "home",
                        principalTable: "Note",
                        principalColumn: "NoteID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeNote_Recipe",
                        column: x => x.RecipeID,
                        principalSchema: "home",
                        principalTable: "Recipe",
                        principalColumn: "RecipeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeStep",
                schema: "home",
                columns: table => new
                {
                    RecipeStepID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    RecipeID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeStep", x => x.RecipeStepID);
                    table.ForeignKey(
                        name: "FK_RecipeStep_Recipe_RecipeID",
                        column: x => x.RecipeID,
                        principalSchema: "home",
                        principalTable: "Recipe",
                        principalColumn: "RecipeID");
                });

            migrationBuilder.CreateTable(
                name: "Activity",
                schema: "home",
                columns: table => new
                {
                    ActivityID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompletedDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDateUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    StateID = table.Column<long>(type: "bigint", nullable: false),
                    StatusID = table.Column<long>(type: "bigint", nullable: false),
                    UserID = table.Column<long>(type: "bigint", nullable: false),
                    ActivityStateID = table.Column<long>(type: "bigint", nullable: true),
                    ActivityStatusID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activity", x => x.ActivityID);
                    table.ForeignKey(
                        name: "FK_Activity_ActivityState_ActivityStateID",
                        column: x => x.ActivityStateID,
                        principalSchema: "home",
                        principalTable: "ActivityState",
                        principalColumn: "ActivityStateID");
                    table.ForeignKey(
                        name: "FK_Activity_ActivityStatus_ActivityStatusID",
                        column: x => x.ActivityStatusID,
                        principalSchema: "home",
                        principalTable: "ActivityStatus",
                        principalColumn: "ActivityStatusID");
                    table.ForeignKey(
                        name: "FK_Activity_State",
                        column: x => x.StateID,
                        principalSchema: "home",
                        principalTable: "ActivityState",
                        principalColumn: "ActivityStateID");
                    table.ForeignKey(
                        name: "FK_Activity_Status",
                        column: x => x.StatusID,
                        principalSchema: "home",
                        principalTable: "ActivityStatus",
                        principalColumn: "ActivityStatusID");
                    table.ForeignKey(
                        name: "FK_Activity_User",
                        column: x => x.UserID,
                        principalSchema: "home",
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApiAuditEntry",
                schema: "home",
                columns: table => new
                {
                    ApiAuditEntryID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CreatedResourceID = table.Column<long>(type: "bigint", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    HttpResponseStatusCode = table.Column<short>(type: "smallint", nullable: false),
                    RemoteIPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestReceivedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResponseSentOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserID = table.Column<long>(type: "bigint", nullable: true),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiAuditEntry", x => x.ApiAuditEntryID);
                    table.ForeignKey(
                        name: "FK_ApiAuditEntry_User",
                        column: x => x.UserID,
                        principalSchema: "home",
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Light",
                columns: table => new
                {
                    LightID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    LightGroupID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Light", x => x.LightID);
                    table.ForeignKey(
                        name: "FK_Light_Group",
                        column: x => x.LightGroupID,
                        principalTable: "LightGroup",
                        principalColumn: "LightGroupID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityRegion",
                schema: "home",
                columns: table => new
                {
                    ActivityRegionID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Region = table.Column<long>(type: "bigint", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    ActivityID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityRegion", x => x.ActivityRegionID);
                    table.ForeignKey(
                        name: "FK_ActivityRegion_Activity",
                        column: x => x.ActivityID,
                        principalSchema: "home",
                        principalTable: "Activity",
                        principalColumn: "ActivityID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Audit",
                schema: "home",
                columns: table => new
                {
                    AuditID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditContent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AuditDateUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditUserUserID = table.Column<long>(type: "bigint", nullable: false),
                    AuditUserName = table.Column<string>(type: "nvarchar(152)", maxLength: 152, nullable: true),
                    Entity = table.Column<long>(type: "bigint", nullable: false),
                    EntityID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit", x => x.AuditID);
                    table.ForeignKey(
                        name: "FK_Activity_Audit",
                        column: x => x.EntityID,
                        principalSchema: "home",
                        principalTable: "Activity",
                        principalColumn: "ActivityID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Audit_User_AuditUserUserID",
                        column: x => x.AuditUserUserID,
                        principalSchema: "home",
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Note_Audit",
                        column: x => x.EntityID,
                        principalSchema: "home",
                        principalTable: "Note",
                        principalColumn: "NoteID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Recipe_Audit",
                        column: x => x.EntityID,
                        principalSchema: "home",
                        principalTable: "Recipe",
                        principalColumn: "RecipeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Audit",
                        column: x => x.EntityID,
                        principalSchema: "home",
                        principalTable: "User",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActivityContent",
                schema: "home",
                columns: table => new
                {
                    ActivityContentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    FK_ActivityContent_ActivityRegion = table.Column<long>(type: "bigint", nullable: false),
                    RegionID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityContent", x => x.ActivityContentID);
                    table.ForeignKey(
                        name: "FK_ActivityContent_ActivityRegion",
                        column: x => x.FK_ActivityContent_ActivityRegion,
                        principalSchema: "home",
                        principalTable: "ActivityRegion",
                        principalColumn: "ActivityRegionID",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Activity_UserID",
                schema: "home",
                table: "Activity",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityContent_FK_ActivityContent_ActivityRegion",
                schema: "home",
                table: "ActivityContent",
                column: "FK_ActivityContent_ActivityRegion");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRegion_ActivityID",
                schema: "home",
                table: "ActivityRegion",
                column: "ActivityID");

            migrationBuilder.CreateIndex(
                name: "IX_ApiAuditEntry_UserID",
                schema: "home",
                table: "ApiAuditEntry",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_AuditUserUserID",
                schema: "home",
                table: "Audit",
                column: "AuditUserUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_Entity_EntityID",
                schema: "home",
                table: "Audit",
                columns: new[] { "Entity", "EntityID" });

            migrationBuilder.CreateIndex(
                name: "IX_Audit_EntityID",
                schema: "home",
                table: "Audit",
                column: "EntityID");

            migrationBuilder.CreateIndex(
                name: "IX_Light_LightGroupID",
                table: "Light",
                column: "LightGroupID");

            migrationBuilder.CreateIndex(
                name: "IX_LightGroup_LightLocationID",
                table: "LightGroup",
                column: "LightLocationID");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredient_RecipeID",
                schema: "home",
                table: "RecipeIngredient",
                column: "RecipeID");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeNote_RecipeID",
                schema: "home",
                table: "RecipeNote",
                column: "RecipeID");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeStep_RecipeID",
                schema: "home",
                table: "RecipeStep",
                column: "RecipeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityContent",
                schema: "home");

            migrationBuilder.DropTable(
                name: "ApiAuditEntry",
                schema: "home");

            migrationBuilder.DropTable(
                name: "Audit",
                schema: "home");

            migrationBuilder.DropTable(
                name: "Light");

            migrationBuilder.DropTable(
                name: "RecipeIngredient",
                schema: "home");

            migrationBuilder.DropTable(
                name: "RecipeNote",
                schema: "home");

            migrationBuilder.DropTable(
                name: "RecipeStep",
                schema: "home");

            migrationBuilder.DropTable(
                name: "ActivityRegion",
                schema: "home");

            migrationBuilder.DropTable(
                name: "LightGroup");

            migrationBuilder.DropTable(
                name: "Ingredient",
                schema: "home");

            migrationBuilder.DropTable(
                name: "Note",
                schema: "home");

            migrationBuilder.DropTable(
                name: "Recipe",
                schema: "home");

            migrationBuilder.DropTable(
                name: "Activity",
                schema: "home");

            migrationBuilder.DropTable(
                name: "LightLocation");

            migrationBuilder.DropTable(
                name: "ActivityState",
                schema: "home");

            migrationBuilder.DropTable(
                name: "ActivityStatus",
                schema: "home");

            migrationBuilder.DropTable(
                name: "User",
                schema: "home");
        }
    }
}
