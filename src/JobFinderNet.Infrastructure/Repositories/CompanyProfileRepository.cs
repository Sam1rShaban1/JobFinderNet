using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Repositories;

public class CompanyProfileRepository : ICompanyProfileRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyProfileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyProfile?> GetByIdAsync(int id)
    {
        return await _context.CompanyProfiles
            .Include(c => c.Jobs.Where(j => j.IsActive))
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CompanyProfile?> GetByNameAsync(string name)
    {
        return await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<CompanyProfile?> GetByClaimedUserIdAsync(string userId)
    {
        return await _context.CompanyProfiles
            .FirstOrDefaultAsync(c => c.ClaimedByUserId == userId);
    }

    public async Task<List<CompanyProfile>> SearchAsync(string? query, int limit = 20)
    {
        var dbQuery = _context.CompanyProfiles.AsQueryable();

        if (!string.IsNullOrEmpty(query))
        {
            dbQuery = dbQuery.Where(c => c.Name.Contains(query));
        }

        return await dbQuery
            .OrderBy(c => c.Name)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AddAsync(CompanyProfile company)
    {
        _context.CompanyProfiles.Add(company);
    }

    public void Update(CompanyProfile company)
    {
        _context.CompanyProfiles.Update(company);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
