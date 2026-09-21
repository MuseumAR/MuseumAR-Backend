IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AgeGroups] (
    [Id] int NOT NULL IDENTITY,
    [GroupName] nvarchar(50) NOT NULL,
    [MinAge] int NULL,
    [MaxAge] int NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__AgeGroup__3214EC073267C0C0] PRIMARY KEY ([Id])
);

CREATE TABLE [Languages] (
    [Id] int NOT NULL IDENTITY,
    [LanguageCode] varchar(10) NOT NULL,
    [LanguageName] nvarchar(50) NOT NULL,
    [NativeName] nvarchar(50) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Language__3214EC075A095684] PRIMARY KEY ([Id])
);

CREATE TABLE [Museums] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Address] nvarchar(500) NULL,
    [City] nvarchar(100) NULL,
    [Province] nvarchar(100) NULL,
    [Country] nvarchar(100) NULL DEFAULT N'Vietnam',
    [Latitude] decimal(10,7) NULL,
    [Longitude] decimal(10,7) NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [OpeningHours] nvarchar(500) NULL,
    [ContactPhone] nvarchar(20) NULL,
    [ContactEmail] nvarchar(255) NULL,
    [Website] nvarchar(500) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Museums__3214EC07F64D086B] PRIMARY KEY ([Id])
);

CREATE TABLE [PaymentMethods] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(50) NOT NULL,
    [DisplayName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    [IconUrl] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__PaymentM__3214EC07DA605523] PRIMARY KEY ([Id])
);

CREATE TABLE [Permissions] (
    [Id] int NOT NULL IDENTITY,
    [PermissionName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    [Module] nvarchar(50) NULL,
    CONSTRAINT [PK__Permissi__3214EC07E1106079] PRIMARY KEY ([Id])
);

CREATE TABLE [Roles] (
    [Id] int NOT NULL IDENTITY,
    [RoleName] nvarchar(50) NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Roles__3214EC077E061479] PRIMARY KEY ([Id])
);

CREATE TABLE [Themes] (
    [Id] int NOT NULL IDENTITY,
    [ThemeName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Themes__3214EC074D21EDC9] PRIMARY KEY ([Id])
);

CREATE TABLE [Visitors] (
    [Id] int NOT NULL IDENTITY,
    [DeviceId] nvarchar(255) NOT NULL,
    [DisplayName] nvarchar(100) NULL,
    [Email] nvarchar(255) NULL,
    [PreferredLang] varchar(10) NOT NULL DEFAULT 'vi',
    [DeviceType] nvarchar(50) NULL,
    [DeviceModel] nvarchar(100) NULL,
    [AppVersion] nvarchar(20) NULL,
    [FirstSeenAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [LastSeenAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Visitors__3214EC0718CE3833] PRIMARY KEY ([Id])
);

CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [ParentId] int NULL,
    [SortOrder] int NOT NULL,
    [IconUrl] nvarchar(500) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Categori__3214EC07A1949F6A] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Categories_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]),
    CONSTRAINT [FK_Categories_Parent] FOREIGN KEY ([ParentId]) REFERENCES [Categories] ([Id])
);

CREATE TABLE [Exhibitions] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [StartDate] datetime2 NULL,
    [EndDate] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Exhibiti__3214EC0743051CE0] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Exhibitions_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id])
);

CREATE TABLE [MuseumLanguages] (
    [MuseumId] int NOT NULL,
    [LanguageId] int NOT NULL,
    [IsDefault] bit NOT NULL,
    CONSTRAINT [PK_MuseumLanguages] PRIMARY KEY ([MuseumId], [LanguageId]),
    CONSTRAINT [FK_MuseumLang_Language] FOREIGN KEY ([LanguageId]) REFERENCES [Languages] ([Id]),
    CONSTRAINT [FK_MuseumLang_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id])
);

CREATE TABLE [MuseumMaps] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [FloorNumber] int NOT NULL DEFAULT 1,
    [MapName] nvarchar(100) NULL,
    [MapImageUrl] nvarchar(500) NOT NULL,
    [Width] int NULL,
    [Height] int NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__MuseumMa__3214EC077DAA1EC5] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MuseumMaps_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [MuseumTranslations] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [OpeningHours] nvarchar(500) NULL,
    CONSTRAINT [PK__MuseumTr__3214EC077CD8E139] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MuseumTrans_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RolePermissions] (
    [RoleId] int NOT NULL,
    [PermissionId] int NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
    CONSTRAINT [FK_RolePermissions_Permission] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]),
    CONSTRAINT [FK_RolePermissions_Role] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [PhoneNumber] nvarchar(20) NULL,
    [AvatarUrl] nvarchar(500) NULL,
    [RoleId] int NOT NULL,
    [MuseumId] int NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [LastLoginAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [PasswordResetToken] nvarchar(100) NULL,
    [ResetTokenExpiresAt] datetime2 NULL,
    CONSTRAINT [PK__Users__3214EC07BE722AC8] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Users_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]),
    CONSTRAINT [FK_Users_Role] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id])
);

CREATE TABLE [TourRoutes] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [EstimatedMinutes] int NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [AgeGroupId] int NULL,
    [ThemeId] int NULL,
    [IsDefault] bit NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__TourRout__3214EC07F00F9A99] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TourRoutes_AgeGroup] FOREIGN KEY ([AgeGroupId]) REFERENCES [AgeGroups] ([Id]),
    CONSTRAINT [FK_TourRoutes_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]),
    CONSTRAINT [FK_TourRoutes_Theme] FOREIGN KEY ([ThemeId]) REFERENCES [Themes] ([Id])
);

