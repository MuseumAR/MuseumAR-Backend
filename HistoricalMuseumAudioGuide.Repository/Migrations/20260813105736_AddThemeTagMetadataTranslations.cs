using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddThemeTagMetadataTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EraEn",
                table: "ExhibitMetadata",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HistoricalEventEn",
                table: "ExhibitMetadata",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TagTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagTrans_Tag",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ThemeTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThemeId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ThemeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThemeTrans_Theme",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_TagTrans",
                table: "TagTranslations",
                columns: new[] { "TagId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ThemeTrans",
                table: "ThemeTranslations",
                columns: new[] { "ThemeId", "LanguageCode" },
                unique: true);

            migrationBuilder.Sql(@"
INSERT INTO ThemeTranslations (ThemeId, LanguageCode, ThemeName, Description)
SELECT t.Id, 'vi', t.ThemeName, t.Description
FROM Themes t
WHERE NOT EXISTS (
    SELECT 1 FROM ThemeTranslations x WHERE x.ThemeId = t.Id AND x.LanguageCode = 'vi'
);

INSERT INTO TagTranslations (TagId, LanguageCode, TagName)
SELECT tg.Id, 'vi', tg.TagName
FROM Tags tg
WHERE NOT EXISTS (
    SELECT 1 FROM TagTranslations x WHERE x.TagId = tg.Id AND x.LanguageCode = 'vi'
);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagTranslations");

            migrationBuilder.DropTable(
                name: "ThemeTranslations");

            migrationBuilder.DropColumn(
                name: "EraEn",
                table: "ExhibitMetadata");

            migrationBuilder.DropColumn(
                name: "HistoricalEventEn",
                table: "ExhibitMetadata");
        }
    }
}
