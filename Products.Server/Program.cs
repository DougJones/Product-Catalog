using Microsoft.EntityFrameworkCore;

namespace Products.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var azureHome = Environment.GetEnvironmentVariable("HOME");
            string dbPath;

            if (!string.IsNullOrEmpty(azureHome))
            {
                dbPath = Path.Combine(azureHome, "site", "app_data", "products.db");

                var dbFolder = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(dbFolder))
                {
                    Directory.CreateDirectory(dbFolder);
                }
            }
            else
            {
                // Use the application content root so local dev uses the project app_data folder,
                // not the build output folder under bin/Debug.
                dbPath = Path.Combine(builder.Environment.ContentRootPath, "app_data", "Products.db");

                var localDbFolder = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(localDbFolder))
                {
                    Directory.CreateDirectory(localDbFolder);
                }
            }

            // 1. REGISTER THE CORS SECURITY COMPONENT (Keep it highly flexible for local dev debugging)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularFrontend", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<DataSeeder>();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}")
            );

            var app = builder.Build();

            // 2. DATA ENGINES SEED EXECUTION LIFECYCLE
            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                seeder.SeedAsync().GetAwaiter().GetResult();
            }

            // 3. CORE MIDDLEWARE ROUTING MATRIX PIPELINE ORDER (CRITICAL FIX)

            // Step A: Activate CORS headers FIRST so the browser accepts the raw data packet streams immediately
            app.UseCors("AllowAngularFrontend");

            // Step B: Map your API route controllers explicitly right after CORS.
            // Putting this high up guarantees that requests matching /api/products execute immediately, 
            // bypassing any fallback file interceptions.
            app.MapProductEndpoints();

            // Step C: Fallback serving mechanisms only trigger if a request does NOT match your API endpoints.
            app.UseDefaultFiles();
            app.UseStaticFiles(); // Added to safely deliver physical frontend scripts when compiled

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
