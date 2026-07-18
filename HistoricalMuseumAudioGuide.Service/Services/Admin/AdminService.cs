using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.Interfaces;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Museum;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.Ticketing;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.User;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System.Collections.Generic;
using System.Threading.Tasks;
using HistoricalMuseumAudioGuide.Service.Services;
using HistoricalMuseumAudioGuide.Service.Services.Media;
using Microsoft.AspNetCore.Http;

namespace HistoricalMuseumAudioGuide.Service.Services.Admin
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMuseumResolver _museumResolver;
        private readonly IMediaService _mediaService;

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper, IMuseumResolver museumResolver, IMediaService mediaService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _museumResolver = museumResolver;
            _mediaService = mediaService;
        }

        public async Task<ResponseModel> GetMuseumProfileAsync()
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var museum = await _unitOfWork.Museums.GetByIdAsync(museumId);
            if (museum == null) return ResponseModel.NotFound("Museum not found");
            
            var dto = _mapper.Map<MuseumDto>(museum);
            return ResponseModel.Success("Museum profile retrieved successfully", dto);
        }

        public async Task<ResponseModel> UpdateMuseumProfileAsync(UpdateMuseumProfileDto museumDto)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var museum = await _unitOfWork.Museums.GetByIdAsync(museumId);
            if (museum == null) return ResponseModel.NotFound("Museum not found");

            _mapper.Map(museumDto, museum);
            museum.UpdatedAt = System.DateTime.UtcNow;

            _unitOfWork.Museums.Update(museum);
            await _unitOfWork.CompleteAsync();
            return ResponseModel.Success("Museum profile updated successfully");
        }

        public async Task<ResponseModel> UploadMuseumImageAsync(IFormFile file)
        {
            var museumId = await _museumResolver.GetMuseumIdAsync();
            var museum = await _unitOfWork.Museums.GetByIdAsync(museumId);
            if (museum == null) return ResponseModel.NotFound("Museum not found");

            var fileUrl = await _mediaService.UploadFileAsync(file, "museums");

            museum.ThumbnailUrl = fileUrl;
            museum.UpdatedAt = System.DateTime.UtcNow;
            _unitOfWork.Museums.Update(museum);

            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("Museum image uploaded successfully", fileUrl);
        }

        public async Task<ResponseModel> GetAllTicketTypesAsync()
        {
            var ticketTypes = await _unitOfWork.TicketTypes.GetAllAsync();
            var dtos = _mapper.Map<IEnumerable<TicketTypeDto>>(ticketTypes);
            return ResponseModel.Success("Ticket types retrieved successfully", dtos);
        }

        public async Task<ResponseModel> GetAllUsersAsync(string? roleName, string? status, string? search)
        {
            var users = await _unitOfWork.Users.GetAllUsersWithRoleAsync();
            var query = users.AsQueryable();

            if (!string.IsNullOrEmpty(roleName))
            {
                query = query.Where(u => u.Role != null && u.Role.RoleName.Contains(roleName, System.StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(u => u.Status.Contains(status, System.StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search, System.StringComparison.OrdinalIgnoreCase) || u.Email.Contains(search, System.StringComparison.OrdinalIgnoreCase));
            }

            var dtos = _mapper.Map<IEnumerable<UserResponseDto>>(query.ToList());
            return ResponseModel.Success("Users retrieved successfully", dtos);
        }

        public async Task<ResponseModel> GetUserByIdAsync(int id)
        {
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Id == id, includeProperties: "Role");
            if (user == null) return ResponseModel.NotFound("User not found");

            var dto = _mapper.Map<UserResponseDto>(user);
            return ResponseModel.Success("User retrieved successfully", dto);
        }

        public async Task<ResponseModel> CreateUserAsync(CreateUserDto dto)
        {
            var existingUser = await _unitOfWork.Users.GetUserByEmailAsync(dto.Email);
            if (existingUser != null) return ResponseModel.BadRequest("Email already exists");

            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            user.Status = "Active";
            user.CreatedAt = System.DateTime.UtcNow;
            user.UpdatedAt = System.DateTime.UtcNow;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            var createdUser = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Id == user.Id, includeProperties: "Role");
            var responseDto = _mapper.Map<UserResponseDto>(createdUser);
            return ResponseModel.Success("User created successfully", responseDto);
        }

        public async Task<ResponseModel> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            var user = await _unitOfWork.Users.GetFirstOrDefaultAsync(u => u.Id == id, includeProperties: "Role");
            if (user == null) return ResponseModel.NotFound("User not found");

            _mapper.Map(dto, user);
            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }
            user.UpdatedAt = System.DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("User updated successfully");
        }

        public async Task<ResponseModel> DeleteUserAsync(int id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null) return ResponseModel.NotFound("User not found");

            user.Status = "Inactive";
            user.UpdatedAt = System.DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return ResponseModel.Success("User deleted (inactivated) successfully");
        }

        public async Task<ResponseModel> GetAuditLogsAsync(int? userId, string? action, System.DateTime? fromDate, System.DateTime? toDate, int page, int pageSize)
        {
            var logs = await _unitOfWork.AuditLogs.GetAllAsync();
            var query = logs.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(l => l.UserId == userId.Value);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(l => l.Action.Contains(action, System.StringComparison.OrdinalIgnoreCase));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.CreatedAt <= toDate.Value);
            }

            var totalItems = query.Count();
            var paginatedLogs = query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)System.Math.Ceiling((double)totalItems / pageSize),
                Items = paginatedLogs
            };

            return ResponseModel.Success("Audit logs retrieved successfully", result);
        }
    }
}
