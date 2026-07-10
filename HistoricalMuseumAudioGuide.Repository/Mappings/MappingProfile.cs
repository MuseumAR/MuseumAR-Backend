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
            
            CreateMap<Museum, MuseumDto>().ReverseMap();
            CreateMap<CreateMuseumDto, Museum>();
            CreateMap<UpdateMuseumProfileDto, Museum>();
            
            CreateMap<Exhibit, ExhibitDto>()
                .ForMember(dest => dest.ExhibitMetadata, opt => opt.MapFrom(src => src.ExhibitMetadatum))
                .ReverseMap();
            CreateMap<CreateExhibitDto, Exhibit>();

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
            CreateMap<Theme, ThemeDto>().ReverseMap();
            CreateMap<CreateThemeDto, Theme>();

            // Tag & TagGroup
            CreateMap<TagGroup, TagGroupDto>().ReverseMap();
            CreateMap<CreateTagGroupDto, TagGroup>();
            CreateMap<Tag, TagDto>().ReverseMap();
            CreateMap<CreateTagDto, Tag>();

            // Content Version
            CreateMap<ContentVersion, ContentVersionDto>();

            CreateMap<Exhibition, ExhibitionDto>().ReverseMap();
            CreateMap<CreateExhibitionDto, Exhibition>();

            // AR Asset
            CreateMap<ExhibitArasset, ExhibitArassetDto>();
            CreateMap<CreateExhibitArassetDto, ExhibitArasset>();

            // Offline Package
            CreateMap<OfflinePackage, OfflinePackageDto>();

            // Ticketing
            CreateMap<TicketType, TicketTypeDto>();
            CreateMap<CreateTicketTypeDto, TicketType>();
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.TicketTypeName, opt => opt.MapFrom(src => src.TicketType.Name));

            // Visitor
            CreateMap<Bookmark, BookmarkDto>();
            CreateMap<CreateBookmarkDto, Bookmark>();
            CreateMap<VisitedExhibit, VisitedExhibitDto>();
            CreateMap<CreateVisitedExhibitDto, VisitedExhibit>();

            // Maps & Routes
            CreateMap<MuseumMap, MuseumMapDto>()
                .ForMember(dest => dest.MapType, opt => opt.MapFrom(src => src.MapName ?? "floor"))
                .ReverseMap()
                .ForMember(dest => dest.MapName, opt => opt.MapFrom(src => src.MapType));
            CreateMap<TourRoute, TourRouteDto>().ReverseMap();
            CreateMap<CreateTourRouteDto, TourRoute>();

            // System Config
            CreateMap<SystemConfiguration, SystemConfigDto>();
        }
    }
}
