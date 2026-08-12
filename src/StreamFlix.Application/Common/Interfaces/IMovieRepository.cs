using StreamFlix.Domain.Entities;

namespace StreamFlix.Application.Common.Interfaces;

/// <summary>
/// Contrato para persistir y consultar Movies. Vive en Application (no en Infrastructure)
/// a propósito: Application define QUÉ necesita ("dame una película por id"), pero
/// no le importa CÓMO se resuelve (PostgreSQL, SQL Server, memoria, un archivo JSON...).
///
/// Esto es "inversión de dependencias": Infrastructure depende de esta interfaz
/// (la implementa), en lugar de que Application dependa de Infrastructure.
/// </summary>
public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(Guid id);
    Task<List<Movie>> GetAllAsync();
    Task AddAsync(Movie movie);
    Task DeleteAsync(Movie movie);
    Task SaveChangesAsync();
}
