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
        public async Task<ActionResult<Product>> AddProduct([FromForm] Product product, IFormFile image)
        {
            try
            {
                _logger.LogInformation("Adding a new product.");
                _logger.LogInformation($"Product name: {product.Name}");
                _logger.LogInformation($"Product description: {product.Description}");
                _logger.LogInformation($"Product price: {product.Price}");
                _logger.LogInformation($"Product stock: {product.Stock}");

                if (image != null && image.Length > 0)
                {
                    _logger.LogInformation($"Received image with length: {image.Length}");
                    var imagePath = Path.Combine("wwwroot/images", image.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        _logger.LogInformation("Starting to copy image to file stream.");
                        await image.CopyToAsync(stream);
                        _logger.LogInformation("Finished copying image to file stream.");
                    }
                    product.ImageUrl = $"/images/{image.FileName}";
                }

                if (string.IsNullOrEmpty(product.ImageUrl))
                {
                    _logger.LogError("The ImageUrl field is required.");
                    return BadRequest(new { errors = new { ImageUrl = new[] { "The ImageUrl field is required." } } });
                }

                var newProduct = await _productService.AddProduct(product);
                return Ok(newProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while adding product: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding product");
            }
        }

    }
}
