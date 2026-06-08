using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class ExhibitArasset
{
    public int Id { get; set; }

    public int ExhibitId { get; set; }

    public string AssetType { get; set; } = null!;

    public string AssetUrl { get; set; } = null!;

    public long? FileSizeBytes { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Exhibit Exhibit { get; set; } = null!;
}
