using JobFinderNet.Core.Models;

namespace JobFinderNet.Core.Interfaces.Repositories;

public interface IApplicationNoteRepository
{
    Task<List<ApplicationNote>> GetByApplicationIdAsync(int applicationId);
    Task AddAsync(ApplicationNote note);
}
