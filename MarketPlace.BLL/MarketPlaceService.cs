using MarketPlace.BLL.Exceptions;
using MarketPlace.DAL.Interfaces;
using MarketPlace.DAL.Models.Entities;
using MarketPlace.DAL.Models.Responses;

namespace MarketPlace.BLL
{
    public class MarketPlaceService(IMarketPlaceRepository repository) : IMarketPlaceService
    {
        public async Task<ApiResponse<string>> AddMarketAsync(string name, string address)
        {
            try
            {
                var market = new Market
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Address = address
                };

                await repository.AddMarketAsync(market);
                return new ApiResponse<string>(ResponseCode.Success, ResponseMessages.MarketAdded,
                    market.Id.ToString());
            }
            catch (Exception ex)
            {
                return HandleException<string>(ex);
            }
        }

        public async Task<ApiResponse<Product>> GetProductByName(string productName)
        {
            try
            {
                var product = await repository.FindProductByNameAsync(productName);
                if (product == null)
                {
                    throw new ProductNotFoundException($"Product with name '{productName}' was not found.");
                }

                return new ApiResponse<Product>(ResponseCode.Success, ResponseMessages.ProductFound, product);
            }
            catch (Exception ex)
            {
                return HandleException<Product>(ex);
            }
        }

        public async Task<ApiResponse<string>> AddProductAsync(string productName)
        {
            try
            {
                if (await repository.FindProductByNameAsync(productName) != null)
                {
                    throw new ProductAlreadyExistException($"Product with name '{productName}' already exists.");
                }
                
                var product = new Product { Name = productName };
                await repository.AddProductAsync(product);
                return new ApiResponse<string>(ResponseCode.Success, ResponseMessages.ProductAdded, product.Name);
            }
            catch (Exception ex)
            {
                return HandleException<string>(ex);
            }
        }


        public async Task<ApiResponse<Market>> GetMarketById(string marketId)
        {
            try
            {
                Guid marketGuid = Guid.Parse(marketId.ToString());
                var market = await repository.FindMarketByIdAsync(marketGuid);
                if (market == null)
                {
                    throw new MarketNotFoundException($"Market with ID {marketId} was not found.");
                }

                return new ApiResponse<Market>(ResponseCode.Success, ResponseMessages.MarketFound, market);
            }
            catch (Exception ex)
            {
                return HandleException<Market>(ex);
            }
        }

        public async Task<ApiResponse<string>> AddProductToMarketAsync(string marketId, string productName, int amount, decimal price)
        {
            try
            {
                if (amount < 0)
                {
                    throw new InvalidNumberException($"Amount can't be less than zero ({amount}).");
                }
                if (price <= 0)
                {
                    throw new InvalidNumberException($"Price can't be zero or less ({price}).");
                }
                Guid marketGuid = Guid.Parse(marketId.ToString());
                var market = await repository.FindMarketByIdAsync(marketGuid);
                if (market == null)
                {
                    throw new MarketNotFoundException($"Market with ID {marketId} was not found.");
                }

                var product = await repository.FindProductByNameAsync(productName);

                await repository.AddProductInMarketAsync(market, product, amount, price);

                return new ApiResponse<string>(ResponseCode.Success, ResponseMessages.ProductAddedToProductInMarket);
            }
            catch (Exception ex)
            {
                return HandleException<string>(ex);
            }
        }

        public async Task<ApiResponse<Market>> GetMarketWithCheapestProductAsync(string productName)
        {
            try
            {
                var market = await repository.FindCheapestMarketWithProductAsync(productName);
                if (market == null)
                {
                    throw new ProductNotFoundException($"No market sells the product '{productName}'.");
                }

                return new ApiResponse<Market>(ResponseCode.Success, ResponseMessages.CheapestMarketFound, market);
            }
            catch (Exception ex)
            {
                return HandleException<Market>(ex);
            }
        }

