using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MarketPlace.DAL.Repositories.Factories
{
    public class MarketPlaceDbContextFactory : IDesignTimeDbContextFactory<MarketPlaceDbContext>
    {
        public MarketPlaceDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var dbPath = Path.Combine(basePath, "MarketPlaceDatabase.db");
            
            var optionsBuilder = new DbContextOptionsBuilder<MarketPlaceDbContext>();
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            
            return new MarketPlaceDbContext(optionsBuilder.Options);
        }
    }
}