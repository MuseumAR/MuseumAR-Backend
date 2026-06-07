-- ============================================================
-- DATABASE: MuseumAudioGuide
-- Project: Historical Site and Museum Audio Guide with Simple AR
-- Code: SU26SE165
-- ============================================================

-- CREATE DATABASE MuseumAudioGuide;
-- GO
-- USE MuseumAudioGuide;
-- GO

-- ============================================================
-- 1. ROLES & PERMISSIONS
-- ============================================================

CREATE TABLE Roles (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoleName        NVARCHAR(50)    NOT NULL UNIQUE,
    Description     NVARCHAR(255)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Permissions (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PermissionName  NVARCHAR(100)   NOT NULL UNIQUE,
    Description     NVARCHAR(255)   NULL,
    Module          NVARCHAR(50)    NULL  -- e.g. 'Exhibit', 'Analytics', 'User', 'System'
);

CREATE TABLE RolePermissions (
    RoleId          INT NOT NULL,
    PermissionId    INT NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id),
    CONSTRAINT FK_RolePermissions_Permission FOREIGN KEY (PermissionId) REFERENCES Permissions(Id)
);

-- ============================================================
-- 2. LANGUAGES
-- ============================================================

CREATE TABLE Languages (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    LanguageCode    VARCHAR(10)     NOT NULL UNIQUE,   -- 'vi', 'en', 'ja', 'ko'
    LanguageName    NVARCHAR(50)    NOT NULL,           -- 'Vietnamese', 'English'
    NativeName      NVARCHAR(50)    NULL,               -- 'Tiếng Việt', 'English'
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- 3. USERS (CMS Users: Admin, Content Manager, Museum Manager)
-- ============================================================

CREATE TABLE Users (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(100)   NOT NULL,
    Email           NVARCHAR(255)   NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(500)   NOT NULL,
    PhoneNumber     NVARCHAR(20)    NULL,
    AvatarUrl       NVARCHAR(500)   NULL,
    RoleId          INT             NOT NULL,
    MuseumId        INT             NULL,  -- FK added after Museums table
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active'
                    CHECK (Status IN ('Active', 'Inactive', 'Suspended')),
    LastLoginAt     DATETIME2       NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Users_Role FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- ============================================================
-- 4. MUSEUMS
-- ============================================================

CREATE TABLE Museums (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(200)   NOT NULL,
    Description     NVARCHAR(MAX)   NULL,
    Address         NVARCHAR(500)   NULL,
    City            NVARCHAR(100)   NULL,
    Province        NVARCHAR(100)   NULL,
    Country         NVARCHAR(100)   NULL DEFAULT N'Vietnam',
    Latitude        DECIMAL(10,7)   NULL,
    Longitude       DECIMAL(10,7)   NULL,
    ThumbnailUrl    NVARCHAR(500)   NULL,
    OpeningHours    NVARCHAR(500)   NULL,
    ContactPhone    NVARCHAR(20)    NULL,
    ContactEmail    NVARCHAR(255)   NULL,
    Website         NVARCHAR(500)   NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active'
                    CHECK (Status IN ('Active', 'Inactive', 'Maintenance')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);

-- Add FK from Users to Museums
ALTER TABLE Users
ADD CONSTRAINT FK_Users_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id);

-- Museum-Language support (which languages each museum supports)
CREATE TABLE MuseumLanguages (
    MuseumId        INT NOT NULL,
    LanguageId      INT NOT NULL,
    IsDefault       BIT NOT NULL DEFAULT 0,
    CONSTRAINT PK_MuseumLanguages PRIMARY KEY (MuseumId, LanguageId),
    CONSTRAINT FK_MuseumLang_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_MuseumLang_Language FOREIGN KEY (LanguageId) REFERENCES Languages(Id)
);

-- Museum translations (multilingual museum info)
CREATE TABLE MuseumTranslations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    LanguageCode    VARCHAR(10)     NOT NULL,
    Name            NVARCHAR(200)   NOT NULL,
    Description     NVARCHAR(MAX)   NULL,
    OpeningHours    NVARCHAR(500)   NULL,
    CONSTRAINT FK_MuseumTrans_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_MuseumTrans UNIQUE (MuseumId, LanguageCode)
);

-- ============================================================
-- 4.1. MUSEUM MAPS & POIs (2D Map support)
-- ============================================================

CREATE TABLE MuseumMaps (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    FloorNumber     INT             NOT NULL DEFAULT 1,
    MapName         NVARCHAR(100)   NULL, -- e.g., 'Ground Floor', '2nd Floor - Modern History'
    MapImageUrl     NVARCHAR(500)   NOT NULL,
    Width           INT             NULL, -- Original image width for coordinate calculations
    Height          INT             NULL,
    IsDefault       BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_MuseumMaps_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id) ON DELETE CASCADE
);

