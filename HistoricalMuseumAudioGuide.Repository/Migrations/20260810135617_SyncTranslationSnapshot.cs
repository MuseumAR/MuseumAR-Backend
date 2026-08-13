using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class SyncTranslationSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "TicketTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "TicketTypes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MuseumTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OpeningHours = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuseumTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuseumTrans_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomTrans_Room",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_MuseumTrans",
                table: "MuseumTranslations",
                columns: new[] { "MuseumId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_RoomTrans",
                table: "RoomTranslations",
                columns: new[] { "RoomId", "LanguageCode" },
                unique: true);

            migrationBuilder.Sql(@"
INSERT INTO RoomTranslations (RoomId, LanguageCode, RoomName, Description)
SELECT r.Id, 'vi', r.RoomName, r.Description
FROM Rooms r
WHERE NOT EXISTS (
    SELECT 1 FROM RoomTranslations t WHERE t.RoomId = r.Id AND t.LanguageCode = 'vi'
);
");

            migrationBuilder.Sql(@"
INSERT INTO MuseumTranslations (MuseumId, LanguageCode, Name, Description, Address, OpeningHours)
SELECT m.Id, 'vi', m.Name, m.Description, m.Address, m.OpeningHours
FROM Museums m
WHERE NOT EXISTS (
    SELECT 1 FROM MuseumTranslations t WHERE t.MuseumId = m.Id AND t.LanguageCode = 'vi'
);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MuseumTranslations");

            migrationBuilder.DropTable(
                name: "RoomTranslations");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "TicketTypes");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "TicketTypes");
        }
    }
}
