using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Products.Server
{
    public static class ProductEndpoints
    {
        public static void MapProductEndpoints(this IEndpointRouteBuilder app)
        {

            var group = app.MapGroup("/api/products")
                           .WithTags("Products"); 


            group.MapGet("/", async (IProductRepository repository, ILogger<DataSeeder> logger) =>
            {
                try
                {
                    var summaries = await repository.GetSummariesAsync();
                    return Results.Ok(summaries);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to retrieve product summary list.");
                    return Results.Problem("An error occurred while compiling the product catalog.");
                }
            })
            .WithName("GetProducts")
            .WithSummary("Get a summary list of all products")
            .WithDescription("Returns minimal structures required for the UI catalog view.");


            group.MapGet("/{id:int}", async (int id, IProductRepository repository, ILogger<DataSeeder> logger) =>
            {
                try
                {
                    var detail = await repository.GetDetailByIdAsync(id);
                    return detail is not null
                        ? Results.Ok(detail)
                        : Results.NotFound(new { Message = $"Product with ID {id} was not found." });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error fetching product details for ID {Id}", id);
                    return Results.Problem("Internal evaluation boundary encountered.");
                }
            })
            .WithName("GetProductById")
            .WithSummary("Get full details of a specific product")
            .WithDescription("Returns comprehensive description profiles matching an asset key index.");


            group.MapGet("/metrics", async (IProductRepository repository, ILogger<DataSeeder> logger) =>
            {
                try
                {
                    var metrics = await repository.GetMetricsAsync();
                    return Results.Ok(metrics);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to compute catalog metrics data.");
                    return Results.Problem("An error occurred while grouping database metrics.");
                }
            })
            .WithName("GetProductMetrics")
            .WithSummary("Fetch database summary metrics")
            .WithDescription("Calculates totals, averages, price boundaries, and unit distribution charts.");


        }
    }
}