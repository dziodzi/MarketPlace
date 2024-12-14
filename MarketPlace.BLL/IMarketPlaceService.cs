
using MarketPlace.DAL.Models.Entities;
using MarketPlace.DAL.Models.Responses;

namespace MarketPlace.BLL;

public interface IMarketPlaceService
{
    Task<ApiResponse<string>> AddMarketAsync(string name, string address);
    Task<ApiResponse<Market>> GetMarketById(string marketId);
    Task<ApiResponse<string>> AddProductAsync(string productName);
    Task<ApiResponse<Product>> GetProductByName(string productName);
    Task<ApiResponse<string>> AddProductToMarketAsync(string marketId, string productName, int amount, decimal price);
    Task<ApiResponse<Market>> GetMarketWithCheapestProductAsync(string productName);
    Task<ApiResponse<Dictionary<string, int>>> GetAvailableProductsAsync(string marketId, decimal moneyToBuy);
    Task<ApiResponse<decimal?>> BuyProductsAsync(string marketId, Dictionary<string, int> productsToBuy);
    Task<ApiResponse<Market>> GetMarketWithBestPriceForBatch(Dictionary<string, int> productsToBuy);
}