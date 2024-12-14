using System.Globalization;
using MarketPlace.DAL.Interfaces;
using MarketPlace.DAL.Models.Entities;

namespace MarketPlace.DAL.Repositories
{
    public class FileMarketPlaceRepository : IMarketPlaceRepository
    {
        private readonly string _productFile;
        private readonly string _marketFile;
        private readonly string _productInMarketFile;
        private List<string> _productCache = [];
        private List<string> _marketCache = [];
        private List<string> _productInMarketCache = [];
        private static readonly string[] first = new[] { "MarketId,ProductName,Amount,Price" };

        public FileMarketPlaceRepository(string productFile, string marketFile, string productInMarketFile)
        {
            _productFile = productFile;
            _marketFile = marketFile;
            _productInMarketFile = productInMarketFile;

            EnsureFileExists(_productFile, "Name");
            EnsureFileExists(_marketFile, "MarketId,MarketName,Address");
            EnsureFileExists(_productInMarketFile, "MarketId,ProductName,Amount,Price");

            LoadFilesIntoCache();
        }

        private static void EnsureFileExists(string filePath, string header)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            if (!File.Exists(filePath)) File.WriteAllText(filePath, header + "\n");
        }

        private void LoadFilesIntoCache()
        {
            _productCache = File.ReadAllLines(_productFile).ToList();
            _marketCache = File.ReadAllLines(_marketFile).ToList();
            _productInMarketCache = File.ReadAllLines(_productInMarketFile).ToList();
        }

        private async Task WriteCacheToFileAsync()
        {
            await File.WriteAllLinesAsync(_productFile, _productCache);
            await File.WriteAllLinesAsync(_marketFile, _marketCache);
            await File.WriteAllLinesAsync(_productInMarketFile, _productInMarketCache);
        }

        public async Task AddMarketAsync(Market market)
        {
            var marketLine = $"{market.Id},{market.Name},{market.Address}";
            _marketCache.Add(marketLine);
            await WriteCacheToFileAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            var productLine = $"{product.Name}";
            _productCache.Add(productLine);
            await WriteCacheToFileAsync();
        }

        public async Task AddProductInMarketAsync(Market market, Product product, int amount, decimal price)
        {
            var productInMarketLines = _productInMarketCache.Skip(1).ToList();
            var existingLineIndex = productInMarketLines.FindIndex(line =>
            {
                var data = line.Split(',');
                return data[0] == market.Id.ToString() && data[1] == product.Name;
            });

            if (existingLineIndex >= 0)
            {
                var data = productInMarketLines[existingLineIndex].Split(',');
                var currentAmount = int.Parse(data[2]);
                var currentPrice = decimal.Parse(data[3], CultureInfo.InvariantCulture);

                var newAmount = currentAmount + amount;
                var newPrice = price > 0 ? price : currentPrice;

                productInMarketLines[existingLineIndex] = $"{market.Id},{product.Name},{newAmount},{newPrice.ToString(CultureInfo.InvariantCulture)}";
            }
            else
            {
                var newLine = $"{market.Id},{product.Name},{amount},{price.ToString(CultureInfo.InvariantCulture)}";
                productInMarketLines.Add(newLine);
            }

            _productInMarketCache = first.Concat(productInMarketLines).ToList();
            await WriteCacheToFileAsync();
        }

        public async Task<Market> FindCheapestMarketWithProductAsync(string productName)
        {
            var productInMarketLines = _productInMarketCache.Skip(1);
            decimal? lowestPrice = null;
            string marketId = null;

            foreach (var line in productInMarketLines)
            {
                var data = line.Split(',');
                if (data[1] != productName || int.Parse(data[2]) <= 0) continue;
                var price = decimal.Parse(data[3], CultureInfo.InvariantCulture);
                if (lowestPrice.HasValue && price >= lowestPrice.Value) continue;
                lowestPrice = price;
                marketId = data[0];
            }

            return marketId != null ? await FindMarketByIdAsync(Guid.Parse(marketId)) : null;
        }

        public async Task<Dictionary<string, int>> GetAvailableProductsAsync(Market market, decimal moneyToBuy)
        {
            var productInMarketLines = _productInMarketCache.Skip(1);
            var result = new Dictionary<string, int>();

            var availableItems = productInMarketLines
                .Select(line => line.Split(','))
                .Where(data => Guid.Parse(data[0]) == market.Id && int.Parse(data[2]) > 0)
                .OrderBy(data => (double)decimal.Parse(data[3], CultureInfo.InvariantCulture));

            foreach (var item in availableItems)
            {
                var productName = item[1];
                var amount = int.Parse(item[2]);
                var price = decimal.Parse(item[3], CultureInfo.InvariantCulture);
                var maxAmount = (int)(moneyToBuy / price);
                if (amount < maxAmount) maxAmount = amount;

                result[productName] = maxAmount;
            }

            return result;
        }

        public async Task<decimal?> CalculateTotalPriceAsync(Market market, Dictionary<Product, int> productAmounts)
        {
            decimal totalCost = 0;
            var productInMarketLines = _productInMarketCache.Skip(1);

            foreach (var (product, amount) in productAmounts)
            {
                var item = productInMarketLines
                    .Select(line => line.Split(','))
                    .FirstOrDefault(data => Guid.Parse(data[0]) == market.Id && data[1] == product.Name);

                if (item == null || int.Parse(item[2]) < amount)
                    return null;

                var price = decimal.Parse(item[3], CultureInfo.InvariantCulture);
                totalCost += price * amount;
            }

            return totalCost;
        }

        public async Task<Market> FindMarketWithCheapestBatchPriceAsync(Dictionary<Product, int> productAmounts)
        {
            var marketLines = _marketCache.Skip(1);
            Market bestMarket = null;
            var lowestTotalCost = decimal.MaxValue;

            foreach (var line in marketLines)
            {
                var data = line.Split(',');
                var market = new Market
                {
                    Id = Guid.Parse(data[0]),
                    Name = data[1],
                    Address = data[2]
                };

                var cost = await CalculateTotalPriceAsync(market, productAmounts);
                if (!cost.HasValue || cost.Value >= lowestTotalCost) continue;
                bestMarket = market;
                lowestTotalCost = cost.Value;
            }

            return bestMarket;
        }

        public async Task<Market> FindMarketByIdAsync(Guid marketId)
        {
            var marketLine = _marketCache.Skip(1)
                .Select(line => line.Split(','))
                .FirstOrDefault(data => Guid.Parse(data[0]) == marketId);

            if (marketLine == null) return null;

            return new Market
            {
                Id = marketId,
                Name = marketLine[1],
                Address = marketLine[2]
            };
        }

        public async Task<Product> FindProductByNameAsync(string productName)
        {
            var productLine = _productCache.Skip(1)
                .Select(line => line.Split(','))
                .FirstOrDefault(data => data[0] == productName);

            return new Product
            {
                Name = productLine[0]
            };
        }
    }
}