using System.Net;

namespace CBoosterSharp.Network.ApiService;

public class ApiException : Exception
{
    public HttpStatusCode? StatusCode { get; set; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; set; }

    public ApiException(
        string message,
        Exception? innerException,
        HttpStatusCode? statusCode = null
    ) : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public ApiException(
        string message,
        IReadOnlyDictionary<string, string[]>? Errors = null,
        HttpStatusCode? statusCode = null,
        Exception? innerException = null
    ) : base(message, innerException)
    {
        StatusCode = statusCode;
        this.Errors = Errors;
    }

    public ApiException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public ApiException(string message) : base(message) { }

    public ApiException(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }
}
