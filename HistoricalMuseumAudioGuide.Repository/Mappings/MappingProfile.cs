using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibition;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;

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
        }
    }
}
