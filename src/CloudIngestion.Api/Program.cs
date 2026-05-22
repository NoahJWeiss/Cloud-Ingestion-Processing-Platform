using CloudIngestion.Core.Services;
using CloudIngestion.Infrastructure;
using CloudIngestion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<UploadValidator>(sp =>
{
    var validator = new UploadValidator();
    var maxMb = builder.Configuration.GetValue<int?>("Upload:MaxFileSizeMb");
    if (maxMb.HasValue)
        validator.MaxFileSizeBytes = maxMb.Value * 1024L * 1024L;
    return validator;
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                      ?? ["http://localhost:5173"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply migrations automatically in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors();
app.MapControllers();

app.Run();

// Expose for integration test host
public partial class Program { }
