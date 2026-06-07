namespace CBoosterSharp.Network.ApiService;

public abstract class Resource<T>(T? data, Exception? exception)
{
    public T? Data { get; } = data;
    public Exception? Exception { get; } = exception;

    public sealed class Empty : Resource<T>
    {
        public Empty() : base(default, null) { }
    }

    public sealed class Loading(T? data = default) : Resource<T>(data, null) { }

    public sealed class Success(T data) : Resource<T>(data, null) { }

    public sealed class Error(T? data = default, Exception? exception = null)
        : Resource<T>(data, exception)
    { }


    public override string ToString()
    {
        var typeName = GetType().Name;

        string dataStr = Data != null ? Data.ToString() ?? "null" : "null";

        string exceptionStr = Exception != null ? Exception.Message : "null";

        return $"{typeName}(Data: {dataStr}, Exception: {exceptionStr})";
    }
}
