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
                dbPath = Path.Combine(builder.Environment.ContentRootPath, "app_data", "Products.db");

                var localDbFolder = Path.GetDirectoryName(dbPath);
                if (!Directory.Exists(localDbFolder))
                {
                    Directory.CreateDirectory(localDbFolder);
                }
            }

            
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

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                seeder.SeedAsync().GetAwaiter().GetResult();
            }

            
            app.UseCors("AllowAngularFrontend");
            app.MapProductEndpoints();
            app.UseDefaultFiles();
            app.UseStaticFiles(); 
            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
