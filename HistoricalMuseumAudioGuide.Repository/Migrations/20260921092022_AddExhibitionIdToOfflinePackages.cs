using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddExhibitionIdToOfflinePackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExhibitionId",
                table: "OfflinePackages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                table: "OfflinePackages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfflinePackages_ExhibitionId",
                table: "OfflinePackages",
                column: "ExhibitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_OfflinePackages_Exhibitions",
                table: "OfflinePackages",
                column: "ExhibitionId",
                principalTable: "Exhibitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OfflinePackages_Exhibitions",
                table: "OfflinePackages");

            migrationBuilder.DropIndex(
                name: "IX_OfflinePackages_ExhibitionId",
                table: "OfflinePackages");

            migrationBuilder.DropColumn(
                name: "ExhibitionId",
                table: "OfflinePackages");

            migrationBuilder.DropColumn(
                name: "PackageName",
                table: "OfflinePackages");
        }
    }
}
