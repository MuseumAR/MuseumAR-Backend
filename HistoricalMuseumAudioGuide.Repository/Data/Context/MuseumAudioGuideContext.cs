using System;
using System.Collections.Generic;
using HistoricalMuseumAudioGuide.Repository.Entities;
using Microsoft.EntityFrameworkCore;

namespace HistoricalMuseumAudioGuide.Repository.Data.Context;

public partial class MuseumAudioGuideContext : DbContext
{
    public MuseumAudioGuideContext()
    {
    }

    public MuseumAudioGuideContext(DbContextOptions<MuseumAudioGuideContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AgeGroup> AgeGroups { get; set; }

    public virtual DbSet<AnalyticsLog> AnalyticsLogs { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryTranslation> CategoryTranslations { get; set; }

    public virtual DbSet<ContentChangeLog> ContentChangeLogs { get; set; }

    public virtual DbSet<ContentVersion> ContentVersions { get; set; }

    public virtual DbSet<Exhibit> Exhibits { get; set; }

    public virtual DbSet<ExhibitArasset> ExhibitArassets { get; set; }

    public virtual DbSet<ExhibitImage> ExhibitImages { get; set; }

    public virtual DbSet<ExhibitMetadatum> ExhibitMetadata { get; set; }

    public virtual DbSet<ExhibitTranslation> ExhibitTranslations { get; set; }

    public virtual DbSet<Exhibition> Exhibitions { get; set; }

    public virtual DbSet<ExhibitionTranslation> ExhibitionTranslations { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<MapPoi> MapPois { get; set; }

    public virtual DbSet<Museum> Museums { get; set; }

    public virtual DbSet<MuseumLanguage> MuseumLanguages { get; set; }

    public virtual DbSet<MuseumMap> MuseumMaps { get; set; }

    public virtual DbSet<MuseumTranslation> MuseumTranslations { get; set; }

    public virtual DbSet<OfflinePackage> OfflinePackages { get; set; }

    public virtual DbSet<PackageDownload> PackageDownloads { get; set; }

    public virtual DbSet<PaymentLog> PaymentLogs { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SystemConfiguration> SystemConfigurations { get; set; }

    public virtual DbSet<Theme> Themes { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketType> TicketTypes { get; set; }

    public virtual DbSet<TourRoute> TourRoutes { get; set; }

    public virtual DbSet<TourRouteExhibit> TourRouteExhibits { get; set; }

    public virtual DbSet<TourRouteTranslation> TourRouteTranslations { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VisitedExhibit> VisitedExhibits { get; set; }

    public virtual DbSet<Visitor> Visitors { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (!string.IsNullOrEmpty(connectionString))
            {
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AgeGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AgeGroup__3214EC073267C0C0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.GroupName).HasMaxLength(50);
        });

        modelBuilder.Entity<AnalyticsLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Analytic__3214EC076218A530");

            entity.HasIndex(e => e.ActionType, "IX_Analytics_ActionType");

            entity.HasIndex(e => e.EventTimestamp, "IX_Analytics_EventTimestamp");

            entity.HasIndex(e => e.ExhibitId, "IX_Analytics_Exhibit");

            entity.HasIndex(e => e.MuseumId, "IX_Analytics_Museum");

            entity.Property(e => e.ActionType).HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.LanguageUsed)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SearchQuery).HasMaxLength(200);
            entity.Property(e => e.SessionId).HasMaxLength(100);

            entity.HasOne(d => d.Exhibit).WithMany(p => p.AnalyticsLogs)
                .HasForeignKey(d => d.ExhibitId)
                .HasConstraintName("FK_Analytics_Exhibit");

            entity.HasOne(d => d.Museum).WithMany(p => p.AnalyticsLogs)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Analytics_Museum");

            entity.HasOne(d => d.Visitor).WithMany(p => p.AnalyticsLogs)
                .HasForeignKey(d => d.VisitorId)
                .HasConstraintName("FK_Analytics_Visitor");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AuditLog__3214EC07D4241A7D");

            entity.HasIndex(e => e.CreatedAt, "IX_AuditLogs_CreatedAt");

            entity.HasIndex(e => e.EntityType, "IX_AuditLogs_EntityType");

            entity.HasIndex(e => e.UserId, "IX_AuditLogs_UserId");

            entity.Property(e => e.Action).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Bookmark__3214EC0743E83E00");

            entity.HasIndex(e => new { e.VisitorId, e.ExhibitId }, "UQ_Bookmarks").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Exhibit).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.ExhibitId)
                .HasConstraintName("FK_Bookmarks_Exhibit");

            entity.HasOne(d => d.Visitor).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.VisitorId)
                .HasConstraintName("FK_Bookmarks_Visitor");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC07A1949F6A");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Museum).WithMany(p => p.Categories)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Categories_Museum");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_Categories_Parent");
        });

        modelBuilder.Entity<CategoryTranslation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC079C960808");

            entity.HasIndex(e => new { e.CategoryId, e.LanguageCode }, "UQ_CatTrans").IsUnique();

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryTranslations)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_CatTrans_Category");
        });

        modelBuilder.Entity<ContentChangeLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ContentC__3214EC07D7F809D8");

            entity.Property(e => e.ChangeType).HasMaxLength(20);
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.EntityType).HasMaxLength(50);

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.ContentChangeLogs)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK_ChangeLog_User");

            entity.HasOne(d => d.Exhibit).WithMany(p => p.ContentChangeLogs)
                .HasForeignKey(d => d.ExhibitId)
                .HasConstraintName("FK_ChangeLog_Exhibit");

            entity.HasOne(d => d.Version).WithMany(p => p.ContentChangeLogs)
                .HasForeignKey(d => d.VersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChangeLog_Version");
        });

        modelBuilder.Entity<ContentVersion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ContentV__3214EC07E2F5D8C8");

            entity.HasIndex(e => new { e.MuseumId, e.VersionNumber }, "UQ_ContentVersion").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.VersionNumber).HasMaxLength(20);

            entity.HasOne(d => d.Museum).WithMany(p => p.ContentVersions)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContentVersions_Museum");

            entity.HasOne(d => d.PublishedByNavigation).WithMany(p => p.ContentVersions)
                .HasForeignKey(d => d.PublishedBy)
                .HasConstraintName("FK_ContentVersions_Publisher");
        });

        modelBuilder.Entity<Exhibit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exhibits__3214EC0728865193");

            entity.HasIndex(e => e.ExhibitCode, "UQ__Exhibits__5204E7405196F227").IsUnique();

            entity.Property(e => e.ArmarkerUrl)
                .HasMaxLength(500)
                .HasColumnName("ARMarkerUrl");
            entity.Property(e => e.AroverlayUrl)
                .HasMaxLength(500)
                .HasColumnName("AROverlayUrl");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ExhibitCode).HasMaxLength(50);
            entity.Property(e => e.QrcodeData)
                .HasMaxLength(500)
                .HasColumnName("QRCodeData");
            entity.Property(e => e.QrcodeImageUrl)
                .HasMaxLength(500)
                .HasColumnName("QRCodeImageUrl");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Category).WithMany(p => p.Exhibits)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Exhibits_Category");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ExhibitCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Exhibits_CreatedBy");

            entity.HasOne(d => d.Map).WithMany(p => p.Exhibits)
                .HasForeignKey(d => d.MapId)
                .HasConstraintName("FK_Exhibits_Map");

            entity.HasOne(d => d.Museum).WithMany(p => p.Exhibits)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exhibits_Museum");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.ExhibitUpdatedByNavigations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_Exhibits_UpdatedBy");
        });

        modelBuilder.Entity<ExhibitArasset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ExhibitA__3214EC07649477FB");

            entity.ToTable("ExhibitARAssets");

            entity.Property(e => e.AssetType)
                .HasMaxLength(30)
                .HasDefaultValue("OverlayImage");
            entity.Property(e => e.AssetUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(d => d.Exhibit).WithMany(p => p.ExhibitArassets)
                .HasForeignKey(d => d.ExhibitId)
                .HasConstraintName("FK_ARAssets_Exhibit");
        });

        modelBuilder.Entity<ExhibitImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ExhibitI__3214EC07CB1C8F3E");

            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);

            entity.HasOne(d => d.Exhibit).WithMany(p => p.ExhibitImages)
                .HasForeignKey(d => d.ExhibitId)
                .HasConstraintName("FK_ExhibitImages_Exhibit");
        });

        modelBuilder.Entity<ExhibitMetadatum>(entity =>
        {
            entity.HasKey(e => e.ExhibitId);

            entity.Property(e => e.ExhibitId).ValueGeneratedNever();
            entity.Property(e => e.Era).HasMaxLength(100);
            entity.Property(e => e.HistoricalEvent).HasMaxLength(200);

            entity.HasOne(d => d.AgeGroup).WithMany(p => p.ExhibitMetadata)
                .HasForeignKey(d => d.AgeGroupId)
                .HasConstraintName("FK_ExhibMeta_Age");

            entity.HasOne(d => d.Exhibit).WithOne(p => p.ExhibitMetadatum)
                .HasForeignKey<ExhibitMetadatum>(d => d.ExhibitId)
                .HasConstraintName("FK_ExhibMeta_Exhib");

            entity.HasOne(d => d.Theme).WithMany(p => p.ExhibitMetadata)
                .HasForeignKey(d => d.ThemeId)
                .HasConstraintName("FK_ExhibMeta_Theme");
        });

        modelBuilder.Entity<ExhibitTranslation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ExhibitT__3214EC074E3F7B08");

            entity.HasIndex(e => new { e.ExhibitId, e.LanguageCode }, "UQ_ExhibitTrans").IsUnique();

            entity.Property(e => e.AudioUrl).HasMaxLength(500);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Title).HasMaxLength(300);

            entity.HasOne(d => d.Exhibit).WithMany(p => p.ExhibitTranslations)
                .HasForeignKey(d => d.ExhibitId)
                .HasConstraintName("FK_ExhibitTrans_Exhibit");
        });

        modelBuilder.Entity<Exhibition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exhibiti__3214EC0743051CE0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Museum).WithMany(p => p.Exhibitions)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exhibitions_Museum");

            entity.HasMany(d => d.Exhibits).WithMany(p => p.Exhibitions)
                .UsingEntity<Dictionary<string, object>>(
                    "ExhibitionExhibit",
                    r => r.HasOne<Exhibit>().WithMany()
                        .HasForeignKey("ExhibitId")
                        .HasConstraintName("FK_ExhibEx_Exhibit"),
                    l => l.HasOne<Exhibition>().WithMany()
                        .HasForeignKey("ExhibitionId")
                        .HasConstraintName("FK_ExhibEx_Exhibition"),
                    j =>
                    {
                        j.HasKey("ExhibitionId", "ExhibitId");
                        j.ToTable("ExhibitionExhibits");
                    });
        });

        modelBuilder.Entity<ExhibitionTranslation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Exhibiti__3214EC07A48C3DBB");

            entity.HasIndex(e => new { e.ExhibitionId, e.LanguageCode }, "UQ_ExhibTrans").IsUnique();

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(200);

            entity.HasOne(d => d.Exhibition).WithMany(p => p.ExhibitionTranslations)
                .HasForeignKey(d => d.ExhibitionId)
                .HasConstraintName("FK_ExhibTrans_Exhibition");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Language__3214EC075A095684");

            entity.HasIndex(e => e.LanguageCode, "UQ__Language__8B8C8A3446FA90F1").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LanguageCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LanguageName).HasMaxLength(50);
            entity.Property(e => e.NativeName).HasMaxLength(50);
        });

        modelBuilder.Entity<MapPoi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MapPOIs__3214EC07011CA550");

            entity.ToTable("MapPOIs");

            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.Poitype)
                .HasMaxLength(50)
                .HasColumnName("POIType");

            entity.HasOne(d => d.Map).WithMany(p => p.MapPois)
                .HasForeignKey(d => d.MapId)
                .HasConstraintName("FK_MapPOIs_Map");
        });

        modelBuilder.Entity<Museum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Museums__3214EC07F64D086B");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.ContactEmail).HasMaxLength(255);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasDefaultValue("Vietnam");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OpeningHours).HasMaxLength(500);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Website).HasMaxLength(500);
        });

        modelBuilder.Entity<MuseumLanguage>(entity =>
        {
            entity.HasKey(e => new { e.MuseumId, e.LanguageId });

            entity.HasOne(d => d.Language).WithMany(p => p.MuseumLanguages)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MuseumLang_Language");

            entity.HasOne(d => d.Museum).WithMany(p => p.MuseumLanguages)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MuseumLang_Museum");
        });

        modelBuilder.Entity<MuseumMap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MuseumMa__3214EC077DAA1EC5");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.FloorNumber).HasDefaultValue(1);
            entity.Property(e => e.MapImageUrl).HasMaxLength(500);
            entity.Property(e => e.MapName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Museum).WithMany(p => p.MuseumMaps)
                .HasForeignKey(d => d.MuseumId)
                .HasConstraintName("FK_MuseumMaps_Museum");
        });

        modelBuilder.Entity<MuseumTranslation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MuseumTr__3214EC077CD8E139");

            entity.HasIndex(e => new { e.MuseumId, e.LanguageCode }, "UQ_MuseumTrans").IsUnique();

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.OpeningHours).HasMaxLength(500);

            entity.HasOne(d => d.Museum).WithMany(p => p.MuseumTranslations)
                .HasForeignKey(d => d.MuseumId)
                .HasConstraintName("FK_MuseumTrans_Museum");
        });

        modelBuilder.Entity<OfflinePackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OfflineP__3214EC074C67D86D");

            entity.Property(e => e.ArassetCount).HasColumnName("ARAssetCount");
            entity.Property(e => e.Checksum).HasMaxLength(128);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.PackageUrl).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Building");

            entity.HasOne(d => d.Museum).WithMany(p => p.OfflinePackages)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OfflinePackages_Museum");

            entity.HasOne(d => d.Version).WithMany(p => p.OfflinePackages)
                .HasForeignKey(d => d.VersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OfflinePackages_Version");
        });

        modelBuilder.Entity<PackageDownload>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PackageD__3214EC0771127F31");

            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.DownloadedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Package).WithMany(p => p.PackageDownloads)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PkgDownloads_Package");

            entity.HasOne(d => d.Visitor).WithMany(p => p.PackageDownloads)
                .HasForeignKey(d => d.VisitorId)
                .HasConstraintName("FK_PkgDownloads_Visitor");
        });

        modelBuilder.Entity<PaymentLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentL__3214EC07BB4B7413");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.LogMessage).HasMaxLength(500);

            entity.HasOne(d => d.Transaction).WithMany(p => p.PaymentLogs)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentLogs_Transaction");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentM__3214EC07DA605523");

            entity.HasIndex(e => e.Name, "UQ__PaymentM__737584F65638694F").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC07E1106079");

            entity.HasIndex(e => e.PermissionName, "UQ__Permissi__0FFDA357D7FD4E68").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Module).HasMaxLength(50);
            entity.Property(e => e.PermissionName).HasMaxLength(100);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RefreshT__3214EC07DF597E3F");

            entity.HasIndex(e => e.Token, "IX_RefreshTokens_Token");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500);
            entity.Property(e => e.Token).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_RefreshTokens_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC077E061479");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616097A109F1").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolePermissions_Permission"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolePermissions_Role"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId");
                        j.ToTable("RolePermissions");
                    });
        });

        modelBuilder.Entity<SystemConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SystemCo__3214EC07FFC5BA3B");

            entity.HasIndex(e => e.ConfigKey, "UQ__SystemCo__4A3067846AFFD489").IsUnique();

            entity.Property(e => e.ConfigKey).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SystemConfigurations)
                .HasForeignKey(d => d.UpdatedBy)
                .HasConstraintName("FK_SysConfig_User");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Themes__3214EC074D21EDC9");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.ThemeName).HasMaxLength(100);

            entity.HasOne(d => d.Museum).WithMany(p => p.Themes)
                .HasForeignKey(d => d.MuseumId)
                .HasConstraintName("FK_Themes_Museum");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tickets__3214EC07876BB3A3");

            entity.HasIndex(e => e.TicketCode, "UQ__Tickets__598CF7A3F437EC5B").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.PurchaseDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TicketCode).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.TicketType).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TicketTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Type");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("FK_Tickets_Transaction");

            entity.HasOne(d => d.Visitor).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.VisitorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tickets_Visitor");
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TicketTy__3214EC075DC34A40");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Exhibition).WithMany(p => p.TicketTypes)
                .HasForeignKey(d => d.ExhibitionId)
                .HasConstraintName("FK_TicketTypes_Exhibition");

            entity.HasOne(d => d.Museum).WithMany(p => p.TicketTypes)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketTypes_Museum");
        });

        modelBuilder.Entity<TourRoute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourRout__3214EC07F00F9A99");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.AgeGroup).WithMany(p => p.TourRoutes)
                .HasForeignKey(d => d.AgeGroupId)
                .HasConstraintName("FK_TourRoutes_AgeGroup");

            entity.HasOne(d => d.Museum).WithMany(p => p.TourRoutes)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TourRoutes_Museum");

            entity.HasOne(d => d.Theme).WithMany(p => p.TourRoutes)
                .HasForeignKey(d => d.ThemeId)
                .HasConstraintName("FK_TourRoutes_Theme");
        });

        modelBuilder.Entity<TourRouteExhibit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourRout__3214EC07028FAC02");

            entity.HasIndex(e => new { e.TourRouteId, e.ExhibitId }, "UQ_TourRouteExhibits").IsUnique();

            entity.HasOne(d => d.Exhibit).WithMany(p => p.TourRouteExhibits)
                .HasForeignKey(d => d.ExhibitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TourRouteExhibits_Exhibit");

            entity.HasOne(d => d.TourRoute).WithMany(p => p.TourRouteExhibits)
                .HasForeignKey(d => d.TourRouteId)
                .HasConstraintName("FK_TourRouteExhibits_Route");
        });

        modelBuilder.Entity<TourRouteTranslation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TourRout__3214EC07DB106995");

            entity.HasIndex(e => new { e.TourRouteId, e.LanguageCode }, "UQ_TourRouteTrans").IsUnique();

            entity.Property(e => e.LanguageCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.RouteName).HasMaxLength(200);

            entity.HasOne(d => d.TourRoute).WithMany(p => p.TourRouteTranslations)
                .HasForeignKey(d => d.TourRouteId)
                .HasConstraintName("FK_TourRouteTrans_Route");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC07634B4F68");

            entity.HasIndex(e => e.OrderCode, "UQ__Transact__999B5229FB610A1E").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .HasDefaultValue("VND");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.GatewayTransactionId).HasMaxLength(100);
            entity.Property(e => e.OrderCode).HasMaxLength(50);
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Method");

            entity.HasOne(d => d.Visitor).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.VisitorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Visitor");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07BE722AC8");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534AC9E29DD").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Museum).WithMany(p => p.Users)
                .HasForeignKey(d => d.MuseumId)
                .HasConstraintName("FK_Users_Museum");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Role");
        });

        modelBuilder.Entity<VisitedExhibit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VisitedE__3214EC072F597FFC");

            entity.HasIndex(e => e.ExhibitId, "IX_VisitedExhibits_Exhibit");

            entity.HasIndex(e => e.VisitorId, "IX_VisitedExhibits_Visitor");

            entity.Property(e => e.VisitedAt).HasDefaultValueSql("(getutcdate())");

            entity.HasOne(d => d.Exhibit).WithMany(p => p.VisitedExhibits)
                .HasForeignKey(d => d.ExhibitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VisitedExhibits_Exhibit");

            entity.HasOne(d => d.Museum).WithMany(p => p.VisitedExhibits)
                .HasForeignKey(d => d.MuseumId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VisitedExhibits_Museum");

            entity.HasOne(d => d.Visitor).WithMany(p => p.VisitedExhibits)
                .HasForeignKey(d => d.VisitorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VisitedExhibits_Visitor");
        });

        modelBuilder.Entity<Visitor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Visitors__3214EC0718CE3833");

            entity.HasIndex(e => e.DeviceId, "UQ__Visitors__49E123102804749D").IsUnique();

            entity.Property(e => e.AppVersion).HasMaxLength(20);
            entity.Property(e => e.DeviceId).HasMaxLength(255);
            entity.Property(e => e.DeviceModel).HasMaxLength(100);
            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstSeenAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.LastSeenAt).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.PreferredLang)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("vi");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
