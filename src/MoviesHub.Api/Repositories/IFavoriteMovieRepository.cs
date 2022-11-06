using Microsoft.EntityFrameworkCore;
using MoviesHub.Api.Storage;
using MoviesHub.Api.Storage.Entities;

namespace MoviesHub.Api.Repositories;

public interface IFavoriteMovieRepository
{
    Task<bool> AddAsync(FavoriteMovie movie);
    Task<bool> DeleteAsync(FavoriteMovie movie);
    Task<List<FavoriteMovie>> GetFavoriteMovies(string mobileNumber);
    Task<FavoriteMovie?> GetFavoriteMovieById(string id);
    Task<FavoriteMovie?> GetUserFavoriteMovieById(string mobileNumber, string movieId);
    Task<bool> IsUserFavoriteMovie(string mobileNumber, int movieId);
}

public class FavoriteMovieRepository : IFavoriteMovieRepository
{
    private readonly ApplicationDbContext _dbContext;

    public FavoriteMovieRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<bool> AddAsync(FavoriteMovie movie)
    {
        await _dbContext.FavoriteMovies.AddAsync(movie);
        int count = await _dbContext.SaveChangesAsync();

        return count > 0;
    }

    public async Task<bool> DeleteAsync(FavoriteMovie movie)
    {
        _dbContext.FavoriteMovies.Remove(movie);
        int count = await _dbContext.SaveChangesAsync();

        return count > 0;
    }

    public async Task<List<FavoriteMovie>> GetFavoriteMovies(string mobileNumber) =>
        await _dbContext.FavoriteMovies
            .AsNoTracking()
            .Where(x => x.UserMobileNumber.Equals(mobileNumber))
            .ToListAsync();

    public async Task<FavoriteMovie?> GetFavoriteMovieById(string id) =>
        await _dbContext.FavoriteMovies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id.Equals(id));

    public async Task<FavoriteMovie?> GetUserFavoriteMovieById(string mobileNumber, string movieId) => 
        await _dbContext.FavoriteMovies
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MovieId.ToString().Equals(movieId) &&
                                      x.UserMobileNumber.Equals(mobileNumber));

    public async Task<bool> IsUserFavoriteMovie(string mobileNumber, int movieId) =>
        await _dbContext.FavoriteMovies
            .AsNoTracking()
            .AnyAsync(x => x.MovieId.Equals(movieId) &&
                           x.UserMobileNumber.Equals(mobileNumber));
}