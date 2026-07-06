using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class RestructureThemeAndAddTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safely drop FKs (may not exist if DB was created via SQL scripts)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ExhibMeta_Theme')
                    ALTER TABLE [ExhibitMetadata] DROP CONSTRAINT [FK_ExhibMeta_Theme];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TourRoutes_Theme')
                    ALTER TABLE [TourRoutes] DROP CONSTRAINT [FK_TourRoutes_Theme];
            ");

            // Safely drop indexes (may not exist if DB was created via SQL scripts)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TourRoutes_ThemeId' AND object_id = OBJECT_ID('TourRoutes'))
                    DROP INDEX [IX_TourRoutes_ThemeId] ON [TourRoutes];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExhibitMetadata_ThemeId' AND object_id = OBJECT_ID('ExhibitMetadata'))
                    DROP INDEX [IX_ExhibitMetadata_ThemeId] ON [ExhibitMetadata];
            ");

            // Safely drop columns (may not exist if DB was created differently)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ThemeId' AND object_id = OBJECT_ID('TourRoutes'))
                    ALTER TABLE [TourRoutes] DROP COLUMN [ThemeId];
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ThemeId' AND object_id = OBJECT_ID('ExhibitMetadata'))
                    ALTER TABLE [ExhibitMetadata] DROP COLUMN [ThemeId];
            ");

            migrationBuilder.AddColumn<int>(
                name: "ThemeId",
                table: "Exhibitions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TagGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TagGroupId = table.Column<int>(type: "int", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_TagGroup",
                        column: x => x.TagGroupId,
                        principalTable: "TagGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExhibitTags",
                columns: table => new
                {
                    ExhibitId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExhibitTags", x => new { x.ExhibitId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ExhibitTags_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExhibitTags_Tag",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exhibitions_ThemeId",
                table: "Exhibitions",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExhibitTags_TagId",
                table: "ExhibitTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_TagGroupId",
                table: "Tags",
                column: "TagGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exhibitions_Theme",
                table: "Exhibitions",
                column: "ThemeId",
                principalTable: "Themes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exhibitions_Theme",
                table: "Exhibitions");

            migrationBuilder.DropTable(
                name: "ExhibitTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "TagGroups");

            migrationBuilder.DropIndex(
                name: "IX_Exhibitions_ThemeId",
                table: "Exhibitions");

            migrationBuilder.DropColumn(
                name: "ThemeId",
                table: "Exhibitions");

            migrationBuilder.AddColumn<int>(
                name: "ThemeId",
                table: "TourRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThemeId",
                table: "ExhibitMetadata",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourRoutes_ThemeId",
                table: "TourRoutes",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExhibitMetadata_ThemeId",
                table: "ExhibitMetadata",
                column: "ThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExhibMeta_Theme",
                table: "ExhibitMetadata",
                column: "ThemeId",
                principalTable: "Themes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TourRoutes_Theme",
                table: "TourRoutes",
                column: "ThemeId",
                principalTable: "Themes",
                principalColumn: "Id");
        }
    }
}
