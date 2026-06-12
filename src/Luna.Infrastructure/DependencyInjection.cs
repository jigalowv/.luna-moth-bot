using Microsoft.EntityFrameworkCore;
using Luna.Application.Common.Interfaces;
using Luna.Infrastructure.Data;
using Luna.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Luna.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString("Default"));
            options.UseSnakeCaseNamingConvention();
        });
        
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventEditRepository, EventEditRepository>();
        services.AddScoped<IEventTypeRepository, EventTypeRepository>();
        services.AddScoped<IRecordAttendanceRepository, RecordAttendanceRepository>();
        services.AddScoped<IRecordRepository, RecordRepository>();

        return services;
    }
}