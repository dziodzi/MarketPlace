namespace MarketPlace.DAL.Models.DTO;

public class ProductInMarketDto
{
    /// <summary>
    /// The code of the market.
    /// </summary>
    public string MarketId { get; set; }

    /// <summary>
    /// The name of the product.
    /// </summary>
    public string ProductName { get; set; }

    /// <summary>
    /// The amount of the product to add.
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// The price of the product.
    /// </summary>
    public decimal Price { get; set; }
}