CREATE TABLE MapPOIs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MapId           INT             NOT NULL,
    POIType         NVARCHAR(50)    NOT NULL, -- 'WC', 'Exit', 'Elevator', 'TicketCounter', 'Information', 'Stairs'
    LocationX       FLOAT           NOT NULL, -- X coordinate percentage (0-100)
    LocationY       FLOAT           NOT NULL, -- Y coordinate percentage (0-100)
    Description     NVARCHAR(250)   NULL,
    CONSTRAINT FK_MapPOIs_Map FOREIGN KEY (MapId) REFERENCES MuseumMaps(Id) ON DELETE CASCADE
);

-- ============================================================
-- 4.2. EXHIBITIONS & EVENTS
-- ============================================================

CREATE TABLE Exhibitions (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    ThumbnailUrl    NVARCHAR(500)   NULL,
    StartDate       DATETIME2       NULL,
    EndDate         DATETIME2       NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active' 
                    CHECK (Status IN ('Active', 'Inactive', 'Ended')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Exhibitions_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id)
);

CREATE TABLE ExhibitionTranslations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ExhibitionId    INT             NOT NULL,
    LanguageCode    VARCHAR(10)     NOT NULL,
    Name            NVARCHAR(200)   NOT NULL,
    Description     NVARCHAR(MAX)   NULL,
    CONSTRAINT FK_ExhibTrans_Exhibition FOREIGN KEY (ExhibitionId) REFERENCES Exhibitions(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ExhibTrans UNIQUE (ExhibitionId, LanguageCode)
);

-- ============================================================
-- 5. CATEGORIES / COLLECTIONS
-- ============================================================

CREATE TABLE Categories (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    ParentId        INT             NULL,  -- self-referencing for sub-categories
    SortOrder       INT             NOT NULL DEFAULT 0,
    IconUrl         NVARCHAR(500)   NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active'
                    CHECK (Status IN ('Active', 'Inactive')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Categories_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentId) REFERENCES Categories(Id)
);

CREATE TABLE CategoryTranslations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId      INT             NOT NULL,
    LanguageCode    VARCHAR(10)     NOT NULL,
    CategoryName    NVARCHAR(100)   NOT NULL,
    Description     NVARCHAR(500)   NULL,
    CONSTRAINT FK_CatTrans_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_CatTrans UNIQUE (CategoryId, LanguageCode)
);

-- ============================================================
-- 5.1. PERSONALIZATION METADATA (Themes, Age Groups)
-- ============================================================

