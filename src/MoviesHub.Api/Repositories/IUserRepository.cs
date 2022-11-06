using Microsoft.EntityFrameworkCore;
using MoviesHub.Api.Storage;
using MoviesHub.Api.Storage.Entities;

namespace MoviesHub.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetUserByMobileNumberAsync(string mobileNumber) =>
        await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MobileNumber.Equals(mobileNumber));
    
    public async Task<User?> GetUserByIdAsync(string id) =>
        await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => 
                x.Id.ToLower().Equals(id.ToLower()));
    
    public async Task<bool> CreateAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
        int count = await _dbContext.SaveChangesAsync();

        return count > 0;
    }
}

public interface IUserRepository
{
    Task<User?> GetUserByMobileNumberAsync(string mobileNumber);
    Task<User?> GetUserByIdAsync(string id);
    Task<bool> CreateAsync(User user);
}