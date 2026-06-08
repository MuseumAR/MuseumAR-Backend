using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class CategoryTranslation
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual Category Category { get; set; } = null!;
}
