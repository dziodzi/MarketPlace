using MarketPlace.DAL.Interfaces;
using MarketPlace.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.DAL.Repositories
{
    public class DbMarketPlaceRepository(MarketPlaceDbContext context) : IMarketPlaceRepository
    {
        private readonly MarketPlaceDbContext _context = context;

        public async Task AddMarketAsync(Market market)
        {
            _context.Markets.Add(market);
            await _context.SaveChangesAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task AddProductInMarketAsync(Market market, Product product, int amount, decimal price)
        {
            var productInMarket = await _context.ProductsInMarkets
                .FirstOrDefaultAsync(i => i.MarketId == market.Id && i.ProductName == product.Name);

            if (productInMarket == null)
            {
                productInMarket = new ProductInMarket
                {
                    MarketId = market.Id,
                    ProductName = product.Name,
                    Amount = amount,
                    Price = price
                };
                await _context.ProductsInMarkets.AddAsync(productInMarket);
            }
            else
            {
                productInMarket.Amount += amount;
                if (price > 0) productInMarket.Price = price;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Market> FindCheapestMarketWithProductAsync(string productName)
        {
            var cheapestMarketId = await _context.ProductsInMarkets
                .Where(i => i.ProductName == productName && i.Amount > 0)
                .OrderBy(i => (double)i.Price)
                .Select(i => i.MarketId)
                .FirstOrDefaultAsync();

            return await _context.Markets.FindAsync(cheapestMarketId);
        }

        public async Task<Dictionary<string, int>> GetAvailableProductsAsync(Market market, decimal moneyToBuy)
        {
            var availableProducts = await _context.ProductsInMarkets
                .Where(i => i.MarketId == market.Id && i.Amount > 0 && i.Price <= moneyToBuy)
                .OrderBy(i => (double)i.Price)
                .Select(i => new
                {
                    ProductName = i.ProductName,
                    Price = i.Price,
                    Amount = i.Amount
                })
                .ToListAsync();

            var result = new Dictionary<string, int>();

            foreach (var item in availableProducts)
            {
                var maxAmount = (int)(moneyToBuy / item.Price);
                if (item.Amount < maxAmount) maxAmount = item.Amount;

                result[item.ProductName] = maxAmount;
            }

            return result;
        }

        public async Task<decimal?> CalculateTotalPriceAsync(Market market, Dictionary<Product, int> productAmounts)
        {
            decimal totalCost = 0;

            foreach (var (product, amount) in productAmounts)
            {
                var productInMarket = await _context.ProductsInMarkets
                    .FirstOrDefaultAsync(i => i.MarketId == market.Id && i.ProductName == product.Name);

                if (productInMarket == null || productInMarket.Amount < amount)
                {
                    return null;
                }

                totalCost += productInMarket.Price * amount;
            }

            return totalCost;
        }

        public async Task<Market> FindMarketWithCheapestBatchPriceAsync(Dictionary<Product, int> productAmounts)
        {
            var markets = await _context.Markets.ToListAsync();
            Market bestMarket = null;
            var lowestTotalCost = decimal.MaxValue;

            foreach (var market in markets)
            {
                var cost = await CalculateTotalPriceAsync(market, productAmounts);
                if (!cost.HasValue || cost.Value >= lowestTotalCost) continue;
                bestMarket = market;
                lowestTotalCost = cost.Value;
            }

            return bestMarket;
        }

        public async Task<Market> FindMarketByIdAsync(Guid marketId)
        {
            return await _context.Markets.FindAsync(marketId);
        }

        public async Task<Product> FindProductByNameAsync(string productName)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Name == productName);
        }
    }
}
