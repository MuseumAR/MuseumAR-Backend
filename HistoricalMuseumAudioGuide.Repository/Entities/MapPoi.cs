using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class MapPoi
{
    public int Id { get; set; }

    public int MapId { get; set; }

    public string Poitype { get; set; } = null!;

    public double LocationX { get; set; }

    public double LocationY { get; set; }

    public string? Description { get; set; }

    public virtual MuseumMap Map { get; set; } = null!;
}
