namespace MarketPlace.BLL.Exceptions;

public class InsufficientStockException(string message) : Exception(message);