﻿namespace MarketPlace.BLL.Exceptions;

public class InsufficientFundsException(string message) : Exception(message);