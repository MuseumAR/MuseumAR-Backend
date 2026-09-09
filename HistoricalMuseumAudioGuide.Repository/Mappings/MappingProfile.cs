using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Visitor;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.MuseumMap;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.TourRoute;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.SystemConfig;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Category;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ContentVersion;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Auth;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Analytics;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.AgeGroup;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Theme;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Tag;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.User;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Room;

namespace HistoricalMuseumAudioGuide.Repository.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User & Auth
            CreateMap<User, LoginResponseDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName));
            CreateMap<RegisterRequestDto, User>();
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty));

            // Analytics
            CreateMap<CreateAnalyticsLogDto, AnalyticsLog>();

            // System Config
            CreateMap<UpdateSystemConfigDto, SystemConfiguration>();

            // Ticketing
            CreateMap<CreateOrderRequestDto, Transaction>();
            
            CreateMap<MuseumTranslation, MuseumTranslationDto>().ReverseMap();
            CreateMap<Museum, MuseumDto>()
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.MuseumTranslations))
                .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src =>
                    src.MuseumTranslations.FirstOrDefault(t => t.LanguageCode == "en") != null
                        ? src.MuseumTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Name
                        : null))
                .ForMember(dest => dest.DescriptionEn, opt => opt.MapFrom(src =>
                    src.MuseumTranslations.FirstOrDefault(t => t.LanguageCode == "en") != null
                        ? src.MuseumTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Description
                        : null))
                .ForMember(dest => dest.AddressEn, opt => opt.MapFrom(src =>
                    src.MuseumTranslations.FirstOrDefault(t => t.LanguageCode == "en") != null
                        ? src.MuseumTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Address
                        : null))
                .ForMember(dest => dest.OpeningHoursEn, opt => opt.MapFrom(src =>
                    src.MuseumTranslations.FirstOrDefault(t => t.LanguageCode == "en") != null
                        ? src.MuseumTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.OpeningHours
                        : null));
            CreateMap<CreateMuseumDto, Museum>();
            CreateMap<UpdateMuseumProfileDto, Museum>()
                .ForMember(dest => dest.MuseumTranslations, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            
            CreateMap<Exhibit, ExhibitDto>()
                .ForMember(dest => dest.ExhibitMetadata, opt => opt.MapFrom(src => src.ExhibitMetadatum))
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.ExhibitTranslations))
                .ForMember(dest => dest.MapName, opt => opt.MapFrom(src => src.Map != null ? src.Map.MapName : null))
                .ForMember(dest => dest.FloorNumber, opt => opt.MapFrom(src => src.Room != null ? (int?)src.Room.FloorNumber : (src.Map != null ? (int?)src.Map.FloorNumber : null)))
                .ForMember(dest => dest.RoomCode, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomCode : null))
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room != null ? src.Room.RoomName : null))
                .ReverseMap();
            CreateMap<CreateExhibitDto, Exhibit>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Room Mappings
            CreateMap<Room, RoomDto>()
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.RoomTranslations));
            CreateMap<CreateRoomDto, Room>()
                .ForMember(dest => dest.RoomTranslations, opt => opt.Ignore());
            CreateMap<UpdateRoomDto, Room>()
                .ForMember(dest => dest.RoomTranslations, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<RoomTranslation, RoomTranslationDto>().ReverseMap();

            // Exhibit Translation
            CreateMap<ExhibitTranslation, ExhibitTranslationDto>();
            CreateMap<ExhibitTranslationDto, ExhibitTranslation>();

            // Exhibit Metadata
            CreateMap<ExhibitMetadatum, ExhibitMetadataDto>().ReverseMap();

            // Category
            CreateMap<Category, CategoryDto>().ReverseMap();
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<CategoryTranslation, CategoryTranslationDto>().ReverseMap();

            // AgeGroup & Theme
            CreateMap<AgeGroup, AgeGroupDto>();
            CreateMap<Theme, ThemeDto>()
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.ThemeTranslations));
            CreateMap<CreateThemeDto, Theme>()
                .ForMember(dest => dest.ThemeTranslations, opt => opt.Ignore());
            CreateMap<ThemeTranslation, ThemeTranslationDto>().ReverseMap();

            // Tag & TagGroup
            CreateMap<TagGroup, TagGroupDto>()
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.TagGroupTranslations));
            CreateMap<CreateTagGroupDto, TagGroup>()
                .ForMember(dest => dest.TagGroupTranslations, opt => opt.Ignore());
            CreateMap<TagGroupTranslation, TagGroupTranslationDto>().ReverseMap();
            CreateMap<Tag, TagDto>()
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.TagTranslations));
            CreateMap<CreateTagDto, Tag>()
                .ForMember(dest => dest.TagTranslations, opt => opt.Ignore());
            CreateMap<TagTranslation, TagTranslationDto>().ReverseMap();

            // Content Version
            CreateMap<ContentVersion, ContentVersionDto>();

            CreateMap<ExhibitionTranslation, ExhibitionTranslationDto>().ReverseMap();
