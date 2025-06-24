using System;

namespace Order_API.Common.Helpers
{
    public class ApiResponse<T>
    {
        public int Status { get; set; }
        public string Key { get; set; }
        public T? Data { get; set; }
        public long Timestamp { get; set; }

        public ApiResponse(int status, string? key = null, T? data = default)
        {
            Status = status;
            Key = key;
            Data = data;
            Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        }
    }
} 