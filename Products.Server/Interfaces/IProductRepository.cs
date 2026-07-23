using System;
using System.Collections.Generic;
using System.Text;

namespace Products.Server
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductSummaryDto>> GetSummariesAsync();
        Task<ProductDetailDto?> GetDetailByIdAsync(int id);
        Task<CatalogMetricsDto> GetMetricsAsync();
    }
}
