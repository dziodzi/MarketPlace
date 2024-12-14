using MarketPlace.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

public class MarketPlaceDbContext : DbContext
{
    public DbSet<Market> Markets { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductInMarket> ProductsInMarkets { get; set; }

    public MarketPlaceDbContext(DbContextOptions<MarketPlaceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Market>().HasKey(s => s.Id);
        modelBuilder.Entity<Product>().HasKey(p => p.Name);

        modelBuilder.Entity<ProductInMarket>()
            .HasKey(i => new { i.MarketId, i.ProductName });

        modelBuilder.Entity<ProductInMarket>()
            .HasOne<Market>()
            .WithMany()
            .HasForeignKey(i => i.MarketId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProductInMarket>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(i => i.ProductName)
            .OnDelete(DeleteBehavior.Restrict);
    }
}