using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class RoomTranslation
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string RoomName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Room Room { get; set; } = null!;
}
