using AutoMapper;
using HistoricalMuseumAudioGuide.Repository.Data.DTOs.SystemConfig;
using HistoricalMuseumAudioGuide.Repository.Entities;
using HistoricalMuseumAudioGuide.Repository.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HistoricalMuseumAudioGuide.Service.Services.SystemConfig;

public class SystemConfigService : ISystemConfigService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SystemConfigService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseModel> GetAllConfigsAsync()
    {
        var configs = await _unitOfWork.SystemConfigurations.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<SystemConfigDto>>(configs);
        return ResponseModel.Success("Get all configurations successfully", dtos);
    }

    public async Task<ResponseModel> GetConfigByKeyAsync(string key)
    {
        var config = await _unitOfWork.SystemConfigurations.GetFirstOrDefaultAsync(c => c.ConfigKey == key);
        if (config == null) return ResponseModel.NotFound("Configuration key not found");
        
        var dto = _mapper.Map<SystemConfigDto>(config);
        return ResponseModel.Success("Get configuration successfully", dto);
    }

    public async Task<ResponseModel> UpdateConfigAsync(string key, UpdateSystemConfigDto dto, int? updatedBy)
    {
        var config = await _unitOfWork.SystemConfigurations.GetFirstOrDefaultAsync(c => c.ConfigKey == key);
        if (config == null)
        {
            // Create if not exists
            config = _mapper.Map<SystemConfiguration>(dto);
            config.ConfigKey = key;
            config.UpdatedBy = updatedBy;
            config.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SystemConfigurations.AddAsync(config);
        }
        else
        {
            _mapper.Map(dto, config);
            config.UpdatedBy = updatedBy;
            config.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.SystemConfigurations.Update(config);
        }

        await _unitOfWork.CompleteAsync();
        return ResponseModel.Success("Configuration updated successfully");
    }
}
