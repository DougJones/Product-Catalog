using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Products.Server
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;

        public DataSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {

            await _context.Database.EnsureCreatedAsync();


            if (await _context.Products.AnyAsync()) return;


            var detailsPath = Path.Combine(AppContext.BaseDirectory, "SeedData", "product-details.json");
            var imagesPath = Path.Combine(AppContext.BaseDirectory, "SeedData", "products.json");

            if (!File.Exists(detailsPath) || !File.Exists(imagesPath))
            {
                throw new FileNotFoundException("Required seed JSON files are missing from the SeedData folder.");
            }

            // 3. Read and deserialize the primary product details
            var detailsJson = await File.ReadAllTextAsync(detailsPath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var products = JsonSerializer.Deserialize<List<Product>>(detailsJson, options);

            if (products == null || products.Count == 0) return;


            var productMap = products.ToDictionary(p => p.Id);

            // 4. Read the secondary products JSON file to pull image mapping data
            var imagesJson = await File.ReadAllTextAsync(imagesPath);
            using var jsonDocument = JsonDocument.Parse(imagesJson);

            // Loop through the secondary array and patch the target thumbnail property
            foreach (var element in jsonDocument.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("id", out var idProp) &&
                    element.TryGetProperty("imageUrl", out var imgProp))
                {
                    int id = idProp.GetInt32();
                    string imageUrl = imgProp.GetString() ?? string.Empty;

                    if (productMap.TryGetValue(id, out var product))
                    {
                        product.ThumbnailImageUrl = imageUrl;
                    }
                }
            }

            // 5. Commit the fully hydated objects to SQLite
            await _context.Products.AddRangeAsync(productMap.Values);
            await _context.SaveChangesAsync();
        }
    }
}