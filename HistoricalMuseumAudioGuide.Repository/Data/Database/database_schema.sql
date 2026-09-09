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
    IsEmailConfirmed BIT             NOT NULL DEFAULT 0,
    EmailVerificationToken NVARCHAR(100) NULL,
    VerificationTokenExpiresAt DATETIME2 NULL,
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

CREATE TABLE MuseumTranslations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    LanguageCode    VARCHAR(10)     NOT NULL,
    Name            NVARCHAR(200)   NOT NULL,
    Description     NVARCHAR(MAX)   NULL,
    Address         NVARCHAR(500)   NULL,
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
    MapType         NVARCHAR(50)    NULL,
    MapImageUrl     NVARCHAR(500)   NOT NULL,
    Width           INT             NULL, -- Original image width for coordinate calculations
    Height          INT             NULL,
    IsDefault       BIT             NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_MuseumMaps_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id) ON DELETE CASCADE
);

CREATE TABLE Rooms (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    MapId           INT             NULL,
    RoomCode        NVARCHAR(50)    NOT NULL,
    RoomName        NVARCHAR(150)   NOT NULL,
    FloorNumber     INT             NOT NULL DEFAULT 1,
    Description     NVARCHAR(500)   NULL,
    DoorWaypointId  NVARCHAR(50)    NULL,
    WaypointId      NVARCHAR(50)    NULL,
    CenterX         FLOAT           NULL,
    CenterY         FLOAT           NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Rooms_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_Rooms_Map FOREIGN KEY (MapId) REFERENCES MuseumMaps(Id)
);

CREATE TABLE RoomTranslations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    RoomId          INT             NOT NULL,
    LanguageCode    VARCHAR(10)     NOT NULL,
    RoomName        NVARCHAR(150)   NOT NULL,
    Description     NVARCHAR(500)   NULL,
    CONSTRAINT FK_RoomTrans_Room FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_RoomTrans UNIQUE (RoomId, LanguageCode)
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

