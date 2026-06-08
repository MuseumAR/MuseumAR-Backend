using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class Permission
{
    public int Id { get; set; }

    public string PermissionName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Module { get; set; }

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
