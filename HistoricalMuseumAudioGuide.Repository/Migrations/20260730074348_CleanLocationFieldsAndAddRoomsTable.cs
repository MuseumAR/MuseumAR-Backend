using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class CleanLocationFieldsAndAddRoomsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationX",
                table: "Exhibits");

            migrationBuilder.DropColumn(
                name: "LocationY",
                table: "Exhibits");

            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Exhibits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    MapId = table.Column<int>(type: "int", nullable: true),
                    RoomCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FloorNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Map",
                        column: x => x.MapId,
                        principalTable: "MuseumMaps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Rooms_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Exhibits_RoomId",
                table: "Exhibits",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_MapId",
                table: "Rooms",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_MuseumId",
                table: "Rooms",
                column: "MuseumId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exhibits_Room",
                table: "Exhibits",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exhibits_Room",
                table: "Exhibits");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Exhibits_RoomId",
                table: "Exhibits");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Exhibits");

            migrationBuilder.AddColumn<double>(
                name: "LocationX",
                table: "Exhibits",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LocationY",
                table: "Exhibits",
                type: "float",
                nullable: true);
        }
    }
}