CREATE TABLE [Transactions] (
    [Id] int NOT NULL IDENTITY,
    [VisitorId] int NOT NULL,
    [PaymentMethodId] int NOT NULL,
    [OrderCode] nvarchar(50) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    [Currency] nvarchar(10) NOT NULL DEFAULT N'VND',
    [PaymentStatus] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [GatewayTransactionId] nvarchar(100) NULL,
    [PaymentDate] datetime2 NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Transact__3214EC07634B4F68] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transactions_Method] FOREIGN KEY ([PaymentMethodId]) REFERENCES [PaymentMethods] ([Id]),
    CONSTRAINT [FK_Transactions_Visitor] FOREIGN KEY ([VisitorId]) REFERENCES [Visitors] ([Id])
);

CREATE TABLE [CategoryTranslations] (
    [Id] int NOT NULL IDENTITY,
    [CategoryId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [CategoryName] nvarchar(100) NOT NULL,
    [Description] nvarchar(500) NULL,
    CONSTRAINT [PK__Category__3214EC079C960808] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CatTrans_Category] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ExhibitionTranslations] (
    [Id] int NOT NULL IDENTITY,
    [ExhibitionId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK__Exhibiti__3214EC07A48C3DBB] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExhibTrans_Exhibition] FOREIGN KEY ([ExhibitionId]) REFERENCES [Exhibitions] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [TicketTypes] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [ExhibitionId] int NULL,
    [Name] nvarchar(100) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Description] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__TicketTy__3214EC075DC34A40] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TicketTypes_Exhibition] FOREIGN KEY ([ExhibitionId]) REFERENCES [Exhibitions] ([Id]),
    CONSTRAINT [FK_TicketTypes_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id])
);

CREATE TABLE [MapPOIs] (
    [Id] int NOT NULL IDENTITY,
    [MapId] int NOT NULL,
    [POIType] nvarchar(50) NOT NULL,
    [LocationX] float NOT NULL,
    [LocationY] float NOT NULL,
    [Description] nvarchar(250) NULL,
    CONSTRAINT [PK__MapPOIs__3214EC07011CA550] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MapPOIs_Map] FOREIGN KEY ([MapId]) REFERENCES [MuseumMaps] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AuditLogs] (
    [Id] bigint NOT NULL IDENTITY,
    [UserId] int NULL,
    [Action] nvarchar(50) NOT NULL,
    [EntityType] nvarchar(50) NOT NULL,
    [EntityId] int NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [IpAddress] nvarchar(45) NULL,
    [UserAgent] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__AuditLog__3214EC07D4241A7D] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AuditLogs_User] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id])
);

CREATE TABLE [ContentVersions] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [VersionNumber] nvarchar(20) NOT NULL,
    [ChangeDescription] nvarchar(max) NULL,
    [TotalExhibits] int NULL,
    [TotalMediaFiles] int NULL,
    [PackageSizeBytes] bigint NULL,
    [PublishedBy] int NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    [PublishedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__ContentV__3214EC07E2F5D8C8] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ContentVersions_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]),
    CONSTRAINT [FK_ContentVersions_Publisher] FOREIGN KEY ([PublishedBy]) REFERENCES [Users] ([Id])
);

CREATE TABLE [Exhibits] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [CategoryId] int NULL,
    [ExhibitCode] nvarchar(50) NULL,
    [QRCodeData] nvarchar(500) NULL,
    [QRCodeImageUrl] nvarchar(500) NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [AROverlayUrl] nvarchar(500) NULL,
    [ARMarkerUrl] nvarchar(500) NULL,
    [MapId] int NULL,
    [LocationX] float NULL,
    [LocationY] float NULL,
    [SortOrder] int NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    [PublishedAt] datetime2 NULL,
    [CreatedBy] int NULL,
    [UpdatedBy] int NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Exhibits__3214EC0728865193] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Exhibits_Category] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]),
    CONSTRAINT [FK_Exhibits_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Exhibits_Map] FOREIGN KEY ([MapId]) REFERENCES [MuseumMaps] ([Id]),
    CONSTRAINT [FK_Exhibits_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]),
    CONSTRAINT [FK_Exhibits_UpdatedBy] FOREIGN KEY ([UpdatedBy]) REFERENCES [Users] ([Id])
);

CREATE TABLE [RefreshTokens] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Token] nvarchar(500) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [RevokedAt] datetime2 NULL,
    [ReplacedByToken] nvarchar(500) NULL,
    CONSTRAINT [PK__RefreshT__3214EC07DF597E3F] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_User] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [SystemConfigurations] (
    [Id] int NOT NULL IDENTITY,
    [ConfigKey] nvarchar(100) NOT NULL,
    [ConfigValue] nvarchar(max) NOT NULL,
    [Description] nvarchar(255) NULL,
    [UpdatedBy] int NULL,
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__SystemCo__3214EC07FFC5BA3B] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SysConfig_User] FOREIGN KEY ([UpdatedBy]) REFERENCES [Users] ([Id])
);

