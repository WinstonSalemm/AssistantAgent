using Assistant.Infrastructure.Data;
using Assistant.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
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
app.MapGet("/health/db", async (AssistantDbContext context, IConfiguration configuration) =>
{
    try
    {
        // Пробуем несколько способов получить connection string
        var cs1 = configuration.GetConnectionString("DefaultConnection");
        var cs2 = configuration["ConnectionStrings:DefaultConnection"];
        var cs3 = configuration["ConnectionStrings__DefaultConnection"];
        var cs4 = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        
        var connectionString = cs1 ?? cs2 ?? cs3 ?? cs4;
        
        var connectionStringStatus = string.IsNullOrWhiteSpace(connectionString) 
            ? "❌ NOT SET" 
            : $"✅ SET (length: {connectionString.Length}, starts with: {connectionString.Substring(0, Math.Min(20, connectionString.Length))}...)";
        
        var canConnect = false;
        var pendingMigrations = new List<string>();
        var appliedMigrations = new List<string>();
        bool tablesExist = false;
        string? dbError = null;
        
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                canConnect = await context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
                    appliedMigrations = (await context.Database.GetAppliedMigrationsAsync()).ToList();
                    
                    // Проверяем что таблица Tasks существует
                    try
                    {
                        var result = await context.Database.ExecuteSqlRawAsync(
                            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Tasks'");
                        tablesExist = result > 0;
                    }
                    catch
                    {
                        tablesExist = false;
                    }
                }
            }
            catch (Exception dbEx)
            {
                dbError = $"{dbEx.GetType().Name}: {dbEx.Message}";
                if (dbEx.InnerException != null)
                {
                    dbError += $" | Inner: {dbEx.InnerException.GetType().Name}: {dbEx.InnerException.Message}";
                    if (dbEx.InnerException.InnerException != null)
                    {
                        dbError += $" | Inner2: {dbEx.InnerException.InnerException.Message}";
                    }
                }
                // Добавляем более детальную информацию для диагностики
                if (dbEx is Npgsql.NpgsqlException npgsqlEx)
                {
                    dbError += $" | Npgsql Code: {npgsqlEx.SqlState} | Message: {npgsqlEx.Message}";
                }
            }
        }
        
        return Results.Ok(new
        {
            connectionStringStatus,
            connectionStringPreview = connectionString != null 
                ? connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "..." 
                : null,
            connectionStringSources = new
            {
                GetConnectionString = !string.IsNullOrWhiteSpace(cs1),
                ConfigBrackets = !string.IsNullOrWhiteSpace(cs2),
                ConfigUnderscore = !string.IsNullOrWhiteSpace(cs3),
                EnvironmentVar = !string.IsNullOrWhiteSpace(cs4)
            },
            canConnect,
            pendingMigrations,
            appliedMigrations,
            tablesExist,
            error = dbError,
            hint = string.IsNullOrWhiteSpace(connectionString) 
                ? "⚠️ Add ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}} in Railway Variables"
                : (!canConnect && string.IsNullOrWhiteSpace(dbError)
                    ? "⚠️ Check Railway Deploy Logs for database connection errors"
                    : null)
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: $"Error: {ex.Message}\nInner: {ex.InnerException?.Message}\n\n" +
            "💡 Solution: Add ConnectionStrings__DefaultConnection=${{Postgres.DATABASE_URL}} in Railway Variables");
    }
});

// Auto-migrate database on startup (для Railway)
if (app.Environment.IsProduction())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AssistantDbContext>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        // Пробуем получить connection string
        var connectionString = config.GetConnectionString("DefaultConnection") 
            ?? config["ConnectionStrings:DefaultConnection"]
            ?? config["ConnectionStrings__DefaultConnection"]
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        
        // Fallback: собираем из PG* переменных
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            var host = Environment.GetEnvironmentVariable("PGHOST");
            var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
            var db = Environment.GetEnvironmentVariable("PGDATABASE");
            var user = Environment.GetEnvironmentVariable("PGUSER");
            var pass = Environment.GetEnvironmentVariable("PGPASSWORD");
            
            if (!string.IsNullOrWhiteSpace(host) && 
                !string.IsNullOrWhiteSpace(db) && 
                !string.IsNullOrWhiteSpace(user) && 
                !string.IsNullOrWhiteSpace(pass))
            {
                connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass};SSL Mode=Prefer;Trust Server Certificate=true;";
                Log.Information("Built connection string from PG* environment variables");
            }
        }
        
        Log.Information("Database connection string preview: {Preview}", 
            connectionString != null && connectionString.Length > 30 
                ? connectionString.Substring(0, 30) + "..." 
                : connectionString ?? "NULL");
        
        // Логируем информацию о connection string для диагностики
        if (connectionString != null)
        {
            Log.Information("Connection string length: {Length}, starts with: {Start}", 
                connectionString.Length, 
                connectionString.Substring(0, Math.Min(20, connectionString.Length)));
            Log.Information("Connection string contains sslmode: {HasSslMode}", 
                connectionString.Contains("sslmode", StringComparison.OrdinalIgnoreCase));
        }
        
        // Логируем PG* переменные для диагностики
        Log.Information("[DB] Using host: {Host}, db: {Database}", 
            Environment.GetEnvironmentVariable("PGHOST"), 
            Environment.GetEnvironmentVariable("PGDATABASE"));
        
        // Получаем реальный connection string из DbContext для диагностики
        try
        {
            var dbConnection = context.Database.GetDbConnection();
            var realConnectionString = dbConnection.ConnectionString;
            var realPreview = realConnectionString.Length > 50 
                ? realConnectionString.Substring(0, 50) + "..." 
                : realConnectionString;
            var hasSslInReal = realConnectionString.Contains("sslmode", StringComparison.OrdinalIgnoreCase) 
                            || realConnectionString.Contains("SSL Mode", StringComparison.OrdinalIgnoreCase);
            
            Log.Information("Real connection string from DbContext - Preview: {Preview}, Has SSL: {HasSsl}", 
                realPreview, hasSslInReal);
        }
        catch
        {
            // Игнорируем ошибки получения connection string
        }
        
        Log.Information("Checking database connection...");
        try
        {
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
            catch (Exception dbEx)
            {
                Log.Error(dbEx, "❌ Database connection failed: {Message}", dbEx.Message);
                Log.Error("Inner exception: {InnerException}", dbEx.InnerException?.Message);
                
                if (dbEx is NpgsqlException npgsqlEx)
                {
                    Log.Error("Npgsql Error - Code: {SqlState}, Message: {Message}", 
                        npgsqlEx.SqlState, npgsqlEx.Message);
                    Log.Error("Npgsql Error - IsTransient: {IsTransient}, Data: {Data}", 
                        npgsqlEx.IsTransient, string.Join(", ", npgsqlEx.Data.Keys));
                }
                
                // Логируем полный stack trace для диагностики
                if (dbEx.StackTrace != null)
                {
                    Log.Error("Full stack trace: {StackTrace}", dbEx.StackTrace);
                }
            }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "❌ Error during database initialization: {Message}", ex.Message);
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
