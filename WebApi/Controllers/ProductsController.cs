using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    /// <summary>
    /// Controlador para gestión de productos
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene lista de productos con paginación, filtrado por nombre y ordenamiento por precio
        /// </summary>
        /// <param name="parameters">Parámetros de paginación y filtrado</param>
        /// <returns>Lista paginada de productos</returns>
        /// <response code="200">Retorna la lista de productos</response>
        /// <response code="401">No autorizado - Token JWT inválido o expirado</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PaginatedResponse<ProductDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<ProductDto>>>> GetProducts(
            [FromQuery] PaginationParams parameters)
        {
            var query = _context.Products.AsQueryable();

            // Filtro por nombre
            if (!string.IsNullOrWhiteSpace(parameters.Name))
            {
                query = query.Where(p => p.Name.Contains(parameters.Name));
            }

            // Ordenamiento por precio
            if (!string.IsNullOrWhiteSpace(parameters.SortByPrice))
            {
                query = parameters.SortByPrice.ToLower() == "desc"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price);
            }
            else
            {
                query = query.OrderBy(p => p.Id);
            }

            // Conteo total
            var totalCount = await query.CountAsync();

            // Paginación
            var products = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    FechaIngreso = p.FechaIng
                })
                .ToListAsync();

            var paginatedResponse = new PaginatedResponse<ProductDto>
            {
                Data = products,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize,
                TotalCount = totalCount
            };

            return Ok(ApiResponse<PaginatedResponse<ProductDto>>.Ok(paginatedResponse, "Productos obtenidos exitosamente"));
        }

        /// <summary>
        /// Obtiene un producto por su ID
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Producto encontrado</returns>
        /// <response code="200">Retorna el producto</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(ApiResponse<ProductDto>.Fail("Producto no encontrado"));
            }

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                FechaIngreso = product.FechaIng
            };

            return Ok(ApiResponse<ProductDto>.Ok(productDto));
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        /// <param name="createDto">Datos del producto a crear</param>
        /// <returns>Producto creado</returns>
        /// <response code="201">Producto creado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Acceso denegado - Solo administradores</response>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto createDto)
        {
            if (string.IsNullOrWhiteSpace(createDto.Name))
            {
                return BadRequest(ApiResponse<ProductDto>.Fail("El nombre del producto es requerido"));
            }

            if (createDto.Price <= 0)
            {
                return BadRequest(ApiResponse<ProductDto>.Fail("El precio debe ser mayor a 0"));
            }

            var product = new Products
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Price = createDto.Price,
                FechaIng = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                FechaIngreso = product.FechaIng
            };

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                ApiResponse<ProductDto>.Ok(productDto, "Producto creado exitosamente"));
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        /// <param name="id">ID del producto a actualizar</param>
        /// <param name="updateDto">Datos actualizados del producto</param>
        /// <returns>Sin contenido</returns>
        /// <response code="200">Producto actualizado exitosamente</response>
        /// <response code="400">Datos inválidos</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Acceso denegado - Solo administradores</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto updateDto)
        {
            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound(ApiResponse<ProductDto>.Fail("Producto no encontrado"));
            }

            if (string.IsNullOrWhiteSpace(updateDto.Name))
            {
                return BadRequest(ApiResponse<ProductDto>.Fail("El nombre del producto es requerido"));
            }

            existingProduct.Name = updateDto.Name;
            existingProduct.Description = updateDto.Description;
            existingProduct.Price = updateDto.Price;

            await _context.SaveChangesAsync();

            var productDto = new ProductDto
            {
                Id = existingProduct.Id,
                Name = existingProduct.Name,
                Description = existingProduct.Description,
                Price = existingProduct.Price,
                FechaIngreso = existingProduct.FechaIng
            };

            return Ok(ApiResponse<ProductDto>.Ok(productDto, "Producto actualizado exitosamente"));
        }

        /// <summary>
        /// Elimina un producto
        /// </summary>
        /// <param name="id">ID del producto a eliminar</param>
        /// <returns>Sin contenido</returns>
        /// <response code="200">Producto eliminado exitosamente</response>
        /// <response code="404">Producto no encontrado</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Acceso denegado - Solo administradores</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(ApiResponse<object>.Fail("Producto no encontrado"));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(new { DeletedId = id }, "Producto eliminado exitosamente"));
        }
    }
}
