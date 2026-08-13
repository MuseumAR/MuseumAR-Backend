using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddMapIdToWaypoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MapId",
                table: "Waypoints",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Waypoints_MapId",
                table: "Waypoints",
                column: "MapId");

            migrationBuilder.AddForeignKey(
                name: "FK_Waypoints_MuseumMaps_MapId",
                table: "Waypoints",
                column: "MapId",
                principalTable: "MuseumMaps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Waypoints_MuseumMaps_MapId",
                table: "Waypoints");

            migrationBuilder.DropIndex(
                name: "IX_Waypoints_MapId",
                table: "Waypoints");

            migrationBuilder.DropColumn(
                name: "MapId",
                table: "Waypoints");
        }
    }
}