CREATE TABLE [TourRouteTranslations] (
    [Id] int NOT NULL IDENTITY,
    [TourRouteId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [RouteName] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK__TourRout__3214EC07DB106995] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TourRouteTrans_Route] FOREIGN KEY ([TourRouteId]) REFERENCES [TourRoutes] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [PaymentLogs] (
    [Id] int NOT NULL IDENTITY,
    [TransactionId] int NOT NULL,
    [RawResponse] nvarchar(max) NULL,
    [LogMessage] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__PaymentL__3214EC07BB4B7413] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PaymentLogs_Transaction] FOREIGN KEY ([TransactionId]) REFERENCES [Transactions] ([Id])
);

CREATE TABLE [Tickets] (
    [Id] int NOT NULL IDENTITY,
    [VisitorId] int NOT NULL,
    [TicketTypeId] int NOT NULL,
    [TransactionId] int NULL,
    [TicketCode] nvarchar(100) NOT NULL,
    [PurchaseDate] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [ValidDate] datetime2 NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Tickets__3214EC07876BB3A3] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tickets_Transaction] FOREIGN KEY ([TransactionId]) REFERENCES [Transactions] ([Id]),
    CONSTRAINT [FK_Tickets_Type] FOREIGN KEY ([TicketTypeId]) REFERENCES [TicketTypes] ([Id]),
    CONSTRAINT [FK_Tickets_Visitor] FOREIGN KEY ([VisitorId]) REFERENCES [Visitors] ([Id])
);

CREATE TABLE [OfflinePackages] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [VersionId] int NOT NULL,
    [PackageUrl] nvarchar(500) NOT NULL,
    [PackageSizeBytes] bigint NOT NULL,
    [Checksum] nvarchar(128) NULL,
    [AudioCount] int NULL,
    [ImageCount] int NULL,
    [ARAssetCount] int NULL,
    [ExhibitCount] int NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Building',
    [BuiltAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__OfflineP__3214EC074C67D86D] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OfflinePackages_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]),
    CONSTRAINT [FK_OfflinePackages_Version] FOREIGN KEY ([VersionId]) REFERENCES [ContentVersions] ([Id])
);

CREATE TABLE [AnalyticsLogs] (
    [Id] bigint NOT NULL IDENTITY,
    [VisitorId] int NULL,
    [ExhibitId] int NULL,
    [MuseumId] int NOT NULL,
    [ActionType] nvarchar(30) NOT NULL,
    [ListeningDuration] int NULL,
    [LanguageUsed] varchar(10) NULL,
    [SearchQuery] nvarchar(200) NULL,
    [DeviceType] nvarchar(50) NULL,
    [SessionId] nvarchar(100) NULL,
    [IsOfflineEvent] bit NOT NULL,
    [EventTimestamp] datetime2 NOT NULL,
    [SyncedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Analytic__3214EC076218A530] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Analytics_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]),
    CONSTRAINT [FK_Analytics_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]),
    CONSTRAINT [FK_Analytics_Visitor] FOREIGN KEY ([VisitorId]) REFERENCES [Visitors] ([Id])
);

CREATE TABLE [Bookmarks] (
    [Id] int NOT NULL IDENTITY,
    [VisitorId] int NOT NULL,
    [ExhibitId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__Bookmark__3214EC0743E83E00] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Bookmarks_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Bookmarks_Visitor] FOREIGN KEY ([VisitorId]) REFERENCES [Visitors] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ContentChangeLogs] (
    [Id] int NOT NULL IDENTITY,
    [VersionId] int NOT NULL,
    [ExhibitId] int NULL,
    [ChangeType] nvarchar(20) NOT NULL,
    [EntityType] nvarchar(50) NOT NULL,
    [Description] nvarchar(500) NULL,
    [ChangedBy] int NULL,
    [ChangedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__ContentC__3214EC07D7F809D8] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChangeLog_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]),
    CONSTRAINT [FK_ChangeLog_User] FOREIGN KEY ([ChangedBy]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_ChangeLog_Version] FOREIGN KEY ([VersionId]) REFERENCES [ContentVersions] ([Id])
);

CREATE TABLE [ExhibitARAssets] (
    [Id] int NOT NULL IDENTITY,
    [ExhibitId] int NOT NULL,
    [AssetType] nvarchar(30) NOT NULL DEFAULT N'OverlayImage',
    [AssetUrl] nvarchar(500) NOT NULL,
    [FileSizeBytes] bigint NULL,
    [Width] int NULL,
    [Height] int NULL,
    [Description] nvarchar(255) NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__ExhibitA__3214EC07649477FB] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ARAssets_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ExhibitImages] (
    [Id] int NOT NULL IDENTITY,
    [ExhibitId] int NOT NULL,
    [ImageUrl] nvarchar(500) NOT NULL,
    [ThumbnailUrl] nvarchar(500) NULL,
    [Caption] nvarchar(500) NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__ExhibitI__3214EC07CB1C8F3E] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExhibitImages_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ExhibitionExhibits] (
    [ExhibitionId] int NOT NULL,
    [ExhibitId] int NOT NULL,
    CONSTRAINT [PK_ExhibitionExhibits] PRIMARY KEY ([ExhibitionId], [ExhibitId]),
    CONSTRAINT [FK_ExhibEx_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ExhibEx_Exhibition] FOREIGN KEY ([ExhibitionId]) REFERENCES [Exhibitions] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ExhibitMetadata] (
    [ExhibitId] int NOT NULL,
    [ThemeId] int NULL,
    [AgeGroupId] int NULL,
    [Era] nvarchar(100) NULL,
    [HistoricalEvent] nvarchar(200) NULL,
    CONSTRAINT [PK_ExhibitMetadata] PRIMARY KEY ([ExhibitId]),
    CONSTRAINT [FK_ExhibMeta_Age] FOREIGN KEY ([AgeGroupId]) REFERENCES [AgeGroups] ([Id]),
    CONSTRAINT [FK_ExhibMeta_Exhib] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ExhibMeta_Theme] FOREIGN KEY ([ThemeId]) REFERENCES [Themes] ([Id])
);

