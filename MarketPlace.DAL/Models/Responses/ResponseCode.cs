namespace MarketPlace.DAL.Models.Responses;

public enum ResponseCode
{
    Success = 200,
    NotFound = 404,
    BadRequest = 400,
    Conflict = 409,
    InternalError = 500
}