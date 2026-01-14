using Assistant.Infrastructure.Data;
using Assistant.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/assistant-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "AI Personal Assistant API", 
        Version = "v1",
        Description = "API для персонального AI-ассистента с агентной архитектурой"
    });
});

// Add Infrastructure services (Database, AI, Repositories, Agents)
builder.Services.AddInfrastructure(builder.Configuration);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Swagger включен для Development и Production (персональный проект)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Assistant API v1");
    c.RoutePrefix = string.Empty; // Swagger UI на корневом URL
});

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow 
}));

// Database status endpoint (для отладки)
app.MapGet("/health/db", async (AssistantDbContext context) =>
{
    try
    {
        var canConnect = await context.Database.CanConnectAsync();
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
        var appliedMigrations = (await context.Database.GetAppliedMigrationsAsync()).ToList();
        
        bool tablesExist = false;
        if (canConnect)
        {
            try
            {
                // Проверяем что таблица Tasks существует
                var result = await context.Database.ExecuteSqlRawAsync(
                    "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Tasks'");
                tablesExist = result > 0;
            }
            catch
            {
                tablesExist = false;
            }
        }
        
        return Results.Ok(new
        {
            canConnect,
            pendingMigrations,
            appliedMigrations,
            tablesExist,
            connectionString = canConnect ? "✅ Connected" : "❌ Not connected"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: $"Error: {ex.Message}\nInner: {ex.InnerException?.Message}");
    }
});

// Auto-migrate database on startup (для Railway)
if (app.Environment.IsProduction())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        
        Log.Information("Checking database connection...");
        var canConnect = await context.Database.CanConnectAsync();
        Log.Information("Database connection: {CanConnect}", canConnect);
        
        if (canConnect)
        {
            Log.Information("Applying database migrations...");
            await context.Database.MigrateAsync();
            Log.Information("✅ Database migrations applied successfully");
            
            // Проверяем что таблицы созданы
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                Log.Warning("⚠️ Pending migrations: {Migrations}", string.Join(", ", pendingMigrations));
            }
            else
            {
                Log.Information("✅ All migrations are up to date");
            }
        }
        else
        {
            Log.Error("❌ Cannot connect to database! Check ConnectionStrings__DefaultConnection");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Error applying database migrations: {Message}", ex.Message);
        Log.Error("Inner exception: {InnerException}", ex.InnerException?.Message);
        // Не падаем, если миграции не применились - возможно БД еще не создана
    }
}

try
{
    Log.Information("Starting AI Personal Assistant API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