CREATE TABLE [ExhibitTranslations] (
    [Id] int NOT NULL IDENTITY,
    [ExhibitId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [Title] nvarchar(300) NOT NULL,
    [Description] nvarchar(max) NULL,
    [AudioUrl] nvarchar(500) NULL,
    [AudioDuration] int NULL,
    CONSTRAINT [PK__ExhibitT__3214EC074E3F7B08] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ExhibitTrans_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [TourRouteExhibits] (
    [Id] int NOT NULL IDENTITY,
    [TourRouteId] int NOT NULL,
    [ExhibitId] int NOT NULL,
    [StopOrder] int NOT NULL,
    [EstimatedMinutes] int NULL,
    CONSTRAINT [PK__TourRout__3214EC07028FAC02] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TourRouteExhibits_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]),
    CONSTRAINT [FK_TourRouteExhibits_Route] FOREIGN KEY ([TourRouteId]) REFERENCES [TourRoutes] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [VisitedExhibits] (
    [Id] int NOT NULL IDENTITY,
    [VisitorId] int NOT NULL,
    [ExhibitId] int NOT NULL,
    [MuseumId] int NOT NULL,
    [VisitedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__VisitedE__3214EC072F597FFC] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VisitedExhibits_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]),
    CONSTRAINT [FK_VisitedExhibits_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]),
    CONSTRAINT [FK_VisitedExhibits_Visitor] FOREIGN KEY ([VisitorId]) REFERENCES [Visitors] ([Id])
);

CREATE TABLE [PackageDownloads] (
    [Id] int NOT NULL IDENTITY,
    [PackageId] int NOT NULL,
    [VisitorId] int NULL,
    [DeviceType] nvarchar(50) NULL,
    [DownloadedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK__PackageD__3214EC0771127F31] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PkgDownloads_Package] FOREIGN KEY ([PackageId]) REFERENCES [OfflinePackages] ([Id]),
    CONSTRAINT [FK_PkgDownloads_Visitor] FOREIGN KEY ([VisitorId]) REFERENCES [Visitors] ([Id])
);

CREATE INDEX [IX_Analytics_ActionType] ON [AnalyticsLogs] ([ActionType]);

CREATE INDEX [IX_Analytics_EventTimestamp] ON [AnalyticsLogs] ([EventTimestamp]);

CREATE INDEX [IX_Analytics_Exhibit] ON [AnalyticsLogs] ([ExhibitId]);

CREATE INDEX [IX_Analytics_Museum] ON [AnalyticsLogs] ([MuseumId]);

CREATE INDEX [IX_AnalyticsLogs_VisitorId] ON [AnalyticsLogs] ([VisitorId]);

CREATE INDEX [IX_AuditLogs_CreatedAt] ON [AuditLogs] ([CreatedAt]);

CREATE INDEX [IX_AuditLogs_EntityType] ON [AuditLogs] ([EntityType]);

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);

CREATE INDEX [IX_Bookmarks_ExhibitId] ON [Bookmarks] ([ExhibitId]);

CREATE UNIQUE INDEX [UQ_Bookmarks] ON [Bookmarks] ([VisitorId], [ExhibitId]);

CREATE INDEX [IX_Categories_MuseumId] ON [Categories] ([MuseumId]);

CREATE INDEX [IX_Categories_ParentId] ON [Categories] ([ParentId]);

CREATE UNIQUE INDEX [UQ_CatTrans] ON [CategoryTranslations] ([CategoryId], [LanguageCode]);

CREATE INDEX [IX_ContentChangeLogs_ChangedBy] ON [ContentChangeLogs] ([ChangedBy]);

CREATE INDEX [IX_ContentChangeLogs_ExhibitId] ON [ContentChangeLogs] ([ExhibitId]);

CREATE INDEX [IX_ContentChangeLogs_VersionId] ON [ContentChangeLogs] ([VersionId]);

CREATE INDEX [IX_ContentVersions_PublishedBy] ON [ContentVersions] ([PublishedBy]);

CREATE UNIQUE INDEX [UQ_ContentVersion] ON [ContentVersions] ([MuseumId], [VersionNumber]);

CREATE INDEX [IX_ExhibitARAssets_ExhibitId] ON [ExhibitARAssets] ([ExhibitId]);

CREATE INDEX [IX_ExhibitImages_ExhibitId] ON [ExhibitImages] ([ExhibitId]);

CREATE INDEX [IX_ExhibitionExhibits_ExhibitId] ON [ExhibitionExhibits] ([ExhibitId]);

CREATE INDEX [IX_Exhibitions_MuseumId] ON [Exhibitions] ([MuseumId]);

CREATE UNIQUE INDEX [UQ_ExhibTrans] ON [ExhibitionTranslations] ([ExhibitionId], [LanguageCode]);

