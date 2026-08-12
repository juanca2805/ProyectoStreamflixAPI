using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.Common.Interfaces;

public interface IFavoriteRepository
{
    Task<List<Favorite>> GetByUserIdAsync(Guid userId);
    Task<Favorite?> GetByUserAndMovieAsync(Guid userId, Guid movieId);
    Task AddAsync(Favorite favorite);
    Task DeleteAsync(Favorite favorite);
    Task SaveChangesAsync();
}
