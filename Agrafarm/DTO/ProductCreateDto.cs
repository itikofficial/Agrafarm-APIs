namespace Agrafarm.DTO
{
    public class ProductCreateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
       
        public int CategoryId { get; set; }
        public List<string> ImageUrls { get; set; } = new();
        public IFormFile[]? Images { get; set; }

        // This will hold URLs after uploading, used by service
        
    }
}