CREATE INDEX [IX_ExhibitMetadata_AgeGroupId] ON [ExhibitMetadata] ([AgeGroupId]);

CREATE INDEX [IX_ExhibitMetadata_ThemeId] ON [ExhibitMetadata] ([ThemeId]);

CREATE INDEX [IX_Exhibits_CategoryId] ON [Exhibits] ([CategoryId]);

CREATE INDEX [IX_Exhibits_CreatedBy] ON [Exhibits] ([CreatedBy]);

CREATE INDEX [IX_Exhibits_MapId] ON [Exhibits] ([MapId]);

CREATE INDEX [IX_Exhibits_MuseumId] ON [Exhibits] ([MuseumId]);

CREATE INDEX [IX_Exhibits_UpdatedBy] ON [Exhibits] ([UpdatedBy]);

CREATE UNIQUE INDEX [UQ__Exhibits__5204E7405196F227] ON [Exhibits] ([ExhibitCode]) WHERE [ExhibitCode] IS NOT NULL;

CREATE UNIQUE INDEX [UQ_ExhibitTrans] ON [ExhibitTranslations] ([ExhibitId], [LanguageCode]);

CREATE UNIQUE INDEX [UQ__Language__8B8C8A3446FA90F1] ON [Languages] ([LanguageCode]);

CREATE INDEX [IX_MapPOIs_MapId] ON [MapPOIs] ([MapId]);

CREATE INDEX [IX_MuseumLanguages_LanguageId] ON [MuseumLanguages] ([LanguageId]);

CREATE INDEX [IX_MuseumMaps_MuseumId] ON [MuseumMaps] ([MuseumId]);

CREATE UNIQUE INDEX [UQ_MuseumTrans] ON [MuseumTranslations] ([MuseumId], [LanguageCode]);

CREATE INDEX [IX_OfflinePackages_MuseumId] ON [OfflinePackages] ([MuseumId]);

CREATE INDEX [IX_OfflinePackages_VersionId] ON [OfflinePackages] ([VersionId]);

CREATE INDEX [IX_PackageDownloads_PackageId] ON [PackageDownloads] ([PackageId]);

CREATE INDEX [IX_PackageDownloads_VisitorId] ON [PackageDownloads] ([VisitorId]);

CREATE INDEX [IX_PaymentLogs_TransactionId] ON [PaymentLogs] ([TransactionId]);

CREATE UNIQUE INDEX [UQ__PaymentM__737584F65638694F] ON [PaymentMethods] ([Name]);

CREATE UNIQUE INDEX [UQ__Permissi__0FFDA357D7FD4E68] ON [Permissions] ([PermissionName]);

CREATE INDEX [IX_RefreshTokens_Token] ON [RefreshTokens] ([Token]);

CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);

CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);

CREATE UNIQUE INDEX [UQ__Roles__8A2B616097A109F1] ON [Roles] ([RoleName]);

CREATE INDEX [IX_SystemConfigurations_UpdatedBy] ON [SystemConfigurations] ([UpdatedBy]);

CREATE UNIQUE INDEX [UQ__SystemCo__4A3067846AFFD489] ON [SystemConfigurations] ([ConfigKey]);

CREATE INDEX [IX_Tickets_TicketTypeId] ON [Tickets] ([TicketTypeId]);

CREATE INDEX [IX_Tickets_TransactionId] ON [Tickets] ([TransactionId]);

CREATE INDEX [IX_Tickets_VisitorId] ON [Tickets] ([VisitorId]);

CREATE UNIQUE INDEX [UQ__Tickets__598CF7A3F437EC5B] ON [Tickets] ([TicketCode]);

CREATE INDEX [IX_TicketTypes_ExhibitionId] ON [TicketTypes] ([ExhibitionId]);

CREATE INDEX [IX_TicketTypes_MuseumId] ON [TicketTypes] ([MuseumId]);

CREATE INDEX [IX_TourRouteExhibits_ExhibitId] ON [TourRouteExhibits] ([ExhibitId]);

CREATE UNIQUE INDEX [UQ_TourRouteExhibits] ON [TourRouteExhibits] ([TourRouteId], [ExhibitId]);

CREATE INDEX [IX_TourRoutes_AgeGroupId] ON [TourRoutes] ([AgeGroupId]);

CREATE INDEX [IX_TourRoutes_MuseumId] ON [TourRoutes] ([MuseumId]);

CREATE INDEX [IX_TourRoutes_ThemeId] ON [TourRoutes] ([ThemeId]);

CREATE UNIQUE INDEX [UQ_TourRouteTrans] ON [TourRouteTranslations] ([TourRouteId], [LanguageCode]);

CREATE INDEX [IX_Transactions_PaymentMethodId] ON [Transactions] ([PaymentMethodId]);

CREATE INDEX [IX_Transactions_VisitorId] ON [Transactions] ([VisitorId]);

CREATE UNIQUE INDEX [UQ__Transact__999B5229FB610A1E] ON [Transactions] ([OrderCode]);

CREATE INDEX [IX_Users_MuseumId] ON [Users] ([MuseumId]);

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);

CREATE UNIQUE INDEX [UQ__Users__A9D10534AC9E29DD] ON [Users] ([Email]);

CREATE INDEX [IX_VisitedExhibits_Exhibit] ON [VisitedExhibits] ([ExhibitId]);

