using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SlideNarrator.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "API_KEY_SETTING",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EncryptedKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_API_KEY_SETTING", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ROLE",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLE", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "USER_ACCOUNT",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ACCOUNT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_USER_ACCOUNT_ROLE_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ROLE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LOG_ENTRY",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LOG_ENTRY", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LOG_ENTRY_USER_ACCOUNT_UserId",
                        column: x => x.UserId,
                        principalTable: "USER_ACCOUNT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PRESENTATION_PROJECT",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SourceLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SourceFileType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PageCount = table.Column<int>(type: "int", nullable: false),
                    OriginalFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PRESENTATION_PROJECT", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PRESENTATION_PROJECT_USER_ACCOUNT_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "USER_ACCOUNT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PROCESSING_JOB",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Progress = table.Column<int>(type: "int", nullable: false),
                    TargetLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CurrentStep = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OutputFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PresentationProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PROCESSING_JOB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PROCESSING_JOB_PRESENTATION_PROJECT_PresentationProjectId",
                        column: x => x.PresentationProjectId,
                        principalTable: "PRESENTATION_PROJECT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SLIDE_PAGE",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageNumber = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NarrationText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PresentationProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SLIDE_PAGE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SLIDE_PAGE_PRESENTATION_PROJECT_PresentationProjectId",
                        column: x => x.PresentationProjectId,
                        principalTable: "PRESENTATION_PROJECT",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TRANSLATION_RECORD",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TranslatedText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlidePageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSLATION_RECORD", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TRANSLATION_RECORD_SLIDE_PAGE_SlidePageId",
                        column: x => x.SlidePageId,
                        principalTable: "SLIDE_PAGE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AUDIO_TRACK",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    VoiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VoiceGender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Speed = table.Column<double>(type: "float", nullable: false),
                    DurationSeconds = table.Column<double>(type: "float", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TranslationRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AUDIO_TRACK", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AUDIO_TRACK_TRANSLATION_RECORD_TranslationRecordId",
                        column: x => x.TranslationRecordId,
                        principalTable: "TRANSLATION_RECORD",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ROLE",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Presenter" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Viewer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_API_KEY_SETTING_Provider",
                table: "API_KEY_SETTING",
                column: "Provider",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AUDIO_TRACK_TranslationRecordId",
                table: "AUDIO_TRACK",
                column: "TranslationRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_ENTRY_Timestamp",
                table: "LOG_ENTRY",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_LOG_ENTRY_UserId",
                table: "LOG_ENTRY",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PRESENTATION_PROJECT_UserAccountId",
                table: "PRESENTATION_PROJECT",
                column: "UserAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PROCESSING_JOB_PresentationProjectId",
                table: "PROCESSING_JOB",
                column: "PresentationProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_PROCESSING_JOB_Status",
                table: "PROCESSING_JOB",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ROLE_Name",
                table: "ROLE",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SLIDE_PAGE_PresentationProjectId_PageNumber",
                table: "SLIDE_PAGE",
                columns: new[] { "PresentationProjectId", "PageNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TRANSLATION_RECORD_SlidePageId_TargetLanguage",
                table: "TRANSLATION_RECORD",
                columns: new[] { "SlidePageId", "TargetLanguage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_ACCOUNT_Email",
                table: "USER_ACCOUNT",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_ACCOUNT_RoleId",
                table: "USER_ACCOUNT",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "API_KEY_SETTING");

            migrationBuilder.DropTable(
                name: "AUDIO_TRACK");

            migrationBuilder.DropTable(
                name: "LOG_ENTRY");

            migrationBuilder.DropTable(
                name: "PROCESSING_JOB");

            migrationBuilder.DropTable(
                name: "TRANSLATION_RECORD");

            migrationBuilder.DropTable(
                name: "SLIDE_PAGE");

            migrationBuilder.DropTable(
                name: "PRESENTATION_PROJECT");

            migrationBuilder.DropTable(
                name: "USER_ACCOUNT");

            migrationBuilder.DropTable(
                name: "ROLE");
        }
    }
}
