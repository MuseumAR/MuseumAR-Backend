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
    PasswordResetToken NVARCHAR(100) NULL,
    ResetTokenExpiresAt DATETIME2   NULL,
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
-- 4.2. THEMES, EXHIBITIONS & EVENTS (Themes as Exhibition categories)
-- ============================================================
CREATE TABLE Themes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NULL, -- nullable, null means global/system theme
    ThemeName       NVARCHAR(100)   NOT NULL,
    Description     NVARCHAR(255)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Themes_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id)
); 

CREATE TABLE Exhibitions (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    ThemeId         INT             NULL,  -- FK to Themes: categorizes exhibition by reusable theme
    ThumbnailUrl    NVARCHAR(500)   NULL,
    StartDate       DATETIME2       NULL,
    EndDate         DATETIME2       NULL,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active' 
                    CHECK (Status IN ('Active', 'Inactive', 'Ended')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Exhibitions_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_Exhibitions_Theme FOREIGN KEY (ThemeId) REFERENCES Themes(Id)
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
    MuseumId        INT             NULL, -- nullable, null means global/system category
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
-- 5.1. PERSONALIZATION METADATA (Age Groups)
-- ============================================================

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

-- Link Exhibits to Metadata (ThemeId removed — Theme now links to Exhibition, not Exhibit)
CREATE TABLE ExhibitMetadata (
    ExhibitId       INT NOT NULL,
    AgeGroupId      INT NULL,
    Era             NVARCHAR(100)   NULL, -- e.g., 'Lý', 'Trần', 'Lê'
    HistoricalEvent NVARCHAR(200)   NULL, -- e.g., 'Chiến dịch Điện Biên Phủ'
    CONSTRAINT PK_ExhibitMetadata PRIMARY KEY (ExhibitId),
    CONSTRAINT FK_ExhibMeta_Exhib FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id) ON DELETE CASCADE,
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
-- 9.1. TAGS & TAG GROUPS (Faceted search/filter for exhibits)
-- ============================================================

CREATE TABLE TagGroups (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    GroupName       NVARCHAR(100)   NOT NULL,  -- e.g., 'Thời kỳ', 'Chất liệu', 'Chủ đề'
    SortOrder       INT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Tags (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TagGroupId      INT             NOT NULL,
    TagName         NVARCHAR(100)   NOT NULL,  -- e.g., 'Thời Trần', 'Gốm', 'Chiến tranh'
    SortOrder       INT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Tags_TagGroup FOREIGN KEY (TagGroupId) REFERENCES TagGroups(Id) ON DELETE CASCADE
);

CREATE TABLE ExhibitTags (
    ExhibitId       INT NOT NULL,
    TagId           INT NOT NULL,
    CONSTRAINT PK_ExhibitTags PRIMARY KEY (ExhibitId, TagId),
    CONSTRAINT FK_ExhibitTags_Exhibit FOREIGN KEY (ExhibitId) REFERENCES Exhibits(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ExhibitTags_Tag FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
);

-- ============================================================
-- 10. TOUR ROUTES (Suggested tour routes)
-- ThemeId removed — personalization via Tags instead
-- ============================================================

CREATE TABLE TourRoutes (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    EstimatedMinutes INT            NULL,
    ThumbnailUrl    NVARCHAR(500)   NULL,
    AgeGroupId      INT             NULL,  -- Personalization
    IsDefault       BIT             NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active'
                    CHECK (Status IN ('Active', 'Inactive')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TourRoutes_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_TourRoutes_AgeGroup FOREIGN KEY (AgeGroupId) REFERENCES AgeGroups(Id)
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
    ('ContentManager',   N'Quản lý nội dung hiện vật'),
    ('Visitor', N'Khách vãng lai');

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

-- ============================================================
-- SEED DATA CHUẨN XÁC: BẢO TÀNG THÀNH PHỐ HỒ CHÍ MINH
-- Các tài khoản có password là 123456
-- ============================================================

USE MuseumAudioGuide;
GO

-- 1. CHÈN BẢO TÀNG DUY NHẤT (Id = 1)
-- Lưu ý: Đối chiếu bảng Museums (Name, Description, Address, City...)
SET IDENTITY_INSERT Museums ON;
INSERT INTO Museums (Id, Name, Description, Address, City, Province, Country, Latitude, Longitude, ThumbnailUrl, OpeningHours, ContactPhone, ContactEmail, Website, Status, CreatedAt, UpdatedAt)
VALUES (1, 
        N'Bảo tàng Thành phố Hồ Chí Minh', 
        N'Nơi lưu giữ báu vật lịch sử, văn hóa sài gòn qua các thời kỳ, tọa lạc tại tòa nhà dinh Gia Long xưa.', 
        N'65 Lý Tự Trọng, Bến Nghé, Quận 1', 
        N'Thành phố Hồ Chí Minh', N'Thành phố Hồ Chí Minh', N'Vietnam', 
        10.776111, 106.699722, 
        'https://cdn.museum.gov.vn/thumbnails/hcm-museum.jpg', 
        N'08:00 AM - 05:00 PM', '02838299465', 'info@baotangtphcm.vn', 'https://baotangtphcm.vn',
        'Active', GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT Museums OFF;

-- 2. CHÈN TÀI KHOẢN CMS (Bảng Users liên kết Role và MuseumId = 1)
-- Mật khẩu giả định chuỗi Hash của '123456'
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, FullName, Email, PasswordHash, PhoneNumber, AvatarUrl, RoleId, MuseumId, Status, CreatedAt, UpdatedAt)
VALUES 
(1, N'Nguyễn Văn Admin', 'admin@baotangtphcm.vn', '$2a$11$0OOHejSvnDYOJfbKl0GYxOKCuhy.YMhOKuaCItMF00prbBZpiNelq', '0901234567', 'https://cdn.museum.gov.vn/avatars/admin.png', 1, 1, 'Active', GETUTCDATE(), GETUTCDATE()), -- SystemAdmin
(2, N'Trần Thị Quản Lý', 'manager@baotangtphcm.vn', '$2a$11$0OOHejSvnDYOJfbKl0GYxOKCuhy.YMhOKuaCItMF00prbBZpiNelq', '0907654321', 'https://cdn.museum.gov.vn/avatars/manager.png', 2, 1, 'Active', GETUTCDATE(), GETUTCDATE()), -- MuseumManager
(3, N'Lê Văn Nội Dung', 'content@baotangtphcm.vn', '$2a$11$0OOHejSvnDYOJfbKl0GYxOKCuhy.YMhOKuaCItMF00prbBZpiNelq', '0908888888', 'https://cdn.museum.gov.vn/avatars/content.png', 3, 1, 'Active', GETUTCDATE(), GETUTCDATE()); -- ContentManager
SET IDENTITY_INSERT Users OFF;

-- 3. CHÈN BẢN ĐỒ CÁC TẦNG (Bảng MuseumMaps sử dụng MapImageUrl)
SET IDENTITY_INSERT MuseumMaps ON;
INSERT INTO MuseumMaps (Id, MuseumId, FloorNumber, MapName, MapImageUrl, Width, Height, IsDefault, CreatedAt, UpdatedAt)
VALUES 
(1, 1, 0, N'Bản đồ Tầng trệt', 'https://cdn.museum.gov.vn/maps/hcm-ground-floor.png', 1920, 1080, 1, GETUTCDATE(), GETUTCDATE()),
(2, 1, 1, N'Bản đồ Lầu 1', 'https://cdn.museum.gov.vn/maps/hcm-first-floor.png', 1920, 1080, 0, GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT MuseumMaps OFF;

-- 4. CHÈN CÁC ĐIỂM ĐỊNH VỊ TIỆN ÍCH (Bảng MapPOIs)
INSERT INTO MapPOIs (MapId, POIType, LocationX, LocationY, Description)
VALUES 
(1, 'TicketCounter', 15.5, 10.2, N'Quầy bán vé sảnh chính tầng trệt'),
(1, 'WC', 85.0, 90.5, N'Nhà vệ sinh khu vực Khảo cổ'),
(2, 'Exit', 50.0, 5.0, N'Lối thoát hiểm ban công phía Tây');

-- 5. CHÈN CHỦ ĐỀ & TRIỂN LÃM (Bảng Themes và Exhibitions)
SET IDENTITY_INSERT Themes ON;
INSERT INTO Themes (Id, MuseumId, ThemeName, Description, CreatedAt)
VALUES 
(1, 1, N'Lịch sử Sài Gòn - Gia Định', N'Quá trình hình thành và phát triển đô thị', GETUTCDATE()),
(2, 1, N'Kháng chiến thế kỷ XX', N'Phong trào cách mạng địa phương', GETUTCDATE());
SET IDENTITY_INSERT Themes OFF;

SET IDENTITY_INSERT Exhibitions ON;
INSERT INTO Exhibitions (Id, MuseumId, ThemeId, ThumbnailUrl, StartDate, EndDate, Status, CreatedAt, UpdatedAt)
VALUES 
(1, 1, 1, 'https://cdn.museum.gov.vn/exhibitions/thien-nhien.jpg', '2026-01-01', '2026-12-31', 'Active', GETUTCDATE(), GETUTCDATE()),
(2, 1, 2, 'https://cdn.museum.gov.vn/exhibitions/khang-chien.jpg', '2026-05-01', '2026-10-31', 'Active', GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT Exhibitions OFF;

INSERT INTO ExhibitImages (ExhibitId, ImageUrl, Caption, SortOrder) VALUES 
(1, 'https://api.museumar.vn/images/exhibits/details/trong_dong_mat_tren.jpg', N'Hoa văn ngôi sao 14 cánh trên mặt trống', 1);

INSERT INTO ExhibitionTranslations (ExhibitionId, LanguageCode, Name, Description)
VALUES 
(1, 'vi', N'Triển lãm Thiên nhiên và Khảo cổ đất Sài Gòn', N'Minh chứng về địa chất, sinh thái cổ sơ.'),
(1, 'en', N'Saigon Nature and Archaeology Exhibition', N'Evidence of ancient geology and ecology.'),
(2, 'vi', N'Triển lãm Đấu tranh chính trị 1954 - 1975', N'Tái hiện các phong trào học sinh sinh viên nội thành.'),
(2, 'en', N'Political Resistance 1954 - 1975', N'Showcasing urban student movements.');

-- 6. CHÈN DANH MỤC HIỆN VẬT (Bảng Categories)
SET IDENTITY_INSERT Categories ON;
INSERT INTO Categories (Id, MuseumId, ParentId, SortOrder, IconUrl, Status, CreatedAt, UpdatedAt)
VALUES 
(1, 1, NULL, 1, 'https://cdn.museum.gov.vn/icons/archeology.png', 'Active', GETUTCDATE(), GETUTCDATE()),
(2, 1, NULL, 2, 'https://cdn.museum.gov.vn/icons/weapons.png', 'Active', GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT Categories OFF;

INSERT INTO CategoryTranslations (CategoryId, LanguageCode, CategoryName, Description)
VALUES 
(1, 'vi', N'Cổ vật Khảo cổ', N'Công cụ đá, gốm, mộ táng cổ'),
(1, 'en', N'Archaeological Artifacts', N'Stone tools, ancient pottery'),
(2, 'vi', N'Vũ khí Lịch sử', N'Phương tiện, súng pháo chiến tranh'),
(2, 'en', 'Historical Weapons', 'Military firearms and equipment');

-- 7. CHÈN PHÂN KHÚC ĐỘ TUỔI CÁ NHÂN HÓA (Bảng AgeGroups)
SET IDENTITY_INSERT AgeGroups ON;
INSERT INTO AgeGroups (Id, GroupName, MinAge, MaxAge, CreatedAt)
VALUES 
(1, N'Trẻ em & Học sinh', 6, 17, GETUTCDATE()),
(2, N'Người lớn', 18, 59, GETUTCDATE());
SET IDENTITY_INSERT AgeGroups OFF;

-- 8. CHÈN HIỆN VẬT (Bảng Exhibits - Đối chiếu chính xác các cột vị trí)
SET IDENTITY_INSERT Exhibits ON;
INSERT INTO Exhibits (Id, MuseumId, CategoryId, ExhibitCode, QRCodeData, QRCodeImageUrl, ThumbnailUrl, MapId, LocationX, LocationY, SortOrder, Status, PublishedAt, CreatedBy, UpdatedBy, CreatedAt, UpdatedAt)
VALUES 
(1, 1, 1, 'EX-HCM-001', 'MUSEUM_HCM_EX001_SECRET', 'https://cdn.museum.gov.vn/qrs/ex001.png', 'https://cdn.museum.gov.vn/exhibits/mo-chum.jpg', 1, 35.2, 45.8, 1, 'Published', GETUTCDATE(), 3, 3, GETUTCDATE(), GETUTCDATE()),
(2, 1, 2, 'EX-HCM-002', 'MUSEUM_HCM_EX002_SECRET', 'https://cdn.museum.gov.vn/qrs/ex002.png', 'https://cdn.museum.gov.vn/exhibits/uh1.jpg', 2, 60.1, 22.4, 2, 'Published', GETUTCDATE(), 3, 3, GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT Exhibits OFF;

-- Bảng ExhibitMetadata phụ thuộc
INSERT INTO ExhibitMetadata (ExhibitId, AgeGroupId, Era, HistoricalEvent)
VALUES 
(1, 1, N'Thời đại đồ đồng thau (Văn hóa Đồng Nai)', N'Thời tiền sử Nam Bộ'),
(2, 2, N'Kháng chiến chống Mỹ (1954-1975)', N'Chiến dịch Hồ Chí Minh 1975');

-- Bảng trung gian nối Hiện vật vào Triển lãm
INSERT INTO ExhibitionExhibits (ExhibitionId, ExhibitId)
VALUES (1, 1), (2, 2);

-- 9. CHÈN THUYẾT MINH ĐA NGÔN NGỮ (Bảng ExhibitTranslations)
INSERT INTO ExhibitTranslations (ExhibitId, LanguageCode, Title, Description, AudioUrl, AudioDuration)
VALUES 
(1, 'vi', N'Mộ chum Khảo cổ học Dốc Chùa', N'Mộ chum bằng đất nung có niên đại khoảng 2.500 năm trước, minh chứng cho táng thức độc đáo cư dân cổ.', 'https://cdn.museum.gov.vn/audio/vi/mo-chum.mp3', 185),
(1, 'en', N'Doc Chua Archaeological Jar Burial', N'A terracotta burial jar dating back 2,500 years ago, showcasing ancient traditions.', 'https://cdn.museum.gov.vn/audio/en/mo-chum.mp3', 195),
(2, 'vi', N'Máy bay trực thăng chiến lợi phẩm UH-1', N'Chiếc trực thăng thu giữ từ quân đội đối phương, biểu tượng chiến thắng năm 1975.', 'https://cdn.museum.gov.vn/audio/vi/uh1.mp3', 120),
(2, 'en', N'Captured UH-1 Huey Helicopter', N'A helicopter seized during the 1975 spring offensive, a symbol of historical victory.', 'https://cdn.museum.gov.vn/audio/en/uh1.mp3', 132);

-- 10. CHÈN TÀI NGUYÊN AR THỰC TẾ ẢO (Bảng ExhibitARAssets sử dụng AssetUrl chung)
INSERT INTO ExhibitARAssets (ExhibitId, AssetType, AssetUrl, FileSizeBytes, Width, Height, Description, SortOrder, CreatedAt)
VALUES 
(1, 'MarkerImage', 'https://cdn.museum.gov.vn/ar/markers/mo-chum.jpg', 102450, 800, 800, N'Ảnh target nhận diện mộ chum', 1, GETUTCDATE()),
(1, 'Model3D', 'https://cdn.museum.gov.vn/ar/models/mo-chum-3d.glb', 15428900, NULL, NULL, N'Mô hình 3D đám mây điểm của mộ chum', 2, GETUTCDATE()),
(2, 'MarkerImage', 'https://cdn.museum.gov.vn/ar/markers/uh1.jpg', 254800, 1024, 768, N'Ảnh target nhận diện trực thăng UH1', 1, GETUTCDATE()),
(2, 'Model3D', 'https://cdn.museum.gov.vn/ar/models/uh1-helicopter.glb', 32451200, NULL, NULL, N'Mô hình khối kỹ thuật trực thăng UH1', 2, GETUTCDATE());

-- 11. CHÈN ĐỊNH NGHĨA LOẠI VÉ (Bảng TicketTypes sử dụng cột Name thay vì TypeName)
SET IDENTITY_INSERT TicketTypes ON;
INSERT INTO TicketTypes (Id, MuseumId, ExhibitionId, Name, Price, Description, IsActive, CreatedAt, UpdatedAt)
VALUES 
(1, 1, NULL, N'Vé vào cổng phổ thông', 30000.00, N'Áp dụng tham quan toàn bộ khu vực cố định', 1, GETUTCDATE(), GETUTCDATE()),
(2, 1, 2, N'Vé chuyên đề Kháng Chiến đặc biệt', 50000.00, N'Bao gồm lối đi sảnh chuyên đề và tặng kèm tai nghe', 1, GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT TicketTypes OFF;

-- 12. CHÈN PHIÊN BẢN VÀ GÓI ĐỒNG BỘ OFFLINE (Bảng ContentVersions và OfflinePackages)
SET IDENTITY_INSERT ContentVersions ON;
INSERT INTO ContentVersions (Id, MuseumId, VersionNumber, ChangeDescription, TotalExhibits, TotalMediaFiles, PackageSizeBytes, PublishedBy, Status, PublishedAt, CreatedAt)
VALUES 
(1, 1, 'v1.0.0', N'Khởi tạo gói dữ liệu gốc cho Bảo tàng TPHCM bao gồm tầng trệt và lầu 1.', 2, 4, 48133350, 3, 'Published', GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT ContentVersions OFF;

SET IDENTITY_INSERT OfflinePackages ON;
INSERT INTO OfflinePackages (Id, MuseumId, VersionId, PackageUrl, PackageSizeBytes, Checksum, AudioCount, ImageCount, ARAssetCount, ExhibitCount, Status, BuiltAt, CreatedAt)
VALUES 
(1, 1, 1, 'https://cdn.museum.gov.vn/offline/hcm_museum_v100.zip', 48133350, 'SHA256_7F8A9B2C3D4E5F6G7H8I9J0K', 4, 4, 4, 2, 'Available', GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT OfflinePackages OFF;

-- 13. KHÁCH THAM QUAN APP MOBILE (VISITORS)
INSERT INTO Visitors (DeviceId, DisplayName, Email, PreferredLang, DeviceType, DeviceModel, AppVersion) VALUES 
('F39B672A-8811-4E1B-9473-D683A648AA29', N'Đức Mạnh', 'visitor.manh@gmail.com', 'vi', 'iOS', 'iPhone 15 Pro', '1.0.0'),
('A28D471C-9922-4F2A-8361-C234E128BB88', 'John Doe', 'johndoe@gmail.com', 'en', 'Android', 'Samsung S24 Ultra', '1.0.0');

-- 14. CHÈN BẢNG TUYẾN THAM QUAN (TourRoutes)
-- Tuyến 1: Dành cho Học sinh (AgeGroupId = 1), ước tính 45 phút, là tuyến mặc định (IsDefault = 1)
-- Tuyến 2: Dành cho Người lớn (AgeGroupId = 2), ước tính 60 phút
SET IDENTITY_INSERT TourRoutes ON;
INSERT INTO TourRoutes (Id, MuseumId, EstimatedMinutes, ThumbnailUrl, AgeGroupId, IsDefault, Status, CreatedAt, UpdatedAt)
VALUES 
(1, 1, 45, 'https://cdn.museum.gov.vn/routes/hcm-student-tour.jpg', 1, 1, 'Active', GETUTCDATE(), GETUTCDATE()),
(2, 1, 60, 'https://cdn.museum.gov.vn/routes/hcm-history-tour.jpg', 2, 0, 'Active', GETUTCDATE(), GETUTCDATE());
SET IDENTITY_INSERT TourRoutes OFF;


-- 15. CHÈN BẢNG DỊCH THUẬT TUYẾN THAM QUAN (TourRouteTranslations)
-- Cung cấp đa ngôn ngữ (vi, en) cho cả 2 tuyến vừa tạo
INSERT INTO TourRouteTranslations (TourRouteId, LanguageCode, RouteName, Description)
VALUES 
-- Tuyến 1 (Tiếng Việt)
(1, 'vi', N'Hành trình Khám phá Lịch sử xanh (Dành cho Học sinh)', 
 N'Tuyến tham quan được thiết kế ngắn gọn, trực quan, tập trung vào các cổ vật khảo cổ đất Sài Gòn xưa và các hoạt động trải nghiệm tương tác thực tế ảo tăng cường AR.'),
-- Tuyến 1 (Tiếng Anh)
(1, 'en', N'Green History Discovery Tour (For Students)', 
 N'A concise, highly visual tour optimized for student groups, focusing on ancient Saigon archaeological artifacts and interactive AR multimedia experiences.'),

-- Tuyến 2 (Tiếng Việt)
(2, 'vi', N'Sài Gòn - Gia Định: Từ Đô thị cổ đến Thành phố Anh hùng', 
 N'Tuyến đi chuyên sâu xuyên suốt từ tầng trệt lên lầu 1, giúp khách tham quan cái nhìn toàn cảnh từ thời tiền sử, giai đoạn phát triển thương cảng đến cuộc kháng chiến cứu nước vĩ đại.'),
-- Tuyến 2 (Tiếng Anh)
(2, 'en', N'Saigon - Gia Dinh: From Ancient Town to Heroic City', 
 N'An in-depth historical timeline tour guiding visitors from prehistory through early commercial trading eras, culminating in the major 20th-century revolutionary resistance movements.');


-- 16. CHÈN BẢNG CHI TIẾT CÁC ĐIỂM DỪNG CỦA TUYẾN (TourRouteExhibits)
-- Liên kết các hiện vật (ExhibitId 1: Mộ chum, ExhibitId 2: Máy bay UH-1) vào các tuyến theo thứ tự (StopOrder)
INSERT INTO TourRouteExhibits (TourRouteId, ExhibitId, StopOrder, EstimatedMinutes)
VALUES 
-- Tuyến 1 (Học sinh): Ưu tiên xem Mộ chum khảo cổ học trước
(1, 1, 1, 15), -- Điểm dừng 1: Mộ chum (Xem trong 15 phút)
(1, 2, 2, 20), -- Điểm dừng 2: Máy bay UH-1 (Xem trong 20 phút)

-- Tuyến 2 (Người lớn): Đi theo trình tự thời gian từ cổ chí kim
(2, 1, 1, 20), -- Điểm dừng 1: Mộ chum (Xem trong 20 phút)
(2, 2, 2, 25); -- Điểm dừng 2: Máy bay UH-1 (Xem trong 25 phút)

PRINT 'Seed data cho Bảo tàng Thành phố Hồ Chí Minh (Single-Museum) đã được chèn hoàn tất và chính xác với Schema!';
GO