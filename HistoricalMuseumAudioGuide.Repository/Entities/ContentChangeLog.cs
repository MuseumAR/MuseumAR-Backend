using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class ContentChangeLog
{
    public int Id { get; set; }

    public int VersionId { get; set; }

    public int? ExhibitId { get; set; }

    public string ChangeType { get; set; } = null!;

    public string EntityType { get; set; } = null!;

    public string? Description { get; set; }

    public int? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    public virtual User? ChangedByNavigation { get; set; }

    public virtual Exhibit? Exhibit { get; set; }

    public virtual ContentVersion Version { get; set; } = null!;
}
