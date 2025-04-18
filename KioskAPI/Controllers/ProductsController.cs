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

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: api/products/getProducts
        [HttpGet("getProducts")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            _logger.LogInformation("Fetching all products");
            var products = await _productService.GetProductsAsync();
            _logger.LogInformation("Returning {Count} products", products.Count);
            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            _logger.LogInformation("Fetching product with ID {ProductId}", id);
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", id);
                return NotFound();
            }
            _logger.LogInformation("Returning product {ProductName}", product.Name);
            return Ok(product);
        }

        // POST: api/products/addProduct
        [Authorize(Roles = "Admin")]
        [HttpPost("addProduct")]
        public async Task<IActionResult> AddProduct([FromForm] Product product, IFormFile image)
        {
            _logger.LogInformation("Adding new product {ProductName}", product.Name);

            try
            {
                var newProduct = await _productService.AddProductAsync(product);
                _logger.LogInformation("Product created with ID {ProductId}", newProduct.Id);

                if (image != null && image.Length > 0)
                {
                    // Save uploaded image to wwwroot/images
                    var fileName = $"product_{newProduct.Id}.jpg";
                    var imagePath = Path.Combine("wwwroot", "images", fileName);

                    using var stream = new FileStream(imagePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    newProduct.ImageUrl = $"/images/{fileName}";
                    await _productService.UpdateProductAsync(newProduct.Id, newProduct);
                    _logger.LogInformation("Image saved for product {ProductId}", newProduct.Id);
                }

                return Ok(newProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding product {ProductName}", product.Name);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding product");
            }
        }

        // PUT: api/products/updateProduct/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("updateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] Product product, IFormFile? image = null)
        {
            _logger.LogInformation("Updating product {ProductId}", id);

            if (id != product.Id)
            {
                _logger.LogWarning("Product ID mismatch: route ID {RouteId} vs body ID {BodyId}", id, product.Id);
                return BadRequest("Product ID mismatch");
            }

            var existing = await _productService.GetProductByIdAsync(id);
            if (existing == null)
            {
                _logger.LogWarning("Product {ProductId} not found for update", id);
                return NotFound("Product not found");
            }

            // Update product fields
            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
            existing.ReservedStock = product.ReservedStock;
            existing.Category = product.Category;
            existing.LastUpdated = DateTime.UtcNow;

            await _productService.UpdateProductAsync(id, existing);
            _logger.LogInformation("Core data updated for product {ProductId}", id);

            if (image != null && image.Length > 0)
            {
                // Replace product image file
                var fileName = $"product_{id}.jpg";
                var imagePath = Path.Combine("wwwroot", "images", fileName);

                using var stream = new FileStream(imagePath, FileMode.Create);
                await image.CopyToAsync(stream);

                existing.ImageUrl = $"/images/{fileName}";
                await _productService.UpdateProductAsync(id, existing);
                _logger.LogInformation("Image updated for product {ProductId}", id);
            }

            return Ok(existing);
        }

        // POST: api/products/reserve
        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveProductStock([FromBody] QuantityRequest request)
        {
            _logger.LogInformation(
                "Reserving quantity {Quantity} for product {ProductId}",
                request.Quantity, request.ProductId);

            try
            {
                await _productService.ReserveProductStockAsync(request.ProductId, request.Quantity);

                var stock = await _productService.GetStockAsync(request.ProductId);
                var reserved = await _productService.GetReservedStockAsync(request.ProductId);

                // Notify via SSE
                SseController.NotifyProductQuantityChanged(request.ProductId, stock, reserved);

                _logger.LogInformation(
                    "Reserved product {ProductId}: Stock={Stock}, Reserved={Reserved}",
                    request.ProductId, stock, reserved);

                return Ok(new { stock, reserved });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving stock for product {ProductId}", request.ProductId);
                return BadRequest(new { Message = ex.Message });
            }
        }

        // POST: api/products/release
        [HttpPost("release")]
        public async Task<IActionResult> ReleaseProductStock([FromBody] QuantityRequest request)
        {
            _logger.LogInformation(
                "Releasing reserved quantity {Quantity} for product {ProductId}",
                request.Quantity, request.ProductId);

            try
            {
                await _productService.ReleaseProductStockAsync(request.ProductId, request.Quantity);

                var stock = await _productService.GetStockAsync(request.ProductId);
                var reserved = await _productService.GetReservedStockAsync(request.ProductId);

                // Notify via SSE
                SseController.NotifyProductQuantityChanged(request.ProductId, stock, reserved);

                _logger.LogInformation(
                    "Released product {ProductId}: Stock={Stock}, Reserved={Reserved}",
                    request.ProductId, stock, reserved);

                return Ok(new { stock, reserved });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing stock for product {ProductId}", request.ProductId);
                return BadRequest(new { Message = ex.Message });
            }
        }

        // PUT: api/products/toggleVisibility/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("toggleVisibility/{id}")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            _logger.LogInformation("Toggling visibility for product {ProductId}", id);
            var success = await _productService.ToggleVisibilityAsync(id);
            return success ? NoContent() : NotFound();
        }

        // DELETE: api/products/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            _logger.LogInformation("Deleting product {ProductId}", id);
            var success = await _productService.DeleteProductAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
