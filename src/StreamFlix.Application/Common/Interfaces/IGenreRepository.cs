using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.Common.Interfaces;

public interface IGenreRepository
{
    Task<Genre?> GetByIdAsync(Guid id);
    Task<List<Genre>> GetAllAsync();
    Task<List<Genre>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task AddAsync(Genre genre);
    Task SaveChangesAsync();
}