CREATE INDEX [IX_VisitedExhibits_MuseumId] ON [VisitedExhibits] ([MuseumId]);

CREATE INDEX [IX_VisitedExhibits_Visitor] ON [VisitedExhibits] ([VisitorId]);

CREATE UNIQUE INDEX [UQ__Visitors__49E123102804749D] ON [Visitors] ([DeviceId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260623015100_InitialCreate', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;

                IF NOT EXISTS (
                    SELECT * FROM sys.columns 
                    WHERE object_id = OBJECT_ID(N'[dbo].[Themes]') 
                    AND name = N'MuseumId'
                )
                BEGIN
                    ALTER TABLE [Themes] ADD [MuseumId] int NULL;
                    
                    EXEC('CREATE INDEX [IX_Themes_MuseumId] ON [Themes] ([MuseumId])');
                    
                    ALTER TABLE [Themes] ADD CONSTRAINT [FK_Themes_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]);
                END
            

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Categories]') AND [c].[name] = N'MuseumId');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Categories] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Categories] ALTER COLUMN [MuseumId] int NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260630043356_MakeCategoryMuseumIdNullable', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_ExhibMeta_Theme')
                    ALTER TABLE [ExhibitMetadata] DROP CONSTRAINT [FK_ExhibMeta_Theme];
            


                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TourRoutes_Theme')
                    ALTER TABLE [TourRoutes] DROP CONSTRAINT [FK_TourRoutes_Theme];
            


                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TourRoutes_ThemeId' AND object_id = OBJECT_ID('TourRoutes'))
                    DROP INDEX [IX_TourRoutes_ThemeId] ON [TourRoutes];
            


                IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ExhibitMetadata_ThemeId' AND object_id = OBJECT_ID('ExhibitMetadata'))
                    DROP INDEX [IX_ExhibitMetadata_ThemeId] ON [ExhibitMetadata];
            


                IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ThemeId' AND object_id = OBJECT_ID('TourRoutes'))
                    ALTER TABLE [TourRoutes] DROP COLUMN [ThemeId];
            


                IF EXISTS (SELECT 1 FROM sys.columns WHERE name = 'ThemeId' AND object_id = OBJECT_ID('ExhibitMetadata'))
                    ALTER TABLE [ExhibitMetadata] DROP COLUMN [ThemeId];
            

ALTER TABLE [Exhibitions] ADD [ThemeId] int NULL;

CREATE TABLE [TagGroups] (
    [Id] int NOT NULL IDENTITY,
    [GroupName] nvarchar(100) NOT NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK_TagGroups] PRIMARY KEY ([Id])
);

CREATE TABLE [Tags] (
    [Id] int NOT NULL IDENTITY,
    [TagGroupId] int NOT NULL,
    [TagName] nvarchar(100) NOT NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK_Tags] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Tags_TagGroup] FOREIGN KEY ([TagGroupId]) REFERENCES [TagGroups] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ExhibitTags] (
    [ExhibitId] int NOT NULL,
    [TagId] int NOT NULL,
    CONSTRAINT [PK_ExhibitTags] PRIMARY KEY ([ExhibitId], [TagId]),
    CONSTRAINT [FK_ExhibitTags_Exhibit] FOREIGN KEY ([ExhibitId]) REFERENCES [Exhibits] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ExhibitTags_Tag] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Exhibitions_ThemeId] ON [Exhibitions] ([ThemeId]);

CREATE INDEX [IX_ExhibitTags_TagId] ON [ExhibitTags] ([TagId]);

CREATE INDEX [IX_Tags_TagGroupId] ON [Tags] ([TagGroupId]);

ALTER TABLE [Exhibitions] ADD CONSTRAINT [FK_Exhibitions_Theme] FOREIGN KEY ([ThemeId]) REFERENCES [Themes] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260704174835_RestructureThemeAndAddTags', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
DROP TABLE [ContentChangeLogs];

DROP TABLE [MuseumLanguages];

DROP TABLE [MuseumTranslations];

DROP TABLE [PackageDownloads];

DROP TABLE [PaymentLogs];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260706162647_RemoveUnusedTables', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [TicketTypes] ADD [Status] nvarchar(20) NOT NULL DEFAULT N'Pending';

UPDATE TicketTypes SET Status = 'Approved';

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260713174237_AddStatusToTicketTypes', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Visitors] ADD [UserId] int NULL;

CREATE INDEX [IX_Visitors_UserId] ON [Visitors] ([UserId]);

ALTER TABLE [Visitors] ADD CONSTRAINT [FK_Visitors_User] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260717050924_AddUserIdToVisitors', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [TourRoutes] ADD [ExhibitionId] int NULL;

CREATE INDEX [IX_TourRoutes_ExhibitionId] ON [TourRoutes] ([ExhibitionId]);

ALTER TABLE [TourRoutes] ADD CONSTRAINT [FK_TourRoutes_Exhibition] FOREIGN KEY ([ExhibitionId]) REFERENCES [Exhibitions] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260725154600_AddExhibitionIdToTourRoute', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Exhibits]') AND [c].[name] = N'LocationX');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Exhibits] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Exhibits] DROP COLUMN [LocationX];

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Exhibits]') AND [c].[name] = N'LocationY');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Exhibits] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [Exhibits] DROP COLUMN [LocationY];

ALTER TABLE [Exhibits] ADD [RoomId] int NULL;

