namespace MarketPlace.DAL.Models.Responses;

public class ApiResponse<T>
{
    public ResponseCode Code { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }

    public ApiResponse(ResponseCode code, string message, T data = default)
    {
        Code = code;
        Message = message;
        Data = data;
    }
}