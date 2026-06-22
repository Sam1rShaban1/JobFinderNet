using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Services;

public interface ICompanyProfileService
{
    Task<CompanyProfileDto?> GetByIdAsync(int id);
    Task<List<CompanySearchResultDto>> SearchAsync(string? query);
    Task<CompanyProfile?> GetMyCompanyAsync(string userId);
    Task<CompanyProfile> ClaimCompanyAsync(string userId, CreateCompanyProfileDto dto);
    Task<CompanyProfile?> UpdateCompanyAsync(int id, string userId, CreateCompanyProfileDto dto);
}