CREATE TABLE [Rooms] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [MapId] int NULL,
    [RoomCode] nvarchar(50) NOT NULL,
    [RoomName] nvarchar(150) NOT NULL,
    [FloorNumber] int NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK_Rooms] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Rooms_Map] FOREIGN KEY ([MapId]) REFERENCES [MuseumMaps] ([Id]),
    CONSTRAINT [FK_Rooms_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id])
);

CREATE INDEX [IX_Exhibits_RoomId] ON [Exhibits] ([RoomId]);

CREATE INDEX [IX_Rooms_MapId] ON [Rooms] ([MapId]);

CREATE INDEX [IX_Rooms_MuseumId] ON [Rooms] ([MuseumId]);

ALTER TABLE [Exhibits] ADD CONSTRAINT [FK_Exhibits_Room] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260730074348_CleanLocationFieldsAndAddRoomsTable', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Rooms] ADD [CenterX] float NULL;

ALTER TABLE [Rooms] ADD [CenterY] float NULL;

ALTER TABLE [Rooms] ADD [DoorWaypointId] int NULL;

ALTER TABLE [Rooms] ADD [WaypointId] nvarchar(50) NULL;

CREATE TABLE [WaypointEdges] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [FromWaypointId] nvarchar(50) NOT NULL,
    [ToWaypointId] nvarchar(50) NOT NULL,
    [Distance] float NOT NULL,
    [EdgeType] nvarchar(50) NOT NULL,
    [IsBidirectional] bit NOT NULL,
    CONSTRAINT [PK_WaypointEdges] PRIMARY KEY ([Id])
);

CREATE TABLE [Waypoints] (
    [Id] nvarchar(50) NOT NULL,
    [MuseumId] int NOT NULL,
    [FloorNumber] int NOT NULL,
    [X] float NOT NULL,
    [Y] float NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [RoomId] int NULL,
    [Label] nvarchar(100) NULL,
    CONSTRAINT [PK_Waypoints] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Waypoints_Museums_MuseumId] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Waypoints_Rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE SET NULL
);

CREATE INDEX [IX_Waypoints_MuseumId] ON [Waypoints] ([MuseumId]);

CREATE INDEX [IX_Waypoints_RoomId] ON [Waypoints] ([RoomId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260807185539_AddNavigationGraphTables', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [TicketPromotions] (
    [Id] int NOT NULL IDENTITY,
    [TicketTypeId] int NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [NameEn] nvarchar(200) NULL,
    [Description] nvarchar(500) NULL,
    [DescriptionEn] nvarchar(500) NULL,
    [DiscountType] nvarchar(20) NOT NULL DEFAULT N'Percentage',
    [DiscountValue] decimal(18,2) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    CONSTRAINT [PK_TicketPromotions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TicketPromotions_TicketType] FOREIGN KEY ([TicketTypeId]) REFERENCES [TicketTypes] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_TicketPromotions_TicketTypeId] ON [TicketPromotions] ([TicketTypeId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260810123429_AddTicketPromotions', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [TicketTypes] ADD [DescriptionEn] nvarchar(500) NULL;

ALTER TABLE [TicketTypes] ADD [NameEn] nvarchar(100) NULL;

CREATE TABLE [MuseumTranslations] (
    [Id] int NOT NULL IDENTITY,
    [MuseumId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [Address] nvarchar(500) NULL,
    [OpeningHours] nvarchar(500) NULL,
    CONSTRAINT [PK_MuseumTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MuseumTrans_Museum] FOREIGN KEY ([MuseumId]) REFERENCES [Museums] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [RoomTranslations] (
    [Id] int NOT NULL IDENTITY,
    [RoomId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [RoomName] nvarchar(150) NOT NULL,
    [Description] nvarchar(500) NULL,
    CONSTRAINT [PK_RoomTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RoomTrans_Room] FOREIGN KEY ([RoomId]) REFERENCES [Rooms] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [UQ_MuseumTrans] ON [MuseumTranslations] ([MuseumId], [LanguageCode]);

CREATE UNIQUE INDEX [UQ_RoomTrans] ON [RoomTranslations] ([RoomId], [LanguageCode]);


INSERT INTO RoomTranslations (RoomId, LanguageCode, RoomName, Description)
SELECT r.Id, 'vi', r.RoomName, r.Description
FROM Rooms r
WHERE NOT EXISTS (
    SELECT 1 FROM RoomTranslations t WHERE t.RoomId = r.Id AND t.LanguageCode = 'vi'
);



INSERT INTO MuseumTranslations (MuseumId, LanguageCode, Name, Description, Address, OpeningHours)
SELECT m.Id, 'vi', m.Name, m.Description, m.Address, m.OpeningHours
FROM Museums m
WHERE NOT EXISTS (
    SELECT 1 FROM MuseumTranslations t WHERE t.MuseumId = m.Id AND t.LanguageCode = 'vi'
);


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260810135617_SyncTranslationSnapshot', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Waypoints] ADD [MapId] int NULL;

CREATE INDEX [IX_Waypoints_MapId] ON [Waypoints] ([MapId]);

ALTER TABLE [Waypoints] ADD CONSTRAINT [FK_Waypoints_MuseumMaps_MapId] FOREIGN KEY ([MapId]) REFERENCES [MuseumMaps] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260811105331_AddMapIdToWaypoint', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Waypoints] ADD [Code] nvarchar(50) NULL;

ALTER TABLE [Waypoints] ADD [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate()));

ALTER TABLE [Waypoints] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate()));

ALTER TABLE [WaypointEdges] ADD [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate()));

ALTER TABLE [WaypointEdges] ADD [UpdatedAt] datetime2 NOT NULL DEFAULT ((getutcdate()));

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Rooms]') AND [c].[name] = N'DoorWaypointId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Rooms] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [Rooms] ALTER COLUMN [DoorWaypointId] nvarchar(50) NULL;

ALTER TABLE [MuseumMaps] ADD [MapType] nvarchar(50) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260812060540_AddRefinedEntitiesAndColumns', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Users] ADD [EmailVerificationToken] nvarchar(100) NULL;

ALTER TABLE [Users] ADD [IsEmailConfirmed] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Users] ADD [VerificationTokenExpiresAt] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260812062924_AddEmailVerificationToUser', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [ExhibitMetadata] ADD [EraEn] nvarchar(100) NULL;

