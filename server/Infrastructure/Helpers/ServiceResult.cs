namespace DatingApp.Helpers
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public int StatusCode { get; set; } = 200;

        public static ServiceResult Ok(object? data = null, string message = "")
        {
            return new ServiceResult { Success = true, Data = data, Message = message, StatusCode = 200 };
        }

        public static ServiceResult BadRequest(string message)
        {
            return new ServiceResult { Success = false, Message = message, StatusCode = 400 };
        }

        public static ServiceResult Unauthorized(string message)
        {
            return new ServiceResult { Success = false, Message = message, StatusCode = 401 };
        }

        public static ServiceResult NotFound(string message)
        {
            return new ServiceResult { Success = false, Message = message, StatusCode = 404 };
        }

        public static ServiceResult Error(string message, int statusCode = 500)
        {
            return new ServiceResult { Success = false, Message = message, StatusCode = statusCode };
        }
    }

    public class ServiceResult<T> : ServiceResult
    {
        public new T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string message = "")
        {
            return new ServiceResult<T> { Success = true, Data = data, Message = message, StatusCode = 200 };
        }

        public static new ServiceResult<T> BadRequest(string message)
        {
            return new ServiceResult<T> { Success = false, Message = message, StatusCode = 400 };
        }
        
        public static new ServiceResult<T> Unauthorized(string message)
        {
            return new ServiceResult<T> { Success = false, Message = message, StatusCode = 401 };
        }
        
        public static new ServiceResult<T> NotFound(string message)
        {
            return new ServiceResult<T> { Success = false, Message = message, StatusCode = 404 };
        }

        public static new ServiceResult<T> Error(string message, int statusCode = 500)
        {
            return new ServiceResult<T> { Success = false, Message = message, StatusCode = statusCode };
        }
    }
}
