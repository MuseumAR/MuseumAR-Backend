using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class AddRefinedEntitiesAndColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Waypoints",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Waypoints",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Waypoints",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "WaypointEdges",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "WaypointEdges",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "(getutcdate())");

            migrationBuilder.AlterColumn<string>(
                name: "DoorWaypointId",
                table: "Rooms",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapType",
                table: "MuseumMaps",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Waypoints");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Waypoints");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Waypoints");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "WaypointEdges");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WaypointEdges");

            migrationBuilder.DropColumn(
                name: "MapType",
                table: "MuseumMaps");

            migrationBuilder.AlterColumn<int>(
                name: "DoorWaypointId",
                table: "Rooms",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
