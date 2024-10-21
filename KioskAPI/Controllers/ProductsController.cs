using KioskAPI.Controllers;
using KioskAPI.Models;
using KioskAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



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
            Console.WriteLine("Request received: Get all products.");
            var products = await _productService.GetProducts();
            Console.WriteLine($"Returning {products.Count} products.");
            return Ok(products);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            Console.WriteLine($"Request received: Get product by ID {id}.");
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                Console.WriteLine($"Product with ID {id} not found.");
                return NotFound();
            }
            Console.WriteLine($"Returning product: {product.Name}");
            return product;
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProduct([FromForm] Product product, IFormFile image)
        {
            try
            {
                Console.WriteLine($"Request received: Add product {product.Name}");
                var newProduct = await _productService.AddProduct(product);
                Console.WriteLine($"Product added with ID {newProduct.Id}");

                if (image != null && image.Length > 0)
                {
                    Console.WriteLine($"Saving image for product {newProduct.Id}");
                    var newFileName = $"product_{newProduct.Id}.jpg";
                    var imagePath = Path.Combine("wwwroot/images", newFileName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    newProduct.ImageUrl = $"/images/{newFileName}";
                    await _productService.UpdateProduct(newProduct.Id, newProduct);
                }

                return Ok(newProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding product: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding product");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("updateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Product product, IFormFile image = null)
        {
            try
            {
                Console.WriteLine($"Request received: Update product {id}");
                if (id != product.Id) return BadRequest("Product ID mismatch");

                var existingProduct = await _productService.GetProductByIdAsync(id);
                if (existingProduct == null)
                {
                    Console.WriteLine($"Product with ID {id} not found.");
                    return NotFound("Product not found");
                }

                // Обновляем свойства продукта
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.Stock = product.Stock;
                existingProduct.ReservedStock = product.ReservedStock;
                existingProduct.Category = product.Category;
                existingProduct.LastUpdated = DateTime.UtcNow;

                await _productService.UpdateProduct(id, existingProduct);

                if (image != null && image.Length > 0)
                {
                    Console.WriteLine($"Updating image for product {id}");
                    var newFileName = $"product_{existingProduct.Id}.jpg";
                    var imagePath = Path.Combine("wwwroot/images", newFileName);

                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    existingProduct.ImageUrl = $"/images/{newFileName}";
                    await _productService.UpdateProduct(existingProduct.Id, existingProduct);
                }

                return Ok(existingProduct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating product");
            }
        }

        // API для резервирования товара
        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveProductStock([FromBody] QuantityRequest request)
        {
            try
            {
                Console.WriteLine(request.ToString());
                Console.WriteLine($"Request received: Reserve product {request.ProductId} with quantity {request.Quantity}");

                // Резервируем товар
                await _productService.ReserveProductStockAsync(request.ProductId, request.Quantity);

                // Получаем текущее количество доступного и зарезервированного товара
                var stock = await _productService.GetStock(request.ProductId);
                var reservedStock = await _productService.GetReservedStock(request.ProductId);

                // Уведомляем наблюдателей об изменении
                SseController.NotifyProductQuantityChanged(request.ProductId, stock, reservedStock);

                Console.WriteLine($"Reserved product {request.ProductId}. Stock: {stock}, Reserved stock: {reservedStock}");

                return Ok(new { stock = stock, ReservedStock = reservedStock });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reserving product: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
        }


        // API для освобождения товара
        [HttpPost("release")]
        public async Task<IActionResult> ReleaseProductStock([FromBody] QuantityRequest request)
        {
            try
            {
                Console.WriteLine($"Request received: Release product {request.ProductId} with quantity {request.Quantity}");

                // Освобождаем зарезервированное количество товара
                await _productService.ReleaseProductStockAsync(request.ProductId, request.Quantity);
                
                // Получаем текущее количество доступного и зарезервированного товара
                var stock = await _productService.GetStock(request.ProductId);

                var reservedStock = await _productService.GetReservedStock(request.ProductId);

                // Уведомляем наблюдателей об изменении
                SseController.NotifyProductQuantityChanged(request.ProductId, stock, reservedStock);

                Console.WriteLine($"Released product {request.ProductId}. Stock: {stock}, Reserved stock: {reservedStock}");

                return Ok(new { Stock = stock, ReservedStock = reservedStock });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error releasing product: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
        }


        [Authorize(Roles = "Admin")]
        [HttpPut("toggleVisibility/{id}")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            Console.WriteLine($"Request received: Toggle visibility for product {id}");
            var result = await _productService.ToggleVisibility(id);
            return result ? NoContent() : NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            Console.WriteLine($"Request received: Delete product {id}");
            var result = await _productService.DeleteProductAsync(id);
            return result ? NoContent() : NotFound();
        }
    }

}