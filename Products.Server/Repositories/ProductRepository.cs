using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Products.Server
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;


        

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductSummaryDto>> GetSummariesAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Select(p => new ProductSummaryDto(p.Id, p.Title, p.Summary, p.UnitOfMeasure, p.ThumbnailImageUrl ?? p.ImageUrl))
                .ToListAsync();
        }

        public async Task<ProductDetailDto?> GetDetailByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new ProductDetailDto(p.Id, p.Title, p.Summary, p.Description, p.Price, p.ImageUrl))
                .FirstOrDefaultAsync();
        }

        public async Task<CatalogMetricsDto> GetMetricsAsync()
        {
            var rawProducts = await _context.Products.AsNoTracking().ToListAsync();
            if (!rawProducts.Any())
            {
                return new CatalogMetricsDto(0, 0, new(0, "N/A", "$0"), new(0, "N/A", "$0"), []);
            }

            
            var parsedProducts = rawProducts.Select(p =>
            {
                var match = Regex.Match(p.Price, @"\$([\d.]+)(.*)");
                decimal numericPrice = match.Success && decimal.TryParse(match.Groups[1].Value, out var val) ? val : 0m;
                string unit = match.Success ? match.Groups[2].Value.Trim() : "/each";
                return new { Product = p, NumericPrice = numericPrice, Unit = string.IsNullOrEmpty(unit) ? "/each" : unit };
            }).ToList();


            int totalCount = parsedProducts.Count;
            decimal averagePrice = Math.Round(parsedProducts.Average(p => p.NumericPrice), 2);

            var maxItem = parsedProducts.OrderByDescending(p => p.NumericPrice).First().Product;
            var minItem = parsedProducts.OrderBy(p => p.NumericPrice).First().Product;

            var priceUnitBreakdown = parsedProducts
                .GroupBy(p => p.Unit)
                .ToDictionary(g => g.Key, g => g.Count());

            return new CatalogMetricsDto(
                TotalProducts: totalCount,
                AveragePrice: averagePrice,
                MostExpensive: new ProductPriceMetric(maxItem.Id, maxItem.Title, maxItem.Price),
                LeastExpensive: new ProductPriceMetric(minItem.Id, minItem.Title, minItem.Price),
                ByPriceUnit: priceUnitBreakdown
            );
        }
    }

}