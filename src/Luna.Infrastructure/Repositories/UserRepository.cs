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

    public async Task<bool> AddUserAsync(
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

    public async Task<User?> GetByDiscordIdAsync(
        ulong discordId, 
        CancellationToken ct)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.DiscordId == discordId, ct);
    }

    public async Task<bool> SetUserRoleAsync(
        ulong discordId, 
        UserRole role, 
        CancellationToken ct)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.DiscordId == discordId, ct);

            if (user is not null)
            {
                user?.SetRole(role);
                await _context.SaveChangesAsync(ct);
                return true;
            }
            else throw new KeyNotFoundException(
                $"User not found: Discord ID = {discordId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while updating a role " + 
                "of a user with Discord ID: {DiscordId}", 
                discordId);

            return false;
        }
    }
}