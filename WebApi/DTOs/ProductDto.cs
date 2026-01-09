namespace WebApi.DTOs
{
    /// <summary>
    /// DTO para mostrar información de un producto
    /// </summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; }
        public DateTime FechaIngreso { get; set; }
    }

    /// <summary>
    /// DTO para crear un nuevo producto
    /// </summary>
    public class CreateProductDto
    {
        /// <summary>
        /// Nombre del producto (requerido)
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del producto
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Precio del producto (debe ser mayor a 0)
        /// </summary>
        public double Price { get; set; }
    }

    /// <summary>
    /// DTO para actualizar un producto existente
    /// </summary>
    public class UpdateProductDto
    {
        /// <summary>
        /// Nombre del producto
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del producto
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Precio del producto
        /// </summary>
        public double Price { get; set; }
    }
}
