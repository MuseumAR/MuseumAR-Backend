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

namespace HistoricalMuseumAudioGuide.Repository.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Museum, MuseumDto>().ReverseMap();
            CreateMap<CreateMuseumDto, Museum>();
            
            CreateMap<Exhibit, ExhibitDto>().ReverseMap();
            CreateMap<CreateExhibitDto, Exhibit>();

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
