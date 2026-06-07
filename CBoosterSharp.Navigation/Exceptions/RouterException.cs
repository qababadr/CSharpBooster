namespace CBoosterSharp.Navigation.Exceptions;

public class RouterException : Exception
{
    public RouterException()
    {
    }

    public RouterException(string message)
        : base(message)
    {
    }

    public RouterException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
