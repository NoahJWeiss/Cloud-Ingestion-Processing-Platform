using CloudIngestion.Infrastructure;
using CloudIngestion.Infrastructure.Data;
using CloudIngestion.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<FileProcessingWorker>();

var host = builder.Build();

// Apply migrations on startup
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

host.Run();
