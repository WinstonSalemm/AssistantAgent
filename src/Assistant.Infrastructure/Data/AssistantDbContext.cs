using Assistant.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Assistant.Infrastructure.Data;

public class AssistantDbContext : DbContext
{
    public AssistantDbContext(DbContextOptions<AssistantDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }
    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<Memory> Memories { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Role).IsRequired();
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.CreatedAt);
        });

        // TaskEntity configuration
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.HasIndex(e => e.IsCompleted);
            entity.HasIndex(e => e.DueDate);
        });

        // Reminder configuration
        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.RemindAt);
            entity.HasIndex(e => e.IsCompleted);
        });

        // Memory configuration with real[] array (без pgvector)
        modelBuilder.Entity<Memory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            
            // Используем real[] массив вместо vector типа
            // OpenAI ada-002 dimension: 1536
            entity.Property(e => e.Embedding)
                .HasColumnType("real[]"); // PostgreSQL массив чисел
            
            entity.Property(e => e.Metadata).HasColumnType("jsonb");
            entity.HasIndex(e => e.CreatedAt);
        });

        // UserPreference configuration
        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Value).IsRequired();
            entity.HasIndex(e => e.Key).IsUnique();
        });
    }
}
