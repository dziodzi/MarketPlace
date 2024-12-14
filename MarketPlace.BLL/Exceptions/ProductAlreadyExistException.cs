namespace MarketPlace.BLL.Exceptions;

public class ProductAlreadyExistException(string message) : Exception(message);