CREATE TABLE Waypoints (
    Id              NVARCHAR(50)    PRIMARY KEY,
    MuseumId        INT             NOT NULL,
    MapId           INT             NULL,
    FloorNumber     INT             NOT NULL DEFAULT 1,
    X               FLOAT           NOT NULL,
    Y               FLOAT           NOT NULL,
    Type            NVARCHAR(50)    NOT NULL DEFAULT 'HALLWAY',
    RoomId          INT             NULL,
    Code            NVARCHAR(50)    NULL,
    Label           NVARCHAR(100)   NULL,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Waypoints_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_Waypoints_MuseumMaps FOREIGN KEY (MapId) REFERENCES MuseumMaps(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Waypoints_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE SET NULL
);

CREATE TABLE WaypointEdges (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    MuseumId        INT             NOT NULL DEFAULT 1,
    FromWaypointId  NVARCHAR(50)    NOT NULL,
    ToWaypointId    NVARCHAR(50)    NOT NULL,
    Distance        FLOAT           NOT NULL DEFAULT 1.0,
    EdgeType        NVARCHAR(50)    NOT NULL DEFAULT 'WALK',
    IsBidirectional BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_WaypointEdges_FromWaypoint FOREIGN KEY (FromWaypointId) REFERENCES Waypoints(Id),
    CONSTRAINT FK_WaypointEdges_ToWaypoint FOREIGN KEY (ToWaypointId) REFERENCES Waypoints(Id)
);

ALTER TABLE Rooms
ADD CONSTRAINT FK_Rooms_Waypoints FOREIGN KEY (DoorWaypointId) REFERENCES Waypoints(Id) ON DELETE SET NULL;

ALTER TABLE Rooms
ADD CONSTRAINT FK_Rooms_Waypoint FOREIGN KEY (WaypointId) REFERENCES Waypoints(Id) ON DELETE SET NULL;

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

CREATE TABLE ThemeTranslations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    ThemeId         INT             NOT NULL,
    LanguageCode    VARCHAR(10)     NOT NULL,
    ThemeName       NVARCHAR(100)   NOT NULL,
    Description     NVARCHAR(255)   NULL,
    CONSTRAINT FK_ThemeTrans_Theme FOREIGN KEY (ThemeId) REFERENCES Themes(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_ThemeTrans UNIQUE (ThemeId, LanguageCode)
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
    RoomId          INT             NULL,          -- Reference to Rooms
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
    CONSTRAINT FK_Exhibits_Room FOREIGN KEY (RoomId) REFERENCES Rooms(Id),
    CONSTRAINT FK_Exhibits_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    CONSTRAINT FK_Exhibits_UpdatedBy FOREIGN KEY (UpdatedBy) REFERENCES Users(Id)
);

-- Link Exhibits to Metadata (ThemeId removed — Theme now links to Exhibition, not Exhibit)
CREATE TABLE ExhibitMetadata (
    ExhibitId       INT NOT NULL,
    AgeGroupId      INT NULL,
    Era             NVARCHAR(100)   NULL, -- e.g., 'Lý', 'Trần', 'Lê'
    EraEn           NVARCHAR(100)   NULL,
    HistoricalEvent NVARCHAR(200)   NULL, -- e.g., 'Chiến dịch Điện Biên Phủ'
    HistoricalEventEn NVARCHAR(200)   NULL,
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
    AssetType       NVARCHAR(30)    NOT NULL DEFAULT 'Model3D'
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

CREATE TABLE TagTranslations (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TagId           INT             NOT NULL,
    LanguageCode    VARCHAR(10)     NOT NULL,
    TagName         NVARCHAR(100)   NOT NULL,
    CONSTRAINT FK_TagTrans_Tag FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_TagTrans UNIQUE (TagId, LanguageCode)
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
    ExhibitionId    INT             NULL,  -- NULL = general tour, SET = exhibition-specific tour
    IsDefault       BIT             NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active'
                    CHECK (Status IN ('Active', 'Inactive')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TourRoutes_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_TourRoutes_AgeGroup FOREIGN KEY (AgeGroupId) REFERENCES AgeGroups(Id),
    CONSTRAINT FK_TourRoutes_Exhibition FOREIGN KEY (ExhibitionId) REFERENCES Exhibitions(Id)
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
    UserId          INT             NULL,             -- Linked User account
    FirstSeenAt     DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    LastSeenAt      DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Visitors_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- ============================================================
-- 12. PAYMENT METHODS
-- ============================================================

CREATE TABLE PaymentMethods (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(50)    NOT NULL UNIQUE, -- 'PAYOS', 'MOMO', 'CASH'
    DisplaynName     NVARCHAR(100)   NOT NULL,        -- 'PayOS Payment Gateway'
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
    NameEn          NVARCHAR(100)   NULL,
    Price           DECIMAL(18,2)   NOT NULL DEFAULT 0,
    Description     NVARCHAR(500)   NULL,
    DescriptionEn   NVARCHAR(500)   NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Pending'
                    CHECK (Status IN ('Pending', 'Approved', 'Rejected')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TicketTypes_Museum FOREIGN KEY (MuseumId) REFERENCES Museums(Id),
    CONSTRAINT FK_TicketTypes_Exhibition FOREIGN KEY (ExhibitionId) REFERENCES Exhibitions(Id)
);

CREATE TABLE TicketPromotions (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    TicketTypeId    INT             NOT NULL,
    Name            NVARCHAR(200)   NOT NULL,
    NameEn          NVARCHAR(200)   NULL,
    Description     NVARCHAR(500)   NULL,
    DescriptionEn   NVARCHAR(500)   NULL,
    DiscountType    NVARCHAR(20)    NOT NULL DEFAULT 'Percentage'
                    CHECK (DiscountType IN ('Percentage', 'FixedAmount')),
    DiscountValue   DECIMAL(18,2)   NOT NULL,
    StartDate       DATETIME2       NOT NULL,
    EndDate         DATETIME2       NOT NULL,
    IsActive        BIT             NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_TicketPromotions_TicketType FOREIGN KEY (TicketTypeId) REFERENCES TicketTypes(Id) ON DELETE CASCADE
);

CREATE TABLE Tickets (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    VisitorId       INT             NOT NULL,
    TicketTypeId    INT             NOT NULL,
    TransactionId   INT             NULL,
    TicketCode      NVARCHAR(100)   UNIQUE NOT NULL,
    Price           DECIMAL(18,2)   NOT NULL DEFAULT 0,
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
-- SCHEMA COMPLETION
-- ============================================================

PRINT 'Database schema created successfully!';
GO