CREATE TABLE Themes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ThemeName       NVARCHAR(100)   NOT NULL,
    Description     NVARCHAR(255)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE AgeGroups (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    GroupName       NVARCHAR(50)    NOT NULL, -- 'Trẻ em', 'Học sinh', 'Người lớn'
    MinAge          INT             NULL,
    MaxAge          INT             NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- 6. EXHIBITS
-- ============================================================

CREATE TABLE Exhibits (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    CategoryId      INT             NULL,
    ExhibitCode     NVARCHAR(50)    NULL UNIQUE,  -- internal code
    QRCodeData      NVARCHAR(500)   NULL,         -- QR code encoded string
    QRCodeImageUrl  NVARCHAR(500)   NULL,         -- QR code image file
    ThumbnailUrl    NVARCHAR(500)   NULL,
    AROverlayUrl    NVARCHAR(500)   NULL,          -- AR overlay image
    ARMarkerUrl     NVARCHAR(500)   NULL,          -- AR marker/target image
    MapId           INT             NULL,          -- Reference to MuseumMaps for 2D map
    LocationX       FLOAT           NULL,          -- X coordinate percentage (0-100) on the map
    LocationY       FLOAT           NULL,          -- Y coordinate percentage (0-100) on the map
    SortOrder       INT             NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Draft'
                    CHECK (Status IN ('Draft', 'Published', 'Unpublished', 'Archived')),
    PublishedAt     DATETIME2       NULL,
    CreatedBy       INT             NULL,
    UpdatedBy       INT             NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Exhibits_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_Exhibits_Category FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
    CONSTRAINT FK_Exhibits_Map FOREIGN KEY (MapId) REFERENCES MuseumMaps(Id),
    CONSTRAINT FK_Exhibits_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Exhibits_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- Link Exhibits to Metadata
CREATE TABLE ExhibitMetadata (
    ExhibitId       INT NOT NULL,
    ThemeId         INT NULL,
    AgeGroupId      INT NULL,
    Era             NVARCHAR(100)   NULL, -- e.g., 'Lý', 'Trần', 'Lê'
    HistoricalEvent NVARCHAR(200)   NULL, -- e.g., 'Chiến dịch Điện Biên Phủ'
    CONSTRAINT PK_ExhibitMetadata PRIMARY KEY (ExhibitId),
    CONSTRAINT FK_ExhibMeta_Exhib FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ExhibMeta_Theme FOREIGN KEY (ThemeId) REFERENCES Themes(Id),
    CONSTRAINT FK_ExhibMeta_Age FOREIGN KEY (AgeGroupId) REFERENCES AgeGroups(Id)
);

-- Link Exhibits to Exhibitions
CREATE TABLE ExhibitionExhibits (
    ExhibitionId    INT NOT NULL,
    ExhibitId       INT NOT NULL,
    CONSTRAINT PK_ExhibitionExhibits PRIMARY KEY (ExhibitionId, ExhibitId),
    CONSTRAINT FK_ExhibEx_Exhibition FOREIGN KEY (ExhibitionId) REFERENCES Exhibitions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ExhibEx_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id) ON DELETE CASCADE
);

-- ============================================================
-- 7. EXHIBIT TRANSLATIONS (Multilingual content)
-- ============================================================

CREATE TABLE ExhibitTranslations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ExhibitId       INT             NOT NULL,
    LanguageCode    VARCHAR(10)     NOT NULL,
    Title           NVARCHAR(300)   NOT NULL,
    Description     NVARCHAR(MAX)   NULL,
    AudioUrl        NVARCHAR(500)   NULL,
    AudioDuration   INT             NULL,  -- duration in seconds
    CONSTRAINT FK_ExhibitTrans_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ExhibitTrans UNIQUE (ExhibitId, LanguageCode)
);

-- ============================================================
-- 8. EXHIBIT IMAGES (Historical photos/documents)
-- ============================================================

CREATE TABLE ExhibitImages (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ExhibitId       INT             NOT NULL,
    ImageUrl        NVARCHAR(500)   NOT NULL,
    ThumbnailUrl    NVARCHAR(500)   NULL,
    Caption         NVARCHAR(500)   NULL,
    SortOrder       INT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ExhibitImages_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id) ON DELETE CASCADE
);

-- ============================================================
-- 9. EXHIBIT AR ASSETS
-- ============================================================

CREATE TABLE ExhibitARAssets (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ExhibitId       INT             NOT NULL,
    AssetType       NVARCHAR(30)    NOT NULL DEFAULT 'OverlayImage'
                    CHECK (AssetType IN ('OverlayImage', 'MarkerImage', 'Model3D')),
    AssetUrl        NVARCHAR(500)   NOT NULL,
    FileSizeBytes   BIGINT          NULL,
    Width           INT             NULL,
    Height          INT             NULL,
    Description     NVARCHAR(255)   NULL,
    SortOrder       INT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ARAssets_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id) ON DELETE CASCADE
);

-- ============================================================
-- 10. TOUR ROUTES (Suggested tour routes)
-- ============================================================

CREATE TABLE TourRoutes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    EstimatedMinutes INT            NULL,
    ThumbnailUrl    NVARCHAR(500)   NULL,
    AgeGroupId      INT             NULL,  -- Personalization
    ThemeId         INT             NULL,  -- Personalization
    IsDefault       BIT             NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active'
                    CHECK (Status IN ('Active', 'Inactive')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TourRoutes_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_TourRoutes_AgeGroup FOREIGN KEY (AgeGroupId) REFERENCES AgeGroups(Id),
    CONSTRAINT FK_TourRoutes_Theme FOREIGN KEY (ThemeId) REFERENCES Themes(Id)
);

