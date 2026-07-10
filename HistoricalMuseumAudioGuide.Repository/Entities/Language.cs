using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Language
{
    public int Id { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string LanguageName { get; set; } = null!;

    public string? NativeName { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

}
