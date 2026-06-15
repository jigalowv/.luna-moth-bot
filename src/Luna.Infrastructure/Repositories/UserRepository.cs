using Microsoft.EntityFrameworkCore;
using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Domain.Enums;
using Luna.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Luna.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<IUserRepository> _logger;

    public UserRepository(
        AppDbContext context,
        ILogger<IUserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AddAsync(
        User newUser, 
        CancellationToken ct)
    {
        try
        {
            await _context.AddAsync(newUser, ct);
            await _context.SaveChangesAsync(ct);

            return true;   
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while adding a " + 
                "new user with Discord ID: {DiscordId}", 
                newUser.DiscordId);

            return false;
        }
    }

    public async Task<ICollection<User>> GetAllByDiscordIdsAsync(
        ICollection<ulong> discordIds, 
        CancellationToken ct)
    {
        if (discordIds == null || !discordIds.Any())
        {
            return Array.Empty<User>();
        }

        var distinctIds = discordIds.Distinct().ToList();
        var result = new List<User>();

        foreach (var chunk in distinctIds.Chunk(1000))
        {
            var users = await _context.Users
                .Where(i => chunk.Contains(i.DiscordId))
                .ToListAsync(ct);
                
            result.AddRange(users);
        }

        return result;
    }

    public async Task<User?> GetByDiscordIdAsync(
        ulong discordId, 
        CancellationToken ct)
    {
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.DiscordId == discordId, ct);
    }
}