CREATE TABLE TourRouteTranslations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TourRouteId     INT             NOT NULL,
    LanguageCode    VARCHAR(10)     NOT NULL,
    RouteName       NVARCHAR(200)   NOT NULL,
    Description     NVARCHAR(MAX)   NULL,
    CONSTRAINT FK_TourRouteTrans_Route FOREIGN KEY (TourRouteId) REFERENCES TourRoutes(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_TourRouteTrans UNIQUE (TourRouteId, LanguageCode)
);

CREATE TABLE TourRouteExhibits (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TourRouteId     INT             NOT NULL,
    ExhibitId       INT             NOT NULL,
    StopOrder       INT             NOT NULL,  -- order in the route
    EstimatedMinutes INT            NULL,
    CONSTRAINT FK_TourRouteExhibits_Route FOREIGN KEY (TourRouteId) REFERENCES TourRoutes(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TourRouteExhibits_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id),
    CONSTRAINT UQ_TourRouteExhibits UNIQUE (TourRouteId, ExhibitId)
);

-- ============================================================
-- 11. VISITORS (Mobile app users - can be anonymous or registered)
-- ============================================================

CREATE TABLE Visitors (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    DeviceId        NVARCHAR(255)   NOT NULL UNIQUE,  -- unique device identifier
    DisplayName     NVARCHAR(100)   NULL,
    Email           NVARCHAR(255)   NULL,
    PreferredLang   VARCHAR(10)     NOT NULL DEFAULT 'vi',
    DeviceType      NVARCHAR(50)    NULL,  -- 'Android', 'iOS'
    DeviceModel     NVARCHAR(100)   NULL,
    AppVersion      NVARCHAR(20)    NULL,
    FirstSeenAt     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    LastSeenAt      DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- 12. PAYMENT METHODS
-- ============================================================

CREATE TABLE PaymentMethods (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(50)    NOT NULL UNIQUE, -- 'VNPAY', 'MOMO', 'CASH'
    DisplayName     NVARCHAR(100)   NOT NULL,        -- 'VNPay Payment Gateway'
    Description     NVARCHAR(255)   NULL,
    IconUrl         NVARCHAR(500)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);

-- ============================================================
-- 13. TRANSACTIONS & TICKETING
-- ============================================================

CREATE TABLE Transactions (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    VisitorId       INT             NOT NULL,
    PaymentMethodId INT             NOT NULL,
    OrderCode       NVARCHAR(50)    NOT NULL UNIQUE,
    TotalAmount     DECIMAL(18,2)   NOT NULL,
    Currency        NVARCHAR(10)    NOT NULL DEFAULT 'VND',
    PaymentStatus   NVARCHAR(20)    NOT NULL DEFAULT 'Pending'
                    CHECK (PaymentStatus IN ('Pending', 'Completed', 'Failed', 'Refunded', 'Cancelled')),
    GatewayTransactionId NVARCHAR(100) NULL,
    PaymentDate     DATETIME2       NULL,
    Description     NVARCHAR(500)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Transactions_Visitor FOREIGN KEY (VisitorId) REFERENCES Visitors(Id),
    CONSTRAINT FK_Transactions_Method FOREIGN KEY (PaymentMethodId) REFERENCES PaymentMethods(Id)
);

CREATE TABLE TicketTypes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    ExhibitionId    INT             NULL, -- If NULL, it's a general museum admission ticket
    Name            NVARCHAR(100)   NOT NULL,
    Price           DECIMAL(18,2)   NOT NULL DEFAULT 0,
    Description     NVARCHAR(500)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TicketTypes_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_TicketTypes_Exhibition FOREIGN KEY (ExhibitionId) REFERENCES Exhibitions(Id)
);

CREATE TABLE Tickets (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    VisitorId       INT             NOT NULL,
    TicketTypeId    INT             NOT NULL,
    TransactionId   INT             NULL,
    TicketCode      NVARCHAR(100)   UNIQUE NOT NULL,
    PurchaseDate    DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    ValidDate       DATETIME2       NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Pending'
                    CHECK (Status IN ('Pending', 'Paid', 'Used', 'Cancelled', 'Expired')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Tickets_Visitor FOREIGN KEY (VisitorId) REFERENCES Visitors(Id),
    CONSTRAINT FK_Tickets_Type FOREIGN KEY (TicketTypeId) REFERENCES TicketTypes(Id),
    CONSTRAINT FK_Tickets_Transaction FOREIGN KEY (TransactionId) REFERENCES Transactions(Id)
);

CREATE TABLE PaymentLogs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TransactionId   INT             NOT NULL,
    RawResponse     NVARCHAR(MAX)   NULL, -- JSON response from gateway
    LogMessage      NVARCHAR(500)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_PaymentLogs_Transaction FOREIGN KEY (TransactionId) REFERENCES Transactions(Id)
);

