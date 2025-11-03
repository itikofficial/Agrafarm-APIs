using Agrafarm.DTO;
using Agrafarm.Interfaces;
using Agrafarm.Model;
using AutoMapper;

namespace Agrafarm.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        public async Task<IEnumerable<ProductDto>> GetAllActiveProducts()
        {
            var products = await _productRepository.ActiveProductsAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product == null ? null : _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(ProductCreateDto dto)
        {
            var product = _mapper.Map<Product>(dto);
            product.Images = dto.ImageUrls.Select(url => new ProductImage { ImageUrl = url }).ToList();

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> UpdateProductAsync(int id, ProductCreateDto dto)
        {
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null) return false;

            // Update basic fields (name, description, category, etc.)
            _mapper.Map(dto, existing);

            // If new image urls were provided -> replace images
            if (dto.ImageUrls != null && dto.ImageUrls.Any())
            {
                // Ensure existing.Images is a List to manipulate
                if (existing.Images == null)
                    existing.Images = new List<ProductImage>();

                // Remove old image entities from the collection so EF will delete them
                // (use ToList() to avoid modifying collection while iterating)
                var oldImages = existing.Images.ToList();
                foreach (var old in oldImages)
                {
                    existing.Images.Remove(old);
                    // If you want EF to cascade delete, ensure ProductImage is tracked by the context and relationship is configured
                }

                // Add new image entities from dto.ImageUrls
                existing.Images = dto.ImageUrls.Select(url => new ProductImage
                {
                    ImageUrl = url,
                    ProductId = existing.Id
                }).ToList();
            }

            _productRepository.Update(existing);
            await _productRepository.SaveChangesAsync();

            return true;
        }

        public async Task<(bool Success, string Message, bool IsActive)> ToggleProductStatusAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return (false, "Product not found", false);

            product.IsActive = !product.IsActive; // ✅ Toggle status
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            
            string message = product.IsActive ? "Product activated successfully." : "Product deactivated successfully.";
            return (true, message, product.IsActive);
        }




        public async Task<bool> DeleteProductAsync(int id)
        {
            var existing = await _productRepository.GetByIdAsync(id);
            if (existing == null) return false;

            _productRepository.Delete(existing);
            await _productRepository.SaveChangesAsync();
            return true;
        }
    }
}
