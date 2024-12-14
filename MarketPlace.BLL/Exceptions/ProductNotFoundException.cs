namespace MarketPlace.BLL.Exceptions;

public class ProductNotFoundException(string message) : Exception(message);