using Luna.Application.Common.Interfaces;
using Luna.Domain.Entities;
using Luna.Domain.Enums;
using Luna.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Luna.Infrastructure.Repositories;

public sealed class ExecutorRepository : IExecutorRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ExecutorRepository> _logger;

    public ExecutorRepository(
        ILogger<ExecutorRepository> logger,
        AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<bool> AddAsync(ulong discordId, Executor newExecutor, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.DiscordId == discordId, ct);
        
        if (user is null)
            return false;
        
        var executor = await _context.Executors.FindAsync(user.Id, ct);

        if (executor is not null)
            return false;

        try
        {
            newExecutor.UserId = user.Id;
            _context.Executors.Add(newExecutor);
            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error occurred while adding an executor " + 
                "with Discord ID: {DiscordId}", discordId);

            return false;
        }
    }

    public async Task<ICollection<Executor>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Executors
            .AsNoTracking()
            .Include(e => e.User)
            .ToListAsync(ct);
    }

    public async Task<Executor?> GetByDiscordIdAsync(ulong discordId, CancellationToken ct)
    {
        return await _context.Executors
            .AsNoTracking()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.User.DiscordId == discordId, ct);
    }

    public async Task<bool> SetRoleAsync(
        ulong discordId, 
        ExecutorRole role, 
        CancellationToken ct)
    {
        try
        {
            var executor = await _context.Executors
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.User.DiscordId == discordId, ct);

            if (executor is not null)
            {
                executor.Role = role;
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
                "of a executor with Discord ID: {DiscordId}", 
                discordId);

            return false;
        }
    }
}