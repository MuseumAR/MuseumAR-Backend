namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Room
{
    public class CreateRoomDto
    {
        public int MuseumId { get; set; }
        public int? MapId { get; set; }
        public string RoomCode { get; set; } = null!;
        public string RoomName { get; set; } = null!;
        public string? RoomNameEn { get; set; }
        public int? FloorNumber { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
    }
}
