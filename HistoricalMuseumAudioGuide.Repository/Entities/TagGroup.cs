using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class TagGroup
{
    public int Id { get; set; }

    public string GroupName { get; set; } = null!;

    public int SortOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
