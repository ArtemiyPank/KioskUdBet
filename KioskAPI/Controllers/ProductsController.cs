using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KioskAPI.Models;
using KioskAPI.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet("getProducts")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            _logger.LogInformation("Getting all products.");
            var products = await _productService.GetProducts();
            return Ok(products);
        }

        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProduct([FromForm] Product product, IFormFile image)
        {
            try
            {
                _logger.LogInformation("Adding a new product.");
                _logger.LogInformation($"Product name: {product.Name}");
                _logger.LogInformation($"Product description: {product.Description}");
                _logger.LogInformation($"Product price: {product.Price}");
                _logger.LogInformation($"Product stock: {product.Stock}");
                _logger.LogInformation($"Product category: {product.Category}");
                _logger.LogInformation($"Product last updated: {product.LastUpdated}");

                // Сохраняем продукт сначала, чтобы получить его ID
                var newProduct = await _productService.AddProduct(product);

                if (image != null && image.Length > 0)
                {
                    _logger.LogInformation($"Received image with length: {image.Length}");

                    // Генерируем новое имя файла
                    var newFileName = $"{newProduct.Name}_{newProduct.Category}_{newProduct.Id}.jpg";
                    var imagePath = Path.Combine("wwwroot/images", newFileName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        _logger.LogInformation("Starting to copy image to file stream.");
                        await image.CopyToAsync(stream);
                        _logger.LogInformation("Finished copying image to file stream.");
                    }
                    newProduct.ImageUrl = $"/images/{newFileName}";

                    // Обновляем продукт с новым URL изображения
                    await _productService.UpdateProduct(newProduct.Id, newProduct);
                }

                return Ok(newProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while adding product: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding product");
            }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Product product, IFormFile image)
        {
            product.LastUpdated = DateTime.UtcNow; // Устанавливаем дату последнего обновления

            if (image != null && image.Length > 0)
            {
                var imagePath = Path.Combine("wwwroot/images", image.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                product.ImageUrl = $"/images/{image.FileName}";
            }

            var updatedProduct = await _productService.UpdateProduct(id, product);
            return Ok(updatedProduct);
        }



    }
}
