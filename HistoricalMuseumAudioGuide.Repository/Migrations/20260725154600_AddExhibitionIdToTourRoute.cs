using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddExhibitionIdToTourRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExhibitionId",
                table: "TourRoutes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourRoutes_ExhibitionId",
                table: "TourRoutes",
                column: "ExhibitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TourRoutes_Exhibition",
                table: "TourRoutes",
                column: "ExhibitionId",
                principalTable: "Exhibitions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TourRoutes_Exhibition",
                table: "TourRoutes");

            migrationBuilder.DropIndex(
                name: "IX_TourRoutes_ExhibitionId",
                table: "TourRoutes");

            migrationBuilder.DropColumn(
                name: "ExhibitionId",
                table: "TourRoutes");
        }
    }
}
