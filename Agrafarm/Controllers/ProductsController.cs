using Agrafarm.DTO;
using Agrafarm.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Agrafarm.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;

        public ProductsController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
       
        [HttpGet("active-products")]
        public async Task<IActionResult> ActivePrducts()
        {
            var products = await _productService.GetAllActiveProducts();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("toggle-status/{id}")]
        public async Task<IActionResult> ToggleProductStatus(int id)
        {
            var result = await _productService.ToggleProductStatusAsync(id);

            if (!result.Success)
                return NotFound(new { message = result.Message });

            return Ok(new { message = result.Message, status = result.IsActive });
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            // 1️⃣ Upload images and populate dto.ImageUrls
            if (dto.Images != null && dto.Images.Any())
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                foreach (var file in dto.Images)
                {
                    var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var fileUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
                    dto.ImageUrls.Add(fileUrl); // populate urls for service
                }
            }

            // 2️⃣ Call service to create product
            var created = await _productService.CreateProductAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }



        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductCreateDto dto)
        {
            var existing = await _productService.GetProductByIdAsync(id);
            if (existing == null) return NotFound();

            var uploadPath = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // If new images provided -> upload them and populate dto.ImageUrls
            if (dto.Images != null && dto.Images.Any())
            {
                // 1) Delete old files from disk (if any)
                if (existing.ImageUrls != null && existing.ImageUrls.Any())
                {
                    foreach (var oldImageUrl in existing.ImageUrls)
                    {
                        var fileName = Path.GetFileName(oldImageUrl);
                        var oldImagePath = Path.Combine(uploadPath, fileName);

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            try { System.IO.File.Delete(oldImagePath); } catch { /* log if needed */ }
                        }
                    }
                }

                // 2) Save new files to disk and populate dto.ImageUrls
                dto.ImageUrls = new List<string>(); // reset to ensure fresh list
                foreach (var file in dto.Images)
                {
                    // sanitize file name and create unique file name
                    var sanitized = Path.GetFileName(file.FileName);
                    var fileName = $"{Guid.NewGuid()}_{sanitized}";
                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
                    dto.ImageUrls.Add(fileUrl);
                }
            }

            // If no new images provided, keep existing ImageUrls (service will not replace images)
            else
            {
                // Ensure dto.ImageUrls is null or empty so service won't replace existing images.
                dto.ImageUrls = new List<string>();
            }

            // Proceed with DB update
            var success = await _productService.UpdateProductAsync(id, dto);
            return success ? NoContent() : NotFound();
        }



        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            return success ? NoContent() : NotFound();
        }


        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadPath = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
            return Ok(new { imageUrl = fileUrl });
        }
    }
}
