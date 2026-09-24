using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class OfflinePackageTranslation
{
    public int Id { get; set; }

    public int PackageId { get; set; }

    public string LanguageCode { get; set; } = null!;

    public string? PackageName { get; set; }

    public string? Description { get; set; }

    public virtual OfflinePackage Package { get; set; } = null!;
}
