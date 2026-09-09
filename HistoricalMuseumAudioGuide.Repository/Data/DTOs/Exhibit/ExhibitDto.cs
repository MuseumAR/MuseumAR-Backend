using System;
using System.Collections.Generic;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.ARAsset;

namespace HistoricalMuseumAudioGuide.Repository.Data.DTOs.Exhibit
{
    public class ExhibitDto
    {
        public int Id { get; set; }
        public int MuseumId { get; set; }
        public int? CategoryId { get; set; }
        public string? ExhibitCode { get; set; }
        public string? QRCodeData { get; set; }
        public string? QRCodeImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? AROverlayUrl { get; set; }
        public string? ARMarkerUrl { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime? PublishedAt { get; set; }
        
        public int? MapId { get; set; }
        public string? MapName { get; set; }
        public int? FloorNumber { get; set; }
        public int? RoomId { get; set; }
        public string? RoomCode { get; set; }
        public string? RoomName { get; set; }

        /// <summary>True if at least one AR asset with AssetType "Model3D" exists.</summary>
        public bool HasArModel { get; set; }

        /// <summary>Embedded AR assets — replaces the N+1 GET /ar-assets call.</summary>
        public List<ExhibitArassetDto> ArAssets { get; set; } = new();
        
        public ExhibitMetadataDto? ExhibitMetadata { get; set; }
        
        public ICollection<ExhibitTranslationDto> Translations { get; set; } = new List<ExhibitTranslationDto>();
    }
}
