using JobFinderNet.Core.DTOs;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Core.Interfaces.Services;

namespace JobFinderNet.Infrastructure.Services;

public class CompanyProfileService : ICompanyProfileService
{
    private readonly ICompanyProfileRepository _companyProfileRepository;

    public CompanyProfileService(ICompanyProfileRepository companyProfileRepository)
    {
        _companyProfileRepository = companyProfileRepository;
    }

    public async Task<CompanyProfileDto?> GetByIdAsync(int id)
    {
        var profile = await _companyProfileRepository.GetByIdAsync(id);
        if (profile == null) return null;

        return new CompanyProfileDto
        {
            Id = profile.Id,
            Name = profile.Name,
            LogoUrl = profile.LogoUrl,
            Description = profile.Description,
            Website = profile.Website,
            Size = profile.Size,
            Industry = profile.Industry,
            IsVerified = profile.IsVerified,
            OpenRoles = profile.Jobs.Count
        };
    }

    public async Task<List<CompanySearchResultDto>> SearchAsync(string? query)
    {
        var companies = await _companyProfileRepository.SearchAsync(query);
        return companies.Select(c => new CompanySearchResultDto
        {
            Id = c.Id,
            Name = c.Name,
            LogoUrl = c.LogoUrl,
            Industry = c.Industry,
            OpenRoles = c.Jobs.Count
        }).ToList();
    }

    public async Task<CompanyProfile?> GetMyCompanyAsync(string userId)
    {
        return await _companyProfileRepository.GetByClaimedUserIdAsync(userId);
    }

    public async Task<CompanyProfile> ClaimCompanyAsync(string userId, CreateCompanyProfileDto dto)
    {
        var existing = await _companyProfileRepository.GetByNameAsync(dto.Name);

        if (existing != null)
        {
            if (existing.ClaimedByUserId != null)
                throw new InvalidOperationException("Company already claimed");

            existing.ClaimedByUserId = userId;
            existing.LogoUrl = dto.LogoUrl ?? existing.LogoUrl;
            existing.Description = dto.Description ?? existing.Description;
            existing.Website = dto.Website ?? existing.Website;
            existing.Size = dto.Size ?? existing.Size;
            existing.Industry = dto.Industry ?? existing.Industry;
            existing.FoundedYear = dto.FoundedYear ?? existing.FoundedYear;
            existing.Culture = dto.Culture ?? existing.Culture;
            existing.UpdatedAt = DateTime.UtcNow;

            await _companyProfileRepository.SaveChangesAsync();
            return existing;
        }

        var company = new CompanyProfile
        {
            Name = dto.Name,
            LogoUrl = dto.LogoUrl,
            Description = dto.Description,
            Website = dto.Website,
            Size = dto.Size,
            Industry = dto.Industry,
            FoundedYear = dto.FoundedYear,
            Culture = dto.Culture,
            ClaimedByUserId = userId,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _companyProfileRepository.AddAsync(company);
        await _companyProfileRepository.SaveChangesAsync();

        return company;
    }

    public async Task<CompanyProfile?> UpdateCompanyAsync(int id, string userId, CreateCompanyProfileDto dto)
    {
        var company = await _companyProfileRepository.GetByIdAsync(id);
        if (company == null) return null;
        if (company.ClaimedByUserId != userId)
            throw new UnauthorizedAccessException();

        company.LogoUrl = dto.LogoUrl ?? company.LogoUrl;
        company.Description = dto.Description ?? company.Description;
        company.Website = dto.Website ?? company.Website;
        company.Size = dto.Size ?? company.Size;
        company.Industry = dto.Industry ?? company.Industry;
        company.FoundedYear = dto.FoundedYear ?? company.FoundedYear;
        company.Culture = dto.Culture ?? company.Culture;
        company.UpdatedAt = DateTime.UtcNow;

        await _companyProfileRepository.SaveChangesAsync();
        return company;
    }
}
