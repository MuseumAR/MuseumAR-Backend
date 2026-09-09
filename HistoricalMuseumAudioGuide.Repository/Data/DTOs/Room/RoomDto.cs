using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Room
{
    public class RoomDto
    {
        public int Id { get; set; }
        public int MuseumId { get; set; }
        public int? MapId { get; set; }
        public string RoomCode { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public string? RoomNameEn { get; set; }
        public int FloorNumber { get; set; } = 1;
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public string? WaypointId { get; set; }
        public string? DoorWaypointId { get; set; }
        public double? CenterX { get; set; }
        public double? CenterY { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<RoomTranslationDto> Translations { get; set; } = new();
    }
}
