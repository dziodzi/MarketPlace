using MarketPlace.BLL;
using MarketPlace.DAL.Models.DTO;
using MarketPlace.DAL.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IMarketPlaceService _marketPlaceService;

        public ProductController(IMarketPlaceService marketPlaceService)
        {
            _marketPlaceService = marketPlaceService;
        }

        /// <summary>
        /// Add a new product.
        /// </summary>
        [HttpPost("product")]
        public async Task<IActionResult> CreateProduct(ProductDto productDto)
        {
            var response = await _marketPlaceService.AddProductAsync(productDto.Name);
            return CreateResponse(response);
        }

        /// <summary>
        /// Get a product by its name.
        /// </summary>
        [HttpGet("product/{productName}")]
        public async Task<IActionResult> GetProduct(string productName)
        {
            var response = await _marketPlaceService.GetProductByName(productName);
            return CreateResponse(response);
        }

        /// <summary>
        /// Get the cheapest market for a specific product.
        /// </summary>
        [HttpGet("cheapest-market/{productName}")]
        public async Task<IActionResult> GetCheapestMarketForProduct(string productName)
        {
            var response = await _marketPlaceService.GetMarketWithCheapestProductAsync(productName);
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