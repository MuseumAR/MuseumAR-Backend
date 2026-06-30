using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class MakeCategoryMuseumIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Themes]') 
                    AND name = N'MuseumId'
                )
                BEGIN
                    ALTER TABLE [Themes] ADD [MuseumId] int NULL;
                    
                    EXEC('CREATE INDEX [IX_Themes_MuseumId] ON [Themes] ([MuseumId])');
                    
                    ALTER TABLE [Themes] ADD CONSTRAINT [FK_Themes_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]);
                END
            ");

            migrationBuilder.AlterColumn<int>(
                name: "MuseumId",
                table: "Categories",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Themes_Museum",
                table: "Themes");

            migrationBuilder.DropIndex(
                name: "IX_Themes_MuseumId",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "MuseumId",
                table: "Themes");

            migrationBuilder.AlterColumn<int>(
                name: "MuseumId",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