-- ============================================================
-- 14. BOOKMARKS
-- ============================================================

CREATE TABLE Bookmarks (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    VisitorId       INT             NOT NULL,
    ExhibitId       INT             NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Bookmarks_Visitor FOREIGN KEY (VisitorId) REFERENCES Visitors(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Bookmarks_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Bookmarks UNIQUE (VisitorId, ExhibitId)
);

-- ============================================================
-- 15. VISITED EXHIBITS (Track visited exhibits per visitor)
-- ============================================================

CREATE TABLE VisitedExhibits (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    VisitorId       INT             NOT NULL,
    ExhibitId       INT             NOT NULL,
    MuseumId        INT             NOT NULL,
    VisitedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_VisitedExhibits_Visitor FOREIGN KEY (VisitorId) REFERENCES Visitors(Id),
    CONSTRAINT FK_VisitedExhibits_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id),
    CONSTRAINT FK_VisitedExhibits_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id)
);

CREATE INDEX IX_VisitedExhibits_Visitor ON VisitedExhibits(VisitorId);
CREATE INDEX IX_VisitedExhibits_Exhibit ON VisitedExhibits(ExhibitId);

-- ============================================================
-- 16. ANALYTICS LOGS
-- ============================================================

CREATE TABLE AnalyticsLogs (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    VisitorId       INT             NULL,
    ExhibitId       INT             NULL,
    MuseumId        INT             NOT NULL,
    ActionType      NVARCHAR(30)    NOT NULL
                    CHECK (ActionType IN (
                        'QR_SCAN', 'AUDIO_PLAY', 'AUDIO_PAUSE', 'AUDIO_COMPLETE',
                        'AR_VIEW', 'EXHIBIT_VIEW', 'BOOKMARK_ADD', 'BOOKMARK_REMOVE',
                        'SEARCH', 'MAP_VIEW', 'ROUTE_VIEW', 'PACKAGE_DOWNLOAD',
                        'LANGUAGE_SWITCH', 'APP_OPEN', 'APP_CLOSE'
                    )),
    ListeningDuration INT          NULL,  -- seconds (for audio events)
    LanguageUsed    VARCHAR(10)     NULL,
    SearchQuery     NVARCHAR(200)   NULL,  -- for SEARCH action
    DeviceType      NVARCHAR(50)    NULL,
    SessionId       NVARCHAR(100)   NULL,
    IsOfflineEvent  BIT             NOT NULL DEFAULT 0,
    EventTimestamp  DATETIME2       NOT NULL,  -- actual time on device
    SyncedAt        DATETIME2       NULL,      -- when synced to server
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Analytics_Visitor FOREIGN KEY (VisitorId) REFERENCES Visitors(Id),
    CONSTRAINT FK_Analytics_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id),
    CONSTRAINT FK_Analytics_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id)
);

CREATE INDEX IX_Analytics_Museum ON AnalyticsLogs(MuseumId);
CREATE INDEX IX_Analytics_Exhibit ON AnalyticsLogs(ExhibitId);
CREATE INDEX IX_Analytics_ActionType ON AnalyticsLogs(ActionType);
CREATE INDEX IX_Analytics_EventTimestamp ON AnalyticsLogs(EventTimestamp);

-- ============================================================
-- 17. CONTENT VERSIONS
-- ============================================================

CREATE TABLE ContentVersions (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    VersionNumber   NVARCHAR(20)    NOT NULL,  -- e.g. '1.0.0', '1.0.1'
    ChangeDescription NVARCHAR(MAX) NULL,
    TotalExhibits   INT             NULL,
    TotalMediaFiles INT             NULL,
    PackageSizeBytes BIGINT         NULL,
    PublishedBy     INT             NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Draft'
                    CHECK (Status IN ('Draft', 'Published', 'Deprecated')),
    PublishedAt     DATETIME2       NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ContentVersions_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_ContentVersions_Publisher FOREIGN KEY (PublishedBy) REFERENCES Users(Id),
    CONSTRAINT UQ_ContentVersion UNIQUE (MuseumId, VersionNumber)
);

