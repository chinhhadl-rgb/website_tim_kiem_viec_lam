namespace RecruitmentSystem.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public string[]? Errors { get; set; }
        public string Timestamp { get; set; }

        public ApiResponse()
        {
            Message = string.Empty;
            Timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}