ALTER TABLE [ExhibitMetadata] ADD [HistoricalEventEn] nvarchar(200) NULL;

CREATE TABLE [TagTranslations] (
    [Id] int NOT NULL IDENTITY,
    [TagId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [TagName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_TagTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TagTrans_Tag] FOREIGN KEY ([TagId]) REFERENCES [Tags] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ThemeTranslations] (
    [Id] int NOT NULL IDENTITY,
    [ThemeId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [ThemeName] nvarchar(100) NOT NULL,
    [Description] nvarchar(255) NULL,
    CONSTRAINT [PK_ThemeTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThemeTrans_Theme] FOREIGN KEY ([ThemeId]) REFERENCES [Themes] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [UQ_TagTrans] ON [TagTranslations] ([TagId], [LanguageCode]);

CREATE UNIQUE INDEX [UQ_ThemeTrans] ON [ThemeTranslations] ([ThemeId], [LanguageCode]);


INSERT INTO ThemeTranslations (ThemeId, LanguageCode, ThemeName, Description)
SELECT t.Id, 'vi', t.ThemeName, t.Description
FROM Themes t
WHERE NOT EXISTS (
    SELECT 1 FROM ThemeTranslations x WHERE x.ThemeId = t.Id AND x.LanguageCode = 'vi'
);

INSERT INTO TagTranslations (TagId, LanguageCode, TagName)
SELECT tg.Id, 'vi', tg.TagName
FROM Tags tg
WHERE NOT EXISTS (
    SELECT 1 FROM TagTranslations x WHERE x.TagId = tg.Id AND x.LanguageCode = 'vi'
);


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260813105736_AddThemeTagMetadataTranslations', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Tickets] ADD [Price] decimal(18,2) NOT NULL DEFAULT 0.0;

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ExhibitARAssets]') AND [c].[name] = N'AssetType');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [ExhibitARAssets] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [ExhibitARAssets] ADD DEFAULT N'Model3D' FOR [AssetType];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260830102706_AddPriceToTickets', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [TagGroupTranslations] (
    [Id] int NOT NULL IDENTITY,
    [TagGroupId] int NOT NULL,
    [LanguageCode] varchar(10) NOT NULL,
    [GroupName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_TagGroupTranslations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TagGroupTrans_TagGroup] FOREIGN KEY ([TagGroupId]) REFERENCES [TagGroups] ([Id]) ON DELETE CASCADE
);

CREATE UNIQUE INDEX [UQ_TagGroupTrans] ON [TagGroupTranslations] ([TagGroupId], [LanguageCode]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260908214527_AddTagGroupTranslations', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE TABLE [TicketRefundRequests] (
    [Id] int NOT NULL IDENTITY,
    [TicketId] int NOT NULL,
    [VisitorId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [BankName] nvarchar(100) NOT NULL,
    [AccountNumber] nvarchar(50) NOT NULL,
    [AccountHolderName] nvarchar(100) NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [RejectReason] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT ((getutcdate())),
    [ProcessedAt] datetime2 NULL,
    CONSTRAINT [PK_TicketRefundRequests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TicketRefundRequests_Ticket] FOREIGN KEY ([TicketId]) REFERENCES [Tickets] ([Id]),
    CONSTRAINT [FK_TicketRefundRequests_Visitor] FOREIGN KEY ([VisitorId]) REFERENCES [Visitors] ([Id])
);

CREATE INDEX [IX_TicketRefundRequests_TicketId] ON [TicketRefundRequests] ([TicketId]);

CREATE INDEX [IX_TicketRefundRequests_VisitorId] ON [TicketRefundRequests] ([VisitorId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260918103704_AddTicketRefundRequests', N'10.0.8');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [OfflinePackages] ADD [ExhibitionId] int NULL;

ALTER TABLE [OfflinePackages] ADD [PackageName] nvarchar(255) NULL;

CREATE INDEX [IX_OfflinePackages_ExhibitionId] ON [OfflinePackages] ([ExhibitionId]);

ALTER TABLE [OfflinePackages] ADD CONSTRAINT [FK_OfflinePackages_Exhibitions] FOREIGN KEY ([ExhibitionId]) REFERENCES [Exhibitions] ([Id]) ON DELETE SET NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260921092022_AddExhibitionIdToOfflinePackages', N'10.0.8');

COMMIT;
GO

