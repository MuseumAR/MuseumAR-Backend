using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Room
{
    public class RoomDto
    {
        public int Id { get; set; }
        public int MuseumId { get; set; }
        public int? MapId { get; set; }
        public string RoomCode { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public int FloorNumber { get; set; } = 1;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