        public async Task<ApiResponse<Dictionary<string, int>>> GetAvailableProductsAsync(string marketId, decimal moneyToBuy)
        {
            try
            {
                if (moneyToBuy < 0)
                {
                    throw new InvalidNumberException($"Balance can't be less than zero ({moneyToBuy}).");
                }
                Guid marketGuid = Guid.Parse(marketId.ToString());
                var market = await repository.FindMarketByIdAsync(marketGuid);
                if (market == null)
                {
                    throw new MarketNotFoundException($"Market with ID {marketId} was not found.");
                }

                var products = await repository.GetAvailableProductsAsync(market, moneyToBuy);
                return new ApiResponse<Dictionary<string, int>>(ResponseCode.Success,
                    ResponseMessages.AvailableProductsRetrieved, products);
            }
            catch (Exception ex)
            {
                return HandleException<Dictionary<string, int>>(ex);
            }
        }

        public async Task<ApiResponse<decimal?>> BuyProductsAsync(string marketId,
            Dictionary<string, int> productsToBuy)
        {
            try
            {
                Guid marketGuid = Guid.Parse(marketId.ToString());
                var market = await repository.FindMarketByIdAsync(marketGuid);
                if (market == null)
                {
                    throw new MarketNotFoundException($"Market with ID {marketId} was not found.");
                }

                var productAmounts = new Dictionary<Product, int>();
                foreach (var (productName, amount) in productsToBuy)
                {
                    if (amount < 0)
                    {
                        throw new InvalidNumberException($"Can't buy less than zero products ({amount}).");
                    }
                    var product = await repository.FindProductByNameAsync(productName);
                    if (product == null)
                    {
                        throw new ProductNotFoundException($"Product '{productName}' was not found.");
                    }

                    productAmounts[product] = amount;
                }

                var totalCost = await repository.CalculateTotalPriceAsync(market, productAmounts);
                if (totalCost == null)
                {
                    throw new InsufficientStockException("Not enough stock to complete the purchase.");
                }

                foreach (var (product, amount) in productAmounts)
                {
                    await repository.AddProductInMarketAsync(market, product, -amount, 0);
                }

                return new ApiResponse<decimal?>(ResponseCode.Success, ResponseMessages.PurchaseCompleted, totalCost);
            }
            catch (Exception ex)
            {
                return HandleException<decimal?>(ex);
            }
        }

        public async Task<ApiResponse<Market>> GetMarketWithBestPriceForBatch(
            Dictionary<string, int> productsToBuy)
        {
            try
            {
                var productAmounts = new Dictionary<Product, int>();
                foreach (var (productName, amount) in productsToBuy)
                {
                    var product = await repository.FindProductByNameAsync(productName);
                    if (product == null)
                    {
                        throw new ProductNotFoundException($"Product '{productName}' was not found.");
                    }

                    productAmounts[product] = amount;
                }

                var market = await repository.FindMarketWithCheapestBatchPriceAsync(productAmounts);
                if (market == null)
                {
                    throw new MarketNotFoundException("No market can fulfill the batch purchase requirements.");
                }

                return new ApiResponse<Market>(ResponseCode.Success, ResponseMessages.BestMarketFoundForBatchPurchase,
                    market);
            }
            catch (Exception ex)
            {
                return HandleException<Market>(ex);
            }
        }

        private static ApiResponse<T> HandleException<T>(Exception ex)
        {
            return ex switch
            {
                ProductAlreadyExistException => new ApiResponse<T>(ResponseCode.Conflict, ex.Message),
                MarketNotFoundException or ProductNotFoundException => new ApiResponse<T>(ResponseCode.NotFound,
                    ex.Message),
                InsufficientStockException or InvalidNumberException => new ApiResponse<T>(ResponseCode.BadRequest, ex.Message),
                _ => new ApiResponse<T>(ResponseCode.InternalError, ex.Message)
            };
        }
    }
}