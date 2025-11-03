using Agrafarm.Model;

namespace Agrafarm.DTO
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Category? Category { get; set; }
        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
