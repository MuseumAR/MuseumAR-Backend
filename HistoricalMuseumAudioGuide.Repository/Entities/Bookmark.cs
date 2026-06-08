using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Bookmark
{
    public int Id { get; set; }

    public int VisitorId { get; set; }

    public int ExhibitId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Exhibit Exhibit { get; set; } = null!;

    public virtual Visitor Visitor { get; set; } = null!;
}
