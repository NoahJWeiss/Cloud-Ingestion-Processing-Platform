using CloudIngestion.Core.Interfaces;
using CloudIngestion.Infrastructure.Data;
using CloudIngestion.Infrastructure.Queue;
using CloudIngestion.Infrastructure.Repositories;
using CloudIngestion.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudIngestion.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Storage
        var storageOptions = new LocalObjectStorageOptions();
        configuration.GetSection("Storage").Bind(storageOptions);
        services.AddSingleton(storageOptions);
        services.AddScoped<IObjectStorage, LocalObjectStorage>();

        // Queue
        services.AddScoped<IProcessingQueue, DatabaseProcessingQueue>();

        // Repository
        services.AddScoped<IFileRepository, FileRepository>();

        return services;
    }
}
