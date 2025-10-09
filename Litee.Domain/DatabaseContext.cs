using Litee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Litee.Domain;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
  public DbSet<User> Users { get; set; }
  public DbSet<Account> Accounts { get; set; }
  public DbSet<Transaction> Transactions { get; set; }
  public DbSet<Category> Categories { get; set; }
  public DbSet<Receipt> Receipts { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Account>(entity =>
    {
      entity.HasKey(a => a.Id);
      entity.Property(a => a.Name).IsRequired().HasMaxLength(50);
      // Cascade delete when related User is deleted
      entity.HasOne(a => a.User)
            .WithMany(u => u.Accounts)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<Transaction>(entity =>
    {
      entity.HasKey(t => t.Id);
      entity.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(100);
      entity.Property(t => t.Payee)
            .IsRequired();
      entity.Property(t => t.Amount)
            .IsRequired();
      entity.Property(t => t.Date)
            .IsRequired();
      // Cascade delete when related Account is deleted
      entity.HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
      // Cascade delete when related User is deleted
      entity.HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
      // Cascade delete when related Receipt is deleted
      entity.HasOne(t => t.Receipt)
            .WithOne(u => u.Transaction)
            .HasForeignKey<Transaction>(t => t.ReceiptId)
            .OnDelete(DeleteBehavior.Cascade);
      // Set Null when related Category is deleted
      entity.HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    });

    modelBuilder.Entity<Category>(entity =>
    {
      entity.HasKey(c => c.Id);
      entity.Property(c => c.Name)
              .IsRequired()
              .HasMaxLength(50);
      // Cascade delete when related User is deleted
      entity.HasOne(c => c.User)
              .WithMany(u => u.Categories)
              .HasForeignKey(c => c.UserId)
              .OnDelete(DeleteBehavior.Cascade);
    });
  }
}
