namespace MarketPlace.DAL.Models.DTO;

public class PurchaseDto
{
    /// <summary>
    /// ID of the market.
    /// </summary>
    public Guid MarketId { get; set; }

    /// <summary>
    /// A dictionary containing products and their amounts.
    /// </summary>
    public Dictionary<string, int> ProductsToBuy { get; set; }
}
