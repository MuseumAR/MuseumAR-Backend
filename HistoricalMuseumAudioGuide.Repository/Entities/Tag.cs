using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Tag
{
    public int Id { get; set; }

    public int TagGroupId { get; set; }

    public string TagName { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual TagGroup TagGroup { get; set; } = null!;

    public virtual ICollection<Exhibit> Exhibits { get; set; } = new List<Exhibit>();
}
