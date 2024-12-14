using MarketPlace.BLL;
using MarketPlace.DAL.Models.DTO;
using MarketPlace.DAL.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MarketPlace.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketController : ControllerBase
    {
        private readonly IMarketPlaceService _marketPlaceService;

        public MarketController(IMarketPlaceService marketPlaceService)
        {
            _marketPlaceService = marketPlaceService;
        }

        /// <summary>
        /// Add a new market.
        /// </summary>
        [HttpPost("market")]
        public async Task<IActionResult> CreateMarket(MarketDto marketDto)
        {
            var response = await _marketPlaceService.AddMarketAsync(marketDto.Name, marketDto.Address);
            return CreateResponse(response);
        }

        /// <summary>
        /// Get a market by its ID.
        /// </summary>
        [HttpGet("market/{marketId}")]
        public async Task<IActionResult> GetMarket(string marketId)
        {
            var response = await _marketPlaceService.GetMarketById(marketId);
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