using MarketPlace.BLL;
using MarketPlace.DAL.Models.DTO;
using MarketPlace.DAL.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductInMarketController : ControllerBase
    {
        private readonly IMarketPlaceService _marketPlaceService;

        public ProductInMarketController(IMarketPlaceService marketPlaceService)
        {
            _marketPlaceService = marketPlaceService;
        }

        /// <summary>
        /// Add a product to the market.
        /// </summary>
        [HttpPost("add-product-to-market")]
        public async Task<IActionResult> AddProductToMarket(ProductInMarketDto productsInMarketsDto)
        {
            var response = await _marketPlaceService.AddProductToMarketAsync(
                productsInMarketsDto.MarketId,
                productsInMarketsDto.ProductName,
                productsInMarketsDto.Amount,
                productsInMarketsDto.Price);
            return CreateResponse(response);
        }

        /// <summary>
        /// Get available products for a specific market within a given moneyToBuy.
        /// </summary>        
        [HttpGet("available-products/{marketId}")]
        public async Task<IActionResult> GetAvailableProducts(string marketId, decimal moneyToBuy)
        {
            var response = await _marketPlaceService.GetAvailableProductsAsync(marketId, moneyToBuy);
            return CreateResponse(response);
        }

        /// <summary>
        /// Purchase product from a market.
        /// </summary>
        [HttpPatch("buy-products")]
        public async Task<IActionResult> BuyProducts(PurchaseDto purchaseDto)
        {
            var response = await _marketPlaceService.BuyProductsAsync(purchaseDto.MarketId.ToString(), purchaseDto.ProductsToBuy);
            return CreateResponse(response);
        }
        
        /// <summary>
        /// Get the best market to fulfill a batch purchase of products.
        /// </summary>
        [HttpPut("best-batch-price-market")]
        public async Task<IActionResult> GetMarketWithBestPriceForBatch([FromBody] Dictionary<string, int> productsToBuy)
        {
            var response = await _marketPlaceService.GetMarketWithBestPriceForBatch(productsToBuy);
            return CreateResponse(response);
        }

        private IActionResult CreateResponse<T>(ApiResponse<T> response)
        {
            return response.Code switch
            {
                ResponseCode.Success => Ok(response),
                ResponseCode.BadRequest => BadRequest(response),
                ResponseCode.NotFound => NotFound(response),
                _ => StatusCode(StatusCodes.Status500InternalServerError, response)
            };
        }
    }
}
