using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.OfflinePackage;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Visitor;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.Visitor;

public class VisitorService : IVisitorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VisitorService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseModel> GetLatestOfflinePackageAsync(int museumId)
    {
        var packages = await _unitOfWork.OfflinePackages.GetPackagesByMuseumIdAsync(museumId);
        var latest = packages
            .Where(p => p.Status == "Available")
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault();

        if (latest == null)
        {
            return ResponseModel.NotFound("No offline package available for this museum.");
        }

        var dto = _mapper.Map<OfflinePackageDto>(latest);
        return ResponseModel.Success("Latest offline package retrieved", dto);
    }

    public async Task<ResponseModel> GetVisitorProfileAsync(int visitorId)
    {
        var visitor = await _unitOfWork.Visitors.GetByIdAsync(visitorId);
        if (visitor == null) return ResponseModel.NotFound("Visitor profile not found");
        
        return ResponseModel.Success("Visitor profile retrieved", visitor);
    }

    public async Task<ResponseModel> GetBookmarksAsync(int visitorId)
    {
        var bookmarks = await _unitOfWork.Bookmarks.FindAsync(b => b.VisitorId == visitorId);
        var dtos = _mapper.Map<IEnumerable<BookmarkDto>>(bookmarks);
        return ResponseModel.Success("Get bookmarks successfully", dtos);
    }

    public async Task<ResponseModel> AddBookmarkAsync(int visitorId, CreateBookmarkDto dto)
    {
        var existing = await _unitOfWork.Bookmarks.FindAsync(b => b.VisitorId == visitorId && b.ExhibitId == dto.ExhibitId);
        if (existing.Any())
        {
            return ResponseModel.Conflict("Exhibit already bookmarked.");
        }

        var bookmark = _mapper.Map<Bookmark>(dto);
        bookmark.VisitorId = visitorId;
        bookmark.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Bookmarks.AddAsync(bookmark);
        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success("Bookmark added successfully", bookmark.Id);
    }

    public async Task<ResponseModel> RemoveBookmarkAsync(int visitorId, int exhibitId)
    {
        var bookmarks = await _unitOfWork.Bookmarks.FindAsync(b => b.VisitorId == visitorId && b.ExhibitId == exhibitId);
        var bookmark = bookmarks.FirstOrDefault();
        if (bookmark == null) return ResponseModel.NotFound("Bookmark not found");

        _unitOfWork.Bookmarks.Delete(bookmark);
        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success("Bookmark removed successfully");
    }

    public async Task<ResponseModel> GetVisitedExhibitsAsync(int visitorId)
    {
        var visited = await _unitOfWork.VisitedExhibits.FindAsync(v => v.VisitorId == visitorId);
        var dtos = _mapper.Map<IEnumerable<VisitedExhibitDto>>(visited);
        return ResponseModel.Success("Get visited exhibits successfully", dtos);
    }

    public async Task<ResponseModel> TrackVisitedExhibitAsync(int visitorId, CreateVisitedExhibitDto dto)
    {
        var exhibit = await _unitOfWork.Exhibits.GetByIdAsync(dto.ExhibitId);
        if (exhibit == null) return ResponseModel.NotFound("Exhibit not found");

        var visited = _mapper.Map<VisitedExhibit>(dto);
        visited.VisitorId = visitorId;
        visited.MuseumId = exhibit.MuseumId;
        visited.VisitedAt = DateTime.UtcNow;

        await _unitOfWork.VisitedExhibits.AddAsync(visited);
        await _unitOfWork.CompleteAsync();

        return ResponseModel.Success("Exhibit tracked successfully");
    }

    public async Task<ResponseModel> GetVisitorByUserIdAsync(int userId)
    {
        var visitor = await _unitOfWork.Visitors.GetVisitorByUserIdAsync(userId);
        if (visitor == null) return ResponseModel.NotFound("Visitor profile not found for this user.");
        return ResponseModel.Success("Visitor found", visitor);
    }

    public async Task<ResponseModel> SyncVisitorAsync(VisitorSyncDto dto, int? userId)
    {
        var visitor = await _unitOfWork.Visitors.GetVisitorByDeviceIdAsync(dto.DeviceId);

        if (visitor == null)
        {
            visitor = _mapper.Map<HistoricalMuseumAudioGuide.Repository.Entities.Visitor>(dto);
            visitor.UserId = userId;
            visitor.FirstSeenAt = DateTime.UtcNow;
            visitor.LastSeenAt = DateTime.UtcNow;

            await _unitOfWork.Visitors.AddAsync(visitor);
        }
        else
        {
            visitor.DisplayName = dto.DisplayName ?? visitor.DisplayName;
            visitor.Email = dto.Email ?? visitor.Email;
            visitor.PreferredLang = dto.PreferredLang;
            visitor.DeviceType = dto.DeviceType ?? visitor.DeviceType;
            visitor.DeviceModel = dto.DeviceModel ?? visitor.DeviceModel;
            visitor.AppVersion = dto.AppVersion ?? visitor.AppVersion;
            visitor.LastSeenAt = DateTime.UtcNow;

            if (userId.HasValue)
            {
                visitor.UserId = userId;
            }

            _unitOfWork.Visitors.Update(visitor);
        }

        await _unitOfWork.CompleteAsync();
        return ResponseModel.Success("Visitor synchronized successfully.", visitor);
    }
}