-- ============================================================
-- 18. CONTENT CHANGE LOG (track individual content changes)
-- ============================================================

CREATE TABLE ContentChangeLogs (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    VersionId       INT             NOT NULL,
    ExhibitId       INT             NULL,
    ChangeType      NVARCHAR(20)    NOT NULL
                    CHECK (ChangeType IN ('Added', 'Updated', 'Deleted')),
    EntityType      NVARCHAR(50)    NOT NULL,  -- 'Exhibit', 'Audio', 'Image', 'ARAsset'
    Description     NVARCHAR(500)   NULL,
    ChangedBy       INT             NULL,
    ChangedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_ChangeLog_Version FOREIGN KEY (VersionId) REFERENCES ContentVersions(Id),
    CONSTRAINT FK_ChangeLog_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id),
    CONSTRAINT FK_ChangeLog_User FOREIGN KEY (ChangedBy) REFERENCES Users(Id)
);

-- ============================================================
-- 19. OFFLINE PACKAGES
-- ============================================================

CREATE TABLE OfflinePackages (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    VersionId       INT             NOT NULL,
    PackageUrl      NVARCHAR(500)   NOT NULL,
    PackageSizeBytes BIGINT         NOT NULL,
    Checksum        NVARCHAR(128)   NULL,  -- SHA256 for integrity
    AudioCount      INT             NULL,
    ImageCount      INT             NULL,
    ARAssetCount    INT             NULL,
    ExhibitCount    INT             NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Building'
                    CHECK (Status IN ('Building', 'Available', 'Deprecated', 'Failed')),
    BuiltAt         DATETIME2       NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_OfflinePackages_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_OfflinePackages_Version FOREIGN KEY (VersionId) REFERENCES ContentVersions(Id)
);

-- ============================================================
-- 20. PACKAGE DOWNLOADS (track offline package downloads)
-- ============================================================

CREATE TABLE PackageDownloads (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    PackageId       INT             NOT NULL,
    VisitorId       INT             NULL,
    DeviceType      NVARCHAR(50)    NULL,
    DownloadedAt    DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_PkgDownloads_Package FOREIGN KEY (PackageId) REFERENCES OfflinePackages(Id),
    CONSTRAINT FK_PkgDownloads_Visitor FOREIGN KEY (VisitorId) REFERENCES Visitors(Id)
);

-- ============================================================
-- 21. SYSTEM CONFIGURATIONS
-- ============================================================

CREATE TABLE SystemConfigurations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ConfigKey       NVARCHAR(100)   NOT NULL UNIQUE,
    ConfigValue     NVARCHAR(MAX)   NOT NULL,
    Description     NVARCHAR(255)   NULL,
    UpdatedBy       INT             NULL,
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_SysConfig_User FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- ============================================================
-- 22. AUDIT LOGS (System audit trail)
-- ============================================================

CREATE TABLE AuditLogs (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT             NULL,
    Action          NVARCHAR(50)    NOT NULL,  -- 'CREATE', 'UPDATE', 'DELETE', 'LOGIN', 'LOGOUT'
    EntityType      NVARCHAR(50)    NOT NULL,
    EntityId        INT             NULL,
    OldValues       NVARCHAR(MAX)   NULL,  -- JSON
    NewValues       NVARCHAR(MAX)   NULL,  -- JSON
    IpAddress       NVARCHAR(45)    NULL,
    UserAgent       NVARCHAR(500)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType);
CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);

-- ============================================================
-- 23. REFRESH TOKENS (for JWT auth)
-- ============================================================

CREATE TABLE RefreshTokens (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT             NOT NULL,
    Token           NVARCHAR(500)   NOT NULL,
    ExpiresAt       DATETIME2       NOT NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    RevokedAt       DATETIME2       NULL,
    ReplacedByToken NVARCHAR(500)   NULL,
    CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);

-- ============================================================
-- SEED DATA
-- ============================================================

