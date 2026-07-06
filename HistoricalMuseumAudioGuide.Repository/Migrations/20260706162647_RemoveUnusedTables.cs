using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentChangeLogs");

            migrationBuilder.DropTable(
                name: "MuseumLanguages");

            migrationBuilder.DropTable(
                name: "MuseumTranslations");

            migrationBuilder.DropTable(
                name: "PackageDownloads");

            migrationBuilder.DropTable(
                name: "PaymentLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChangedBy = table.Column<int>(type: "int", nullable: true),
                    ExhibitId = table.Column<int>(type: "int", nullable: true),
                    VersionId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ContentC__3214EC07D7F809D8", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeLog_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChangeLog_User",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChangeLog_Version",
                        column: x => x.VersionId,
                        principalTable: "ContentVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MuseumLanguages",
                columns: table => new
                {
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuseumLanguages", x => new { x.MuseumId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_MuseumLang_Language",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MuseumLang_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MuseumTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OpeningHours = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MuseumTr__3214EC077CD8E139", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuseumTrans_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageDownloads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    VisitorId = table.Column<int>(type: "int", nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DownloadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PackageD__3214EC0771127F31", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PkgDownloads_Package",
                        column: x => x.PackageId,
                        principalTable: "OfflinePackages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PkgDownloads_Visitor",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    LogMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RawResponse = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentL__3214EC07BB4B7413", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentLogs_Transaction",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentChangeLogs_ChangedBy",
                table: "ContentChangeLogs",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContentChangeLogs_ExhibitId",
                table: "ContentChangeLogs",
                column: "ExhibitId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentChangeLogs_VersionId",
                table: "ContentChangeLogs",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_MuseumLanguages_LanguageId",
                table: "MuseumLanguages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "UQ_MuseumTrans",
                table: "MuseumTranslations",
                columns: new[] { "MuseumId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackageDownloads_PackageId",
                table: "PackageDownloads",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDownloads_VisitorId",
                table: "PackageDownloads",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_TransactionId",
                table: "PaymentLogs",
                column: "TransactionId");
        }
    }
}
