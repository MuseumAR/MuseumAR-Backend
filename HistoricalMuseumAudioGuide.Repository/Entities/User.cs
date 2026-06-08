using System;
using System.Collections.Generic;

namespace HistoricalMuseumAudioGuide.Repository.Entities;

public partial class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? AvatarUrl { get; set; }

    public int RoleId { get; set; }

    public int? MuseumId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<ContentChangeLog> ContentChangeLogs { get; set; } = new List<ContentChangeLog>();

    public virtual ICollection<ContentVersion> ContentVersions { get; set; } = new List<ContentVersion>();

    public virtual ICollection<Exhibit> ExhibitCreatedByNavigations { get; set; } = new List<Exhibit>();

    public virtual ICollection<Exhibit> ExhibitUpdatedByNavigations { get; set; } = new List<Exhibit>();

    public virtual Museum? Museum { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SystemConfiguration> SystemConfigurations { get; set; } = new List<SystemConfiguration>();
}
