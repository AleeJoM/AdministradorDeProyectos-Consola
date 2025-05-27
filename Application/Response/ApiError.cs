namespace Application.Response
{
    public class ApiError
    {
        public string Message { get; set; }
        public List<string>? Errors { get; set; }
        public string? ErrorCode { get; set; }
    }
}
