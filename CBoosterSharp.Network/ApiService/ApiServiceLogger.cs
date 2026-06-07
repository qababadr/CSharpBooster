using System.Diagnostics;
using System.Text.Json;

namespace CBoosterSharp.Network.ApiService;

public class ApiServiceLogger(Action<string>? onPrint = null) : DelegatingHandler
{
    private readonly Action<string>? _onPrint = onPrint;

    private readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Log("\n- - - - - - - - - - API LOG - - - - - - - - - -\n");

        await LogRequestAsync(request);

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            Log($"\nEXCEPTION: {ex.Message}");
            Log("\n- - - - - - - - - - END - - - - - - - - - -\n");
            throw;
        }

        await LogResponseAsync(response);

        Log("\n- - - - - - - - - - END - - - - - - - - - -\n");
        return response;
    }

    private async Task LogResponseAsync(HttpResponseMessage? response)
    {
        Log("\nRESPONSE:");

        if (response == null)
        {
            Log("Response: null");
            return;
        }

        Log($"Status Code: {(int)response.StatusCode} {response.ReasonPhrase}");
        Log($"Is Success: {response.IsSuccessStatusCode}");

        if (response.Headers != null)
        {
            Log("Headers:");
            foreach (var header in response.Headers)
            {
                Log($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        if (response.Content == null)
        {
            Log("Response Body: <null>");
            return;
        }

        string responseContent = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            Log("Response Body: <empty>");
            return;
        }

        try
        {
            using var json = JsonDocument.Parse(responseContent);

            string prettyJson = JsonSerializer.Serialize(
                json.RootElement,
                _options
            );

            Log("Response Body (JSON):\n" + prettyJson);
        }
        catch (JsonException)
        {
            Log("Response Body (raw):\n" + responseContent);
        }
    }

    private async Task LogRequestAsync(HttpRequestMessage request)
    {
        Log("REQUEST:");
        Log($"URL: {request.RequestUri}");
        Log($"HTTP Method: {request.Method}");

        if (request.Headers != null)
        {
            Log("Headers:");
            foreach (var header in request.Headers)
            {
                Log($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        if (request.Content != null)
        {
            string requestContent = await request.Content.ReadAsStringAsync();
            Log("Body:");
            Log(string.IsNullOrWhiteSpace(requestContent) ? "<empty>" : requestContent);
        }
    }

    private void Log(string message)
    {
        if (_onPrint != null)
        {
            _onPrint(message);
        }
        else
        {
            Debug.WriteLine(message);
        }
    }
}
