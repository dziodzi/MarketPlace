using MarketPlace.DAL.Models.Entities;

namespace MarketPlace.DAL.Interfaces;

public interface IMarketPlaceRepository
{
    Task AddMarketAsync(Market market);
    Task<Market> FindMarketByIdAsync(Guid marketId);
    Task AddProductAsync(Product product);
    Task<Product> FindProductByNameAsync(string productName);
    Task AddProductInMarketAsync(Market market, Product product, int amount, decimal price);
    Task<Market> FindCheapestMarketWithProductAsync(string productName);
    Task<Dictionary<string, int>> GetAvailableProductsAsync(Market market, decimal moneyToBuy);
    Task<decimal?> CalculateTotalPriceAsync(Market market, Dictionary<Product, int> productAmounts);
    Task<Market> FindMarketWithCheapestBatchPriceAsync(Dictionary<Product, int> productAmounts);
}