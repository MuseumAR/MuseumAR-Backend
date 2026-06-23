using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoricalMuseumAudioGuide.Repository.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgeGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MinAge = table.Column<int>(type: "int", nullable: true),
                    MaxAge = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AgeGroup__3214EC073267C0C0", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    LanguageName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NativeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Language__3214EC075A095684", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Museums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Province = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, defaultValue: "Vietnam"),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OpeningHours = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Museums__3214EC07F64D086B", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentM__3214EC07DA605523", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PermissionName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Permissi__3214EC07E1106079", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__3214EC077E061479", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ThemeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Themes__3214EC074D21EDC9", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Visitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PreferredLang = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "vi"),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeviceModel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AppVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FirstSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Visitors__3214EC0718CE3833", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Categori__3214EC07A1949F6A", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Categories_Parent",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Exhibitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Exhibiti__3214EC0743051CE0", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exhibitions_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MuseumLanguages",
                columns: table => new
                {
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuseumLanguages", x => new { x.MuseumId, x.LanguageId });
                    table.ForeignKey(
                        name: "FK_MuseumLang_Language",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MuseumLang_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MuseumMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    FloorNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MapName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MapImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MuseumMa__3214EC077DAA1EC5", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuseumMaps_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MuseumTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpeningHours = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MuseumTr__3214EC077CD8E139", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuseumTrans_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permission",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolePermissions_Role",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    MuseumId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResetTokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__3214EC07BE722AC8", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Role",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TourRoutes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AgeGroupId = table.Column<int>(type: "int", nullable: true),
                    ThemeId = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TourRout__3214EC07F00F9A99", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourRoutes_AgeGroup",
                        column: x => x.AgeGroupId,
                        principalTable: "AgeGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TourRoutes_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TourRoutes_Theme",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitorId = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "VND"),
                    PaymentStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    GatewayTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transact__3214EC07634B4F68", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Method",
                        column: x => x.PaymentMethodId,
                        principalTable: "PaymentMethods",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transactions_Visitor",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CategoryTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Category__3214EC079C960808", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatTrans_Category",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExhibitionTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExhibitionId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Exhibiti__3214EC07A48C3DBB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExhibTrans_Exhibition",
                        column: x => x.ExhibitionId,
                        principalTable: "Exhibitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    ExhibitionId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TicketTy__3214EC075DC34A40", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTypes_Exhibition",
                        column: x => x.ExhibitionId,
                        principalTable: "Exhibitions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TicketTypes_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MapPOIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MapId = table.Column<int>(type: "int", nullable: false),
                    POIType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LocationX = table.Column<double>(type: "float", nullable: false),
                    LocationY = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MapPOIs__3214EC07011CA550", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MapPOIs_Map",
                        column: x => x.MapId,
                        principalTable: "MuseumMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AuditLog__3214EC07D4241A7D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ContentVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ChangeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalExhibits = table.Column<int>(type: "int", nullable: true),
                    TotalMediaFiles = table.Column<int>(type: "int", nullable: true),
                    PackageSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    PublishedBy = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ContentV__3214EC07E2F5D8C8", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentVersions_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContentVersions_Publisher",
                        column: x => x.PublishedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Exhibits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    ExhibitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    QRCodeData = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    QRCodeImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AROverlayUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ARMarkerUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MapId = table.Column<int>(type: "int", nullable: true),
                    LocationX = table.Column<double>(type: "float", nullable: true),
                    LocationY = table.Column<double>(type: "float", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Exhibits__3214EC0728865193", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exhibits_Category",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Exhibits_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Exhibits_Map",
                        column: x => x.MapId,
                        principalTable: "MuseumMaps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Exhibits_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Exhibits_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RefreshT__3214EC07DF597E3F", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_User",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConfigKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ConfigValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SystemCo__3214EC07FFC5BA3B", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysConfig_User",
                        column: x => x.UpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TourRouteTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourRouteId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    RouteName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TourRout__3214EC07DB106995", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourRouteTrans_Route",
                        column: x => x.TourRouteId,
                        principalTable: "TourRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    RawResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PaymentL__3214EC07BB4B7413", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentLogs_Transaction",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitorId = table.Column<int>(type: "int", nullable: false),
                    TicketTypeId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<int>(type: "int", nullable: true),
                    TicketCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    ValidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tickets__3214EC07876BB3A3", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_Transaction",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Type",
                        column: x => x.TicketTypeId,
                        principalTable: "TicketTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tickets_Visitor",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OfflinePackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    VersionId = table.Column<int>(type: "int", nullable: false),
                    PackageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PackageSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Checksum = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    AudioCount = table.Column<int>(type: "int", nullable: true),
                    ImageCount = table.Column<int>(type: "int", nullable: true),
                    ARAssetCount = table.Column<int>(type: "int", nullable: true),
                    ExhibitCount = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Building"),
                    BuiltAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__OfflineP__3214EC074C67D86D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfflinePackages_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OfflinePackages_Version",
                        column: x => x.VersionId,
                        principalTable: "ContentVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnalyticsLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitorId = table.Column<int>(type: "int", nullable: true),
                    ExhibitId = table.Column<int>(type: "int", nullable: true),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ListeningDuration = table.Column<int>(type: "int", nullable: true),
                    LanguageUsed = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    SearchQuery = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsOfflineEvent = table.Column<bool>(type: "bit", nullable: false),
                    EventTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Analytic__3214EC076218A530", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Analytics_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Analytics_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Analytics_Visitor",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bookmarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitorId = table.Column<int>(type: "int", nullable: false),
                    ExhibitId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Bookmark__3214EC0743E83E00", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bookmarks_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bookmarks_Visitor",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContentChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VersionId = table.Column<int>(type: "int", nullable: false),
                    ExhibitId = table.Column<int>(type: "int", nullable: true),
                    ChangeType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedBy = table.Column<int>(type: "int", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ContentC__3214EC07D7F809D8", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChangeLog_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChangeLog_User",
                        column: x => x.ChangedBy,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChangeLog_Version",
                        column: x => x.VersionId,
                        principalTable: "ContentVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExhibitARAssets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExhibitId = table.Column<int>(type: "int", nullable: false),
                    AssetType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "OverlayImage"),
                    AssetUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Width = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ExhibitA__3214EC07649477FB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ARAssets_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExhibitImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExhibitId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ExhibitI__3214EC07CB1C8F3E", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExhibitImages_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExhibitionExhibits",
                columns: table => new
                {
                    ExhibitionId = table.Column<int>(type: "int", nullable: false),
                    ExhibitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExhibitionExhibits", x => new { x.ExhibitionId, x.ExhibitId });
                    table.ForeignKey(
                        name: "FK_ExhibEx_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExhibEx_Exhibition",
                        column: x => x.ExhibitionId,
                        principalTable: "Exhibitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExhibitMetadata",
                columns: table => new
                {
                    ExhibitId = table.Column<int>(type: "int", nullable: false),
                    ThemeId = table.Column<int>(type: "int", nullable: true),
                    AgeGroupId = table.Column<int>(type: "int", nullable: true),
                    Era = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    HistoricalEvent = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExhibitMetadata", x => x.ExhibitId);
                    table.ForeignKey(
                        name: "FK_ExhibMeta_Age",
                        column: x => x.AgeGroupId,
                        principalTable: "AgeGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExhibMeta_Exhib",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExhibMeta_Theme",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ExhibitTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExhibitId = table.Column<int>(type: "int", nullable: false),
                    LanguageCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AudioUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AudioDuration = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__ExhibitT__3214EC074E3F7B08", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExhibitTrans_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TourRouteExhibits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourRouteId = table.Column<int>(type: "int", nullable: false),
                    ExhibitId = table.Column<int>(type: "int", nullable: false),
                    StopOrder = table.Column<int>(type: "int", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TourRout__3214EC07028FAC02", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourRouteExhibits_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TourRouteExhibits_Route",
                        column: x => x.TourRouteId,
                        principalTable: "TourRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VisitedExhibits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitorId = table.Column<int>(type: "int", nullable: false),
                    ExhibitId = table.Column<int>(type: "int", nullable: false),
                    MuseumId = table.Column<int>(type: "int", nullable: false),
                    VisitedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__VisitedE__3214EC072F597FFC", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VisitedExhibits_Exhibit",
                        column: x => x.ExhibitId,
                        principalTable: "Exhibits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitedExhibits_Museum",
                        column: x => x.MuseumId,
                        principalTable: "Museums",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VisitedExhibits_Visitor",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PackageDownloads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    VisitorId = table.Column<int>(type: "int", nullable: true),
                    DeviceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DownloadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__PackageD__3214EC0771127F31", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PkgDownloads_Package",
                        column: x => x.PackageId,
                        principalTable: "OfflinePackages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PkgDownloads_Visitor",
                        column: x => x.VisitorId,
                        principalTable: "Visitors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_ActionType",
                table: "AnalyticsLogs",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_EventTimestamp",
                table: "AnalyticsLogs",
                column: "EventTimestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_Exhibit",
                table: "AnalyticsLogs",
                column: "ExhibitId");

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_Museum",
                table: "AnalyticsLogs",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsLogs_VisitorId",
                table: "AnalyticsLogs",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookmarks_ExhibitId",
                table: "Bookmarks",
                column: "ExhibitId");

            migrationBuilder.CreateIndex(
                name: "UQ_Bookmarks",
                table: "Bookmarks",
                columns: new[] { "VisitorId", "ExhibitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_MuseumId",
                table: "Categories",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "UQ_CatTrans",
                table: "CategoryTranslations",
                columns: new[] { "CategoryId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentChangeLogs_ChangedBy",
                table: "ContentChangeLogs",
                column: "ChangedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContentChangeLogs_ExhibitId",
                table: "ContentChangeLogs",
                column: "ExhibitId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentChangeLogs_VersionId",
                table: "ContentChangeLogs",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentVersions_PublishedBy",
                table: "ContentVersions",
                column: "PublishedBy");

            migrationBuilder.CreateIndex(
                name: "UQ_ContentVersion",
                table: "ContentVersions",
                columns: new[] { "MuseumId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExhibitARAssets_ExhibitId",
                table: "ExhibitARAssets",
                column: "ExhibitId");

            migrationBuilder.CreateIndex(
                name: "IX_ExhibitImages_ExhibitId",
                table: "ExhibitImages",
                column: "ExhibitId");

            migrationBuilder.CreateIndex(
                name: "IX_ExhibitionExhibits_ExhibitId",
                table: "ExhibitionExhibits",
                column: "ExhibitId");

            migrationBuilder.CreateIndex(
                name: "IX_Exhibitions_MuseumId",
                table: "Exhibitions",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "UQ_ExhibTrans",
                table: "ExhibitionTranslations",
                columns: new[] { "ExhibitionId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExhibitMetadata_AgeGroupId",
                table: "ExhibitMetadata",
                column: "AgeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExhibitMetadata_ThemeId",
                table: "ExhibitMetadata",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Exhibits_CategoryId",
                table: "Exhibits",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Exhibits_CreatedBy",
                table: "Exhibits",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Exhibits_MapId",
                table: "Exhibits",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_Exhibits_MuseumId",
                table: "Exhibits",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_Exhibits_UpdatedBy",
                table: "Exhibits",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__Exhibits__5204E7405196F227",
                table: "Exhibits",
                column: "ExhibitCode",
                unique: true,
                filter: "[ExhibitCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UQ_ExhibitTrans",
                table: "ExhibitTranslations",
                columns: new[] { "ExhibitId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Language__8B8C8A3446FA90F1",
                table: "Languages",
                column: "LanguageCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapPOIs_MapId",
                table: "MapPOIs",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_MuseumLanguages_LanguageId",
                table: "MuseumLanguages",
                column: "LanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_MuseumMaps_MuseumId",
                table: "MuseumMaps",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "UQ_MuseumTrans",
                table: "MuseumTranslations",
                columns: new[] { "MuseumId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OfflinePackages_MuseumId",
                table: "OfflinePackages",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_OfflinePackages_VersionId",
                table: "OfflinePackages",
                column: "VersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDownloads_PackageId",
                table: "PackageDownloads",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageDownloads_VisitorId",
                table: "PackageDownloads",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentLogs_TransactionId",
                table: "PaymentLogs",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "UQ__PaymentM__737584F65638694F",
                table: "PaymentMethods",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ__Permissi__0FFDA357D7FD4E68",
                table: "Permissions",
                column: "PermissionName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "UQ__Roles__8A2B616097A109F1",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemConfigurations_UpdatedBy",
                table: "SystemConfigurations",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "UQ__SystemCo__4A3067846AFFD489",
                table: "SystemConfigurations",
                column: "ConfigKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketTypeId",
                table: "Tickets",
                column: "TicketTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TransactionId",
                table: "Tickets",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_VisitorId",
                table: "Tickets",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "UQ__Tickets__598CF7A3F437EC5B",
                table: "Tickets",
                column: "TicketCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypes_ExhibitionId",
                table: "TicketTypes",
                column: "ExhibitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypes_MuseumId",
                table: "TicketTypes",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_TourRouteExhibits_ExhibitId",
                table: "TourRouteExhibits",
                column: "ExhibitId");

            migrationBuilder.CreateIndex(
                name: "UQ_TourRouteExhibits",
                table: "TourRouteExhibits",
                columns: new[] { "TourRouteId", "ExhibitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TourRoutes_AgeGroupId",
                table: "TourRoutes",
                column: "AgeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TourRoutes_MuseumId",
                table: "TourRoutes",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_TourRoutes_ThemeId",
                table: "TourRoutes",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "UQ_TourRouteTrans",
                table: "TourRouteTranslations",
                columns: new[] { "TourRouteId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PaymentMethodId",
                table: "Transactions",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_VisitorId",
                table: "Transactions",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "UQ__Transact__999B5229FB610A1E",
                table: "Transactions",
                column: "OrderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_MuseumId",
                table: "Users",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__Users__A9D10534AC9E29DD",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitedExhibits_Exhibit",
                table: "VisitedExhibits",
                column: "ExhibitId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitedExhibits_MuseumId",
                table: "VisitedExhibits",
                column: "MuseumId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitedExhibits_Visitor",
                table: "VisitedExhibits",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "UQ__Visitors__49E123102804749D",
                table: "Visitors",
                column: "DeviceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsLogs");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Bookmarks");

            migrationBuilder.DropTable(
                name: "CategoryTranslations");

            migrationBuilder.DropTable(
                name: "ContentChangeLogs");

            migrationBuilder.DropTable(
                name: "ExhibitARAssets");

            migrationBuilder.DropTable(
                name: "ExhibitImages");

            migrationBuilder.DropTable(
                name: "ExhibitionExhibits");

            migrationBuilder.DropTable(
                name: "ExhibitionTranslations");

            migrationBuilder.DropTable(
                name: "ExhibitMetadata");

            migrationBuilder.DropTable(
                name: "ExhibitTranslations");

            migrationBuilder.DropTable(
                name: "MapPOIs");

            migrationBuilder.DropTable(
                name: "MuseumLanguages");

            migrationBuilder.DropTable(
                name: "MuseumTranslations");

            migrationBuilder.DropTable(
                name: "PackageDownloads");

            migrationBuilder.DropTable(
                name: "PaymentLogs");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SystemConfigurations");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "TourRouteExhibits");

            migrationBuilder.DropTable(
                name: "TourRouteTranslations");

            migrationBuilder.DropTable(
                name: "VisitedExhibits");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "OfflinePackages");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "TicketTypes");

            migrationBuilder.DropTable(
                name: "TourRoutes");

            migrationBuilder.DropTable(
                name: "Exhibits");

            migrationBuilder.DropTable(
                name: "ContentVersions");

            migrationBuilder.DropTable(
                name: "PaymentMethods");

            migrationBuilder.DropTable(
                name: "Visitors");

            migrationBuilder.DropTable(
                name: "Exhibitions");

            migrationBuilder.DropTable(
                name: "AgeGroups");

            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "MuseumMaps");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Museums");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
