namespace Shared.DTOs
{
    /// <summary>Contains a failed API operation and optional validation details.</summary>
    public class ApiErrorResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IEnumerable<string>? Errors { get; set; }
        public string? TraceId { get; set; }

        /// <summary>Creates a consistent error envelope with optional validation and trace details.</summary>
        public static ApiErrorResponse Create(string message, IEnumerable<string>? errors = null, string? traceId = null)
            => new ApiErrorResponse
            {
                Success = false,
                Message = message,
                Errors = errors,
                TraceId = traceId
            };
    }

    /// <summary>Wraps the data returned by a successful API operation.</summary>
    public class ApiSuccessResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        /// <summary>Creates a success envelope containing the public response DTO.</summary>
        public static ApiSuccessResponse<T> Create(T data, string message = "Operation completed successfully")
            => new ApiSuccessResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
    }
}
