using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using KioskAPI.Models;
using KioskAPI.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ITokenService _tokenService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ITokenService tokenService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpGet("getProducts")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            _logger.LogInformation("Getting all products.");
            var products = await _productService.GetProducts();
            return Ok(products);
        }


        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProduct([FromForm] Product product, IFormFile image)
        {
            try
            {
                _logger.LogInformation("Adding a new product.");
                _logger.LogInformation($"Product ID: {product.Id} \n" +
                    $"Product name: {product.Name} \n" +
                    $"Product description: {product.Description} \n" +
                    $"Product price: {product.Price} \n" +
                    $"Product stock: {product.Stock} \n" +
                    $"Product category: {product.Category} \n" +
                    $"Product last updated: {product.LastUpdated} \n");

                // Сохраняем продукт сначала, чтобы получить его ID
                var newProduct = await _productService.AddProduct(product);

                if (image != null && image.Length > 0)
                {
                    _logger.LogInformation($"Received image with length: {image.Length}");

                    // Генерируем новое имя файла
                    var newFileName = $"product_{newProduct.Id}.jpg";
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


        [Authorize(Roles = "Admin")]
        [HttpPut("updateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Product product, IFormFile image = null)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest("Product ID mismatch");
                }

                _logger.LogInformation("Updating product.");
                _logger.LogInformation($"Product ID: {product.Id} \n" +
                    $"Product name: {product.Name} \n" +
                    $"Product description: {product.Description} \n" +
                    $"Product price: {product.Price} \n" +
                    $"Product stock: {product.Stock} \n" +
                    $"Product category: {product.Category} \n" +
                    $"Product last updated: {product.LastUpdated} \n");


                // Получаем существующий продукт без отслеживания
                var existingProduct = await _productService.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound("Product not found");
                }

                // Копируем обновляемые свойства в существующий продукт
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.Category = product.Category;
                existingProduct.LastUpdated = product.LastUpdated;

                // Обновляем продукт
                await _productService.UpdateProduct(id, existingProduct);

                if (image != null && image.Length > 0)
                {
                    _logger.LogInformation($"Received image with length: {image.Length}");

                    // Генерируем новое имя файла
                    var newFileName = $"product_{existingProduct.Id}.jpg";
                    var imagePath = Path.Combine("wwwroot/images", newFileName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        _logger.LogInformation("Starting to copy image to file stream.");
                        await image.CopyToAsync(stream);
                        _logger.LogInformation("Finished copying image to file stream.");
                    }
                    existingProduct.ImageUrl = $"/images/{newFileName}";

                    // Обновляем продукт с новым URL изображения
                    await _productService.UpdateProduct(existingProduct.Id, existingProduct);
                }
                else
                {
                    _logger.LogInformation("No image provided for the update.");
                }

                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating product: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating product");
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("toggleVisibility/{id}")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            _logger.LogInformation($"is - {id}");
            var result = await _productService.ToggleVisibility(id);
            if (result)
            {
                return NoContent();
            }
            return NotFound();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation($"is - {id}");
            var result = await _productService.DeleteProductAsync(id);
            if (result)
            {
                return NoContent();
            }
            return NotFound();
        }

    }
}
