using System.Net.Http.Headers;

namespace CBoosterSharp.Network.ApiService;

public class ApiServiceBuilder
{
    private string _baseUrl = string.Empty;
    private double _timeOut = 30;
    private bool _withLogger = false;
    private Action<string>? _loggerHandler = null;
    private HttpClient? _httpClient;

    public ApiServiceBuilder BaseUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
        return this;
    }

    public ApiServiceBuilder TimeOut(double timeOut)
    {
        _timeOut = timeOut;
        return this;
    }

    public ApiServiceBuilder WithLogger(Action<string>? loggerHandler = null)
    {
        _withLogger = true;
        _loggerHandler = loggerHandler;
        return this;
    }

    public ApiServiceBuilder SetClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        return this;
    }

    public ApiService Build()
    {
        if (string.IsNullOrEmpty(_baseUrl))
        {
            throw new InvalidOperationException(
                "BaseUrl must be set before building the ApiService."
            );
        }

        if (_httpClient == null)
        {
            _httpClient = new HttpClient(
                _withLogger
                    ? new ApiServiceLogger(onPrint: _loggerHandler)
                    {
                        InnerHandler = new HttpClientHandler(),
                    }
                    : new HttpClientHandler()
            )
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(_timeOut),
            };

            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }
        return new ApiService(_httpClient, _baseUrl);
    }
}
