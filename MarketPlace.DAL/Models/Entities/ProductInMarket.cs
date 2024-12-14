namespace MarketPlace.DAL.Models.Entities
{
    public class ProductInMarket
    {
        public Guid MarketId { get; set; }
        public string ProductName { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
    }
}