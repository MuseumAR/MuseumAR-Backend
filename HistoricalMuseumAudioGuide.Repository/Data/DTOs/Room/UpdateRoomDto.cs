namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Room
{
    public class UpdateRoomDto
    {
        public int? MapId { get; set; }
        public string? RoomCode { get; set; }
        public string? RoomName { get; set; }
        public string? RoomNameEn { get; set; }
        public int? FloorNumber { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
    }
}
