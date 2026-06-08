namespace HistoricalMuseumAudioGuide.Service.Services
{
    public class ResponseModel
    {
        public ResponseModel()
        {
        }

        private ResponseModel(int statusCode, string status, string message, object? data)
        {
            StatusCode = statusCode;
            Status = status;
            Message = message;
            Data = data;
        }

        public int Id { get; set; } = 1;
        public int StatusCode { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }

        public static ResponseModel Success(string message, object? data = null)
        {
            return new ResponseModel(200, "Success", message, data);
        }

        public static ResponseModel Error(string message, object? data = null)
        {
            return new ResponseModel(500, "Error", message, data);
        }

        public static ResponseModel NotFound(string message, object? data = null)
        {
            return new ResponseModel(404, "Not Found", message, data);
        }

        public static ResponseModel BadRequest(string message, object? data = null)
        {
            return new ResponseModel(400, "Bad Request", message, data);
        }

        public static ResponseModel Unauthorized(string message, object? data = null)
        {
            return new ResponseModel(401, "Unauthorized", message, data);
        }

        public static ResponseModel Forbidden(string message, object? data = null)
        {
            return new ResponseModel(403, "Forbidden", message, data);
        }

        public static ResponseModel Conflict(string message, object? data = null)
        {
            return new ResponseModel(409, "Conflict", message, data);
        }
    }
}