-- Roles
INSERT INTO Roles (RoleName, Description) VALUES
    ('SystemAdmin',      N'Quản trị hệ thống'),
    ('MuseumManager',    N'Quản lý bảo tàng - xem analytics'),
    ('ContentManager',   N'Quản lý nội dung hiện vật');

-- Default Languages
INSERT INTO Languages (LanguageCode, LanguageName, NativeName, IsActive) VALUES
    ('vi', 'Vietnamese', N'Tiếng Việt', 1),
    ('en', 'English',    N'English',     1);

-- Default Payment Methods
INSERT INTO PaymentMethods (Name, DisplayName, Description) VALUES
    ('VNPAY', N'Cổng thanh toán VNPay', N'Thanh toán qua ứng dụng ngân hàng, ví điện tử'),
    ('MOMO',  N'Ví điện tử MoMo',       N'Thanh toán qua ứng dụng MoMo'),
    ('CASH',  N'Tiền mặt',              N'Thanh toán trực tiếp tại quầy');

-- Default Permissions
INSERT INTO Permissions (PermissionName, Description, Module) VALUES
    -- Exhibit
    ('exhibit.create',    N'Tạo hiện vật mới',           'Exhibit'),
    ('exhibit.read',      N'Xem thông tin hiện vật',      'Exhibit'),
    ('exhibit.update',    N'Cập nhật hiện vật',           'Exhibit'),
    ('exhibit.delete',    N'Xóa hiện vật',               'Exhibit'),
    ('exhibit.publish',   N'Publish/Unpublish hiện vật',  'Exhibit'),
    -- Media
    ('media.upload',      N'Upload audio/image/AR',       'Media'),
    ('media.delete',      N'Xóa media',                  'Media'),
    -- QR
    ('qr.generate',       N'Tạo QR code',                'QR'),
    -- Analytics
    ('analytics.view',    N'Xem analytics dashboard',     'Analytics'),
    ('analytics.export',  N'Xuất báo cáo analytics',      'Analytics'),
    -- User Management
    ('user.manage',       N'Quản lý tài khoản',          'User'),
    ('role.manage',       N'Quản lý phân quyền',         'User'),
    -- System
    ('system.config',     N'Cấu hình hệ thống',          'System'),
    ('system.language',   N'Quản lý ngôn ngữ',           'System'),
    ('museum.manage',     N'Quản lý bảo tàng',           'Museum'),
    -- Content Version
    ('content.version',   N'Quản lý content version',     'Content'),
    ('package.manage',    N'Quản lý offline package',      'Content'),
    -- Ticketing & Payment
    ('ticket.manage',     N'Quản lý vé',                 'Finance'),
    ('payment.view',      N'Xem lịch sử giao dịch',       'Finance');

-- SystemAdmin gets all permissions
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 1, Id FROM Permissions;

-- MuseumManager gets analytics + read + finance
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 2, Id FROM Permissions
WHERE PermissionName IN ('exhibit.read', 'analytics.view', 'analytics.export', 'ticket.manage', 'payment.view');

-- ContentManager gets exhibit + media + qr + content
INSERT INTO RolePermissions (RoleId, PermissionId)
SELECT 3, Id FROM Permissions
WHERE PermissionName IN (
    'exhibit.create', 'exhibit.read', 'exhibit.update', 'exhibit.delete', 'exhibit.publish',
    'media.upload', 'media.delete', 'qr.generate', 'content.version', 'package.manage'
);

-- Default System Configurations
INSERT INTO SystemConfigurations (ConfigKey, ConfigValue, Description) VALUES
    ('max_audio_size_mb',       '50',       N'Dung lượng tối đa file audio (MB)'),
    ('max_image_size_mb',       '10',       N'Dung lượng tối đa file image (MB)'),
    ('max_ar_asset_size_mb',    '20',       N'Dung lượng tối đa AR asset (MB)'),
    ('supported_audio_formats', 'mp3,wav,aac', N'Định dạng audio hỗ trợ'),
    ('supported_image_formats', 'jpg,jpeg,png,webp', N'Định dạng image hỗ trợ'),
    ('default_language',        'vi',       N'Ngôn ngữ mặc định'),
    ('analytics_sync_interval', '300',      N'Khoảng thời gian sync analytics (giây)'),
    ('package_compression',     'gzip',     N'Phương thức nén package');

PRINT 'Database schema updated successfully with Events, Ticketing, Personalization and Payment!';
GO
