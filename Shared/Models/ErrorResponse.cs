namespace Shared.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
