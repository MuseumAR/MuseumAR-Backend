using System;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.SystemConfig;

public class SystemConfigDto
{
    public int Id { get; set; }
    public string ConfigKey { get; set; } = null!;
    public string ConfigValue { get; set; } = null!;
    public string? Description { get; set; }
}

public class UpdateSystemConfigDto
{
    public string ConfigValue { get; set; } = null!;
    public string? Description { get; set; }
}
