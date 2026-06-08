using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Category
{
    public int Id { get; set; }

    public int MuseumId { get; set; }

    public int? ParentId { get; set; }

    public int SortOrder { get; set; }

    public string? IconUrl { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CategoryTranslation> CategoryTranslations { get; set; } = new List<CategoryTranslation>();

    public virtual ICollection<Exhibit> Exhibits { get; set; } = new List<Exhibit>();

    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    public virtual Museum Museum { get; set; } = null!;

    public virtual Category? Parent { get; set; }
}