#pragma warning disable CS8602
            CreateMap<Exhibition, ExhibitionDto>()
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.ExhibitionTranslations))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => 
                    src.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "vi")!.Name ?? 
                    src.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Name ?? 
                    (src.ExhibitionTranslations.Any() ? src.ExhibitionTranslations.First().Name : null)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => 
                    src.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "vi")!.Description ?? 
                    src.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Description ?? 
                    (src.ExhibitionTranslations.Any() ? src.ExhibitionTranslations.First().Description : null)))
                .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src =>
                    src.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "en") != null
                        ? src.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Name
                        : null))
                .ForMember(dest => dest.DescriptionEn, opt => opt.MapFrom(src =>
                    src.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "en") != null
                        ? src.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Description
                        : null))
                .ForMember(dest => dest.ThemeName, opt => opt.MapFrom(src =>
                    src.Theme != null ? src.Theme.ThemeName : null))
                .ReverseMap()
                .ForMember(dest => dest.ExhibitionTranslations, opt => opt.Ignore());
            CreateMap<CreateExhibitionDto, Exhibition>();

            // AR Asset
            CreateMap<ExhibitArasset, ExhibitArassetDto>();
            CreateMap<CreateExhibitArassetDto, ExhibitArasset>();

            // Offline Package
            CreateMap<OfflinePackage, OfflinePackageDto>();

            // Ticketing
            CreateMap<TicketType, TicketTypeDto>();
            CreateMap<CreateTicketTypeDto, TicketType>();
            CreateMap<UpdateTicketTypeDto, TicketType>();
            CreateMap<TicketPromotion, TicketPromotionDto>();
            CreateMap<CreateTicketPromotionDto, TicketPromotion>();
            CreateMap<UpdateTicketPromotionDto, TicketPromotion>();
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.TicketTypeName, opt => opt.MapFrom(src => src.TicketType != null ? src.TicketType.Name : null))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price > 0 ? src.Price : (src.TicketType != null ? src.TicketType.Price : 0)));

            // Visitor
            CreateMap<Bookmark, BookmarkDto>();
            CreateMap<CreateBookmarkDto, Bookmark>();
            CreateMap<VisitedExhibit, VisitedExhibitDto>();
            CreateMap<CreateVisitedExhibitDto, VisitedExhibit>();
            CreateMap<VisitorSyncDto, Entities.Visitor>();

            // Maps & Routes
            CreateMap<MuseumMap, MuseumMapDto>()
                .ForMember(dest => dest.MapType, opt => opt.MapFrom(src => src.MapType ?? src.MapName ?? "floor"))
                .ReverseMap();

            CreateMap<MapPoi, MapPoiDto>()
                .ForMember(dest => dest.PoiType, opt => opt.MapFrom(src => src.Poitype))
                .ReverseMap()
                .ForMember(dest => dest.Poitype, opt => opt.MapFrom(src => src.PoiType));
            CreateMap<CreateMapPoiDto, MapPoi>()
                .ForMember(dest => dest.Poitype, opt => opt.MapFrom(src => src.PoiType));

            CreateMap<TourRoute, TourRouteDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    src.TourRouteTranslations.FirstOrDefault(t => t.LanguageCode == "vi")!.RouteName ??
                    src.TourRouteTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.RouteName ??
                    (src.TourRouteTranslations.Any() ? src.TourRouteTranslations.First().RouteName : null)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src =>
                    src.TourRouteTranslations.FirstOrDefault(t => t.LanguageCode == "vi")!.Description ??
                    src.TourRouteTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Description ??
                    (src.TourRouteTranslations.Any() ? src.TourRouteTranslations.First().Description : null)))
                .ForMember(dest => dest.EstimatedDurationMinutes, opt => opt.MapFrom(src => src.EstimatedMinutes))
                .ForMember(dest => dest.AgeGroupName, opt => opt.MapFrom(src => src.AgeGroup != null ? src.AgeGroup.GroupName : null))
                .ForMember(dest => dest.ExhibitionName, opt => opt.MapFrom(src => src.Exhibition != null
                    ? (src.Exhibition.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "vi")!.Name ??
                       src.Exhibition.ExhibitionTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Name ??
                       (src.Exhibition.ExhibitionTranslations.Any() ? src.Exhibition.ExhibitionTranslations.First().Name : null))
                    : null))
                .ForMember(dest => dest.Stops, opt => opt.MapFrom(src =>
                    src.TourRouteExhibits.OrderBy(s => s.StopOrder)))
                .ForMember(dest => dest.Translations, opt => opt.MapFrom(src => src.TourRouteTranslations));

            CreateMap<CreateTourRouteDto, TourRoute>()
                .ForMember(dest => dest.EstimatedMinutes, opt => opt.MapFrom(src => src.EstimatedDurationMinutes))
                .ForMember(dest => dest.TourRouteExhibits, opt => opt.Ignore())
                .ForMember(dest => dest.TourRouteTranslations, opt => opt.Ignore());

            CreateMap<TourRouteExhibit, TourRouteStopDto>()
                .ForMember(dest => dest.ExhibitName, opt => opt.MapFrom(src => src.Exhibit != null
                    ? (src.Exhibit.ExhibitTranslations.FirstOrDefault(t => t.LanguageCode == "vi")!.Title ??
                       src.Exhibit.ExhibitTranslations.FirstOrDefault(t => t.LanguageCode == "en")!.Title ??
                       (src.Exhibit.ExhibitTranslations.Any() ? src.Exhibit.ExhibitTranslations.First().Title : null))
                    : null))
                .ForMember(dest => dest.ExhibitCode, opt => opt.MapFrom(src => src.Exhibit != null ? src.Exhibit.ExhibitCode : null))
                .ForMember(dest => dest.MapId, opt => opt.MapFrom(src => src.Exhibit != null ? src.Exhibit.MapId : null))
                .ForMember(dest => dest.FloorNumber, opt => opt.MapFrom(src => src.Exhibit != null ? (src.Exhibit.Room != null ? (int?)src.Exhibit.Room.FloorNumber : (src.Exhibit.Map != null ? (int?)src.Exhibit.Map.FloorNumber : null)) : null))
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.Exhibit != null ? src.Exhibit.RoomId : null))
                .ForMember(dest => dest.RoomCode, opt => opt.MapFrom(src => src.Exhibit != null && src.Exhibit.Room != null ? src.Exhibit.Room.RoomCode : null))
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Exhibit != null && src.Exhibit.Room != null ? src.Exhibit.Room.RoomName : null));
#pragma warning restore CS8602
            CreateMap<CreateTourRouteStopDto, TourRouteExhibit>();

            CreateMap<TourRouteTranslation, TourRouteTranslationDto>().ReverseMap();

            // System Config
            CreateMap<SystemConfiguration, SystemConfigDto>();
        }
    }
}
