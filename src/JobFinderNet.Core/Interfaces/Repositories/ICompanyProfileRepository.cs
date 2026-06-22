using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Repositories;

public interface ICompanyProfileRepository
{
    Task<CompanyProfile?> GetByIdAsync(int id);
    Task<CompanyProfile?> GetByNameAsync(string name);
    Task<CompanyProfile?> GetByClaimedUserIdAsync(string userId);
    Task<List<CompanyProfile>> SearchAsync(string? query, int limit = 20);
    Task AddAsync(CompanyProfile company);
    void Update(CompanyProfile company);
    Task SaveChangesAsync();
}
