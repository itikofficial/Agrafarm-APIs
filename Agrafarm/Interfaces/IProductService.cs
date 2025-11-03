using Agrafarm.DTO;

namespace Agrafarm.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<IEnumerable<ProductDto>> GetAllActiveProducts();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(ProductCreateDto dto);
        Task<bool> UpdateProductAsync(int id, ProductCreateDto dto);
        Task<(bool Success, string Message, bool IsActive)> ToggleProductStatusAsync(int id);
        Task<bool> DeleteProductAsync(int id);
    }
}
