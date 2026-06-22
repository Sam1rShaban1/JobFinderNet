using Microsoft.EntityFrameworkCore;
using JobFinderNet.Core.Models;
using JobFinderNet.Core.Interfaces.Repositories;
using JobFinderNet.Infrastructure.Data;

namespace JobFinderNet.Infrastructure.Repositories;

public class ApplicationNoteRepository : IApplicationNoteRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationNoteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApplicationNote>> GetByApplicationIdAsync(int applicationId)
    {
        return await _context.ApplicationNotes
            .Where(n => n.ApplicationId == applicationId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(ApplicationNote note)
    {
        _context.ApplicationNotes.Add(note);
        await _context.SaveChangesAsync();
    }
}
