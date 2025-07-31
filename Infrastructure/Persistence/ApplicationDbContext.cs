using Microsoft.EntityFrameworkCore;
using retoSquadmakers.Domain.Entities;

namespace retoSquadmakers.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Chiste> Chistes { get; set; } = null!;
    public DbSet<Tematica> Tematicas { get; set; } = null!;
    public DbSet<ChisteTematica> ChisteTematicas { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<NotificationPreference> NotificationPreferences { get; set; } = null!;
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Usuario entity
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Rol).IsRequired().HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).IsRequired();
        });

        // Chiste entity
        modelBuilder.Entity<Chiste>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Texto).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.FechaCreacion).IsRequired();
            entity.Property(e => e.Origen).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.Autor)
                  .WithMany(u => u.Chistes)
                  .HasForeignKey(e => e.AutorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Tematica entity
        modelBuilder.Entity<Tematica>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Nombre).IsUnique();
        });

        // ChisteTematica (many-to-many relationship)
        modelBuilder.Entity<ChisteTematica>(entity =>
        {
            entity.HasKey(e => new { e.ChisteId, e.TematicaId });
            
            entity.HasOne(e => e.Chiste)
                  .WithMany(c => c.ChisteTematicas)
                  .HasForeignKey(e => e.ChisteId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasOne(e => e.Tematica)
                  .WithMany(t => t.ChisteTematicas)
                  .HasForeignKey(e => e.TematicaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Notification entity
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Recipient).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Priority).IsRequired();
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.UserId, e.Status });
            entity.HasIndex(e => e.CreatedAt);
        });

        // NotificationPreference entity
        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NotificationType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsEnabled).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasOne(e => e.User)
                  .WithMany(u => u.NotificationPreferences)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(e => new { e.UserId, e.NotificationType, e.EventType }).IsUnique();
        });

        // NotificationTemplate entity
        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TemplateId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasIndex(e => new { e.TemplateId, e.Type }).IsUnique();
        });
    }
}