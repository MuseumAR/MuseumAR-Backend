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

            // Analytics
            CreateMap<CreateAnalyticsLogDto, AnalyticsLog>();

            // System Config
            CreateMap<UpdateSystemConfigDto, SystemConfiguration>();

            // Ticketing
            CreateMap<CreateOrderRequestDto, Transaction>();
            
            CreateMap<Museum, MuseumDto>().ReverseMap();
            CreateMap<CreateMuseumDto, Museum>();
            
            CreateMap<Exhibit, ExhibitDto>().ReverseMap();
            CreateMap<CreateExhibitDto, Exhibit>();

            // Exhibit Translation
            CreateMap<ExhibitTranslation, ExhibitTranslationDto>();
            CreateMap<ExhibitTranslationDto, ExhibitTranslation>();

            // Category
            CreateMap<Category, CategoryDto>();

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
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.TicketTypeName, opt => opt.MapFrom(src => src.TicketType.Name));

            // Visitor
            CreateMap<Bookmark, BookmarkDto>();
            CreateMap<CreateBookmarkDto, Bookmark>();
            CreateMap<VisitedExhibit, VisitedExhibitDto>();
            CreateMap<CreateVisitedExhibitDto, VisitedExhibit>();

            // Maps & Routes
            CreateMap<MuseumMap, MuseumMapDto>().ReverseMap();
            CreateMap<CreateMuseumMapDto, MuseumMap>();
            CreateMap<TourRoute, TourRouteDto>().ReverseMap();
            CreateMap<CreateTourRouteDto, TourRoute>();

            // System Config
            CreateMap<SystemConfiguration, SystemConfigDto>();
        }
    }
}
