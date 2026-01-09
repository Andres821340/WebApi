namespace WebApi.DTOs
{
    /// <summary>
    /// Respuesta paginada genérica para listados
    /// </summary>
    /// <typeparam name="T">Tipo de datos en la lista</typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Lista de elementos de la página actual
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Número de página actual
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Cantidad de elementos por página
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total de elementos en la base de datos
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total de páginas disponibles
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        /// <summary>
        /// Indica si existe una página anterior
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Indica si existe una página siguiente
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Parámetros de paginación y filtrado para consultas
    /// </summary>
    public class PaginationParams
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        /// <summary>
        /// Número de página (por defecto 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Cantidad de elementos por página (máximo 50)
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        /// <summary>
        /// Filtro por nombre del producto
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Ordenar por precio: 'asc' o 'desc'
        /// </summary>
        public string? SortByPrice { get; set; }
    }

    /// <summary>
    /// Respuesta estándar de la API
    /// </summary>
    /// <typeparam name="T">Tipo de datos</typeparam>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Operación exitosa")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
