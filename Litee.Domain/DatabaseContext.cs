using Litee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Litee.Domain;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
  public DbSet<User> Users { get; set; }
  public DbSet<Account> Accounts { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Account>(entity =>
    {
      entity.HasKey(a => a.Id);
      entity.Property(a => a.Name).IsRequired().HasMaxLength(50);
      entity.HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });
  }
}
