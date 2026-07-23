using System.Text.RegularExpressions;

namespace Products.Server
{
    public class Product
    {
        public required int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }
        
        public string? ThumbnailImageUrl { get; set; }
        
        public required string Summary { get; set; }
        public required string Price { get; set; }

        public decimal NumericPrice
        {
            get
            {
                // Strip out currency symbols, units, and spaces to leave only the number
                var match = Regex.Match(Price, @"(\d+(\.\d+)?)");
                if (match.Success && decimal.TryParse(match.Groups[1].Value, out decimal price))
                {
                    return price;
                }
                return 0.00m;
            }
        }

        public string UnitOfMeasure
        {
            get
            {
                int slashIndex = Price.IndexOf('/');
                if (slashIndex != -1)
                {
                    return Price.Substring(slashIndex).ToLower().Trim();
                }
                return "/each";
            }
        }

        public decimal? PricePerOunce => UnitOfMeasure == "/lb"
       ? Math.Round(NumericPrice / 16, 2, MidpointRounding.AwayFromZero)
       : null;

    }

    public record ProductSummaryDto(int Id, string Title, string Summary, string UnitOfMeasure, string ImageUrl);

    
    public record ProductDetailDto(int Id, string Title, string Summary, string Description, string Price, string ImageUrl);

    
    public record ProductPriceMetric(int Id, string Title, string Price);

    
    public record CatalogMetricsDto(
        int TotalProducts,
        decimal AveragePrice,
        ProductPriceMetric MostExpensive,
        ProductPriceMetric LeastExpensive,
        Dictionary<string, int> ByPriceUnit
    );

}
