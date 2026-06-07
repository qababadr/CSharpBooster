using System.Net.Http.Json;
using System.Text.Json;

namespace CBoosterSharp.Network.ApiService;

public class ApiService(HttpClient httpClient, string? baseUrl = null)
{
    private readonly string? _baseUrl = baseUrl;
    private readonly JsonSerializerOptions jsonSerializerOptions
        = new(JsonSerializerDefaults.Web);

    public async Task<ApiResponse<T>> Get<T>(
        string url,
        CancellationToken? cancellationToken = default,
        Dictionary<string, string>? headers = null
    )
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(url));

            request.Headers.Add("Accept", "application/json");

            if (headers != null)
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);

            var response = cancellationToken == null
                ? await httpClient.SendAsync(request).ConfigureAwait(false)
                : await httpClient
                .SendAsync(request, (CancellationToken)cancellationToken).ConfigureAwait(false);

            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            // Deserialize the API response
            var apiResponse = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(
                responseStream,
                jsonSerializerOptions,
                cancellationToken ?? CancellationToken.None
            ).ConfigureAwait(false)
            ?? throw new ApiException("Error reading the stream", new JsonException());

            // Check HTTP status
            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = apiResponse?.Message ?? "Unknown error";
                IReadOnlyDictionary<string, string[]>? errors = null;

                if (apiResponse?.Errors != null)
                    errors = apiResponse.Errors;

                throw new ApiException(errorMsg, errors, response.StatusCode);
            }

            return apiResponse ?? throw new ApiException("Empty response data");
        }
        catch (Exception ex)
        {
            throw HandleException(ex, cancellationToken);
        }
    }

    public async Task<T> RawGet<T>(
        string url,
        Dictionary<string, string>? headers = null,
        CancellationToken? cancellationToken = default
    )
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl(url));
            request.Headers.Add("Accept", "application/json");

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            HttpResponseMessage response;

            if (cancellationToken == null)
            {
                response = await httpClient.SendAsync(request).ConfigureAwait(false);
            }
            else
            {
                response = await httpClient
                    .SendAsync(request, (CancellationToken)cancellationToken)
                    .ConfigureAwait(false);
            }

            using Stream? responseStream = await response
                .Content.ReadAsStreamAsync()
                .ConfigureAwait(false);

            T? apiResponse;

            response.EnsureSuccessStatusCode();

            if (cancellationToken == null)
            {
                apiResponse = await JsonSerializer
                    .DeserializeAsync<T>(responseStream, jsonSerializerOptions)
                    .ConfigureAwait(false);
            }
            else
            {
                apiResponse = await JsonSerializer
                    .DeserializeAsync<T>(
                        responseStream,
                        jsonSerializerOptions,
                        cancellationToken: (CancellationToken)cancellationToken
                    )
                    .ConfigureAwait(false);
            }

            return apiResponse ?? throw new ApiException(
                "Error reading the stream",
                new JsonException()
            );
        }

        catch (Exception ex)
        {
            throw HandleException(ex, cancellationToken);
        }
    }

    public async Task<ApiResponse<T>> Post<T>(
        string url,
        object? data = null,
        CancellationToken? cancellationToken = default,
        Dictionary<string, string>? headers = null
    )
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(url))
            {
                Content = JsonContent.Create(data)
            };

            request.Headers.Add("Accept", "application/json");

            if (headers != null)
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);

            var response = cancellationToken == null
                ? await httpClient.SendAsync(request).ConfigureAwait(false)
                : await httpClient
                .SendAsync(request, (CancellationToken)cancellationToken).ConfigureAwait(false);

            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            // Deserialize the API response
            var apiResponse = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(
                responseStream,
                jsonSerializerOptions,
                cancellationToken ?? CancellationToken.None
            ).ConfigureAwait(false)
            ?? throw new ApiException("Error reading the stream", new JsonException());

            // Check HTTP status
            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = apiResponse?.Message ?? "Unknown error";
                IReadOnlyDictionary<string, string[]>? errors = null;

                if (apiResponse?.Errors != null)
                    errors = apiResponse.Errors;

                throw new ApiException(errorMsg, errors, response.StatusCode);
            }

            return apiResponse ?? throw new ApiException("Empty response data");
        }
        catch (Exception ex)
        {
            throw HandleException(ex, cancellationToken);
        }
    }

    public async Task<T> RawPost<T>(
        string url,
        object? data = null,
        CancellationToken? cancellationToken = default,
        Dictionary<string, string>? headers = null
    )
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(url))
            {
                Content = JsonContent.Create(data)
            };
            request.Headers.Add("Accept", "application/json");

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            var response = cancellationToken == null
                ? await httpClient.SendAsync(request).ConfigureAwait(false)
                : await httpClient.SendAsync(request, (CancellationToken)cancellationToken)
                .ConfigureAwait(false);

            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var apiResponse = cancellationToken == null
                ? await JsonSerializer.DeserializeAsync<T>(responseStream, jsonSerializerOptions).ConfigureAwait(false)
                : await JsonSerializer.DeserializeAsync<T>(responseStream, jsonSerializerOptions, (CancellationToken)cancellationToken).ConfigureAwait(false);

            return apiResponse ?? throw new ApiException("Error reading the stream", new JsonException());
        }
        catch (Exception ex)
        {
            throw HandleException(ex, cancellationToken);
        }
    }

    public async Task<ApiResponse<T>> Put<T>(
        string url,
        object data,
        CancellationToken? cancellationToken = default,
        Dictionary<string, string>? headers = null
    )
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl(url))
            {
                Content = JsonContent.Create(data)
            };

            request.Headers.Add("Accept", "application/json");

            if (headers != null)
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);

            var response = cancellationToken == null
                ? await httpClient.SendAsync(request).ConfigureAwait(false)
                : await httpClient
                .SendAsync(request, (CancellationToken)cancellationToken).ConfigureAwait(false);

            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            // Deserialize the API response
            var apiResponse = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(
                responseStream,
                jsonSerializerOptions,
                cancellationToken ?? CancellationToken.None
            ).ConfigureAwait(false)
            ?? throw new ApiException("Error reading the stream", new JsonException());

            // Check HTTP status
            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = apiResponse?.Message ?? "Unknown error";
                IReadOnlyDictionary<string, string[]>? errors = null;

                if (apiResponse?.Errors != null)
                    errors = apiResponse.Errors;

                throw new ApiException(errorMsg, errors, response.StatusCode);
            }

            return apiResponse ?? throw new ApiException("Empty response data");
        }
        catch (Exception ex)
        {
            throw HandleException(ex, cancellationToken);
        }
    }

    public async Task<ApiResponse<T>> Patch<T>(
        string url,
        object? data,
        CancellationToken? cancellationToken = default,
        Dictionary<string, string>? headers = null
    )
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, BuildUrl(url))
            {
                Content = JsonContent.Create(data)
            };

            request.Headers.Add("Accept", "application/json");

            if (headers != null)
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);

            var response = cancellationToken == null
                ? await httpClient.SendAsync(request).ConfigureAwait(false)
                : await httpClient
                .SendAsync(request, (CancellationToken)cancellationToken).ConfigureAwait(false);

            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            // Deserialize the API response
            var apiResponse = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(
                responseStream,
                jsonSerializerOptions,
                cancellationToken ?? CancellationToken.None
            ).ConfigureAwait(false)
            ?? throw new ApiException("Error reading the stream", new JsonException());

            // Check HTTP status
            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = apiResponse?.Message ?? "Unknown error";
                IReadOnlyDictionary<string, string[]>? errors = null;

                if (apiResponse?.Errors != null)
                    errors = apiResponse.Errors;

                throw new ApiException(errorMsg, errors, response.StatusCode);
            }

            return apiResponse ?? throw new ApiException("Empty response data");
        }
        catch (Exception ex)
        {
            throw HandleException(ex, cancellationToken);
        }
    }

    public async Task<ApiResponse<T>> Upload<T>(
        string url,
        Stream fileStream,
        string filename,
        string contentType,
        object? data = null,
        string mediaField = "file",
        Dictionary<string, string>? headers = null,
        CancellationToken? cancellationToken = default
    )
    {
        try
        {
            using var content = new MultipartFormDataContent();

            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            content.Add(fileContent, mediaField, filename);

            if (data != null)
            {
                var properties = data.GetType().GetProperties();

                foreach (var property in properties)
                {
                    var value = property.GetValue(data);
                    if (value != null)
                    {
                        content.Add(
                            new StringContent(value.ToString()!),
                            property.Name
                        );
                    }
                }
            }

            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(url))
            {
                Content = content
            };

            request.Headers.Add("Accept", "application/json");

            if (headers != null)
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);

            var response = cancellationToken == null
                ? await httpClient.SendAsync(request).ConfigureAwait(false)
                : await httpClient.SendAsync(
                    request,
                    (CancellationToken)cancellationToken
                ).ConfigureAwait(false);

            using var responseStream = await response
                .Content
                .ReadAsStreamAsync()
                .ConfigureAwait(false);

            var apiResponse = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(
                responseStream,
                jsonSerializerOptions,
                cancellationToken ?? CancellationToken.None
            ).ConfigureAwait(false)
            ?? throw new ApiException("Error reading the stream", new JsonException());

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    apiResponse.Message ?? "Unknown error",
                    apiResponse.Errors,
                    response.StatusCode
                );
            }

            return apiResponse;
        }
        catch (Exception ex)
        {
            throw HandleException(ex, cancellationToken);
        }
    }


    public async Task<ApiResponse<T>> Delete<T>(
        string url,
        CancellationToken? cancellationToken = default,
        Dictionary<string, string>? headers = null
    )
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, BuildUrl(url));

            request.Headers.Add("Accept", "application/json");

            if (headers != null)
                foreach (var header in headers)
                    request.Headers.Add(header.Key, header.Value);

            var response = cancellationToken == null
                ? await httpClient.SendAsync(request).ConfigureAwait(false)
                : await httpClient
                .SendAsync(request, (CancellationToken)cancellationToken).ConfigureAwait(false);

            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            // Deserialize the API response
            var apiResponse = await JsonSerializer.DeserializeAsync<ApiResponse<T>>(
                responseStream,
                jsonSerializerOptions,
                cancellationToken ?? CancellationToken.None
            ).ConfigureAwait(false)
            ?? throw new ApiException("Error reading the stream", new JsonException());

            // Check HTTP status
            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = apiResponse?.Message ?? "Unknown error";
                IReadOnlyDictionary<string, string[]>? errors = null;

                if (apiResponse?.Errors != null)
                    errors = apiResponse.Errors;

                throw new ApiException(errorMsg, errors, response.StatusCode);
            }

            return apiResponse ?? throw new ApiException("Empty response data");
        }
        catch (Exception ex)
        {
            throw HandleException(ex, cancellationToken);
        }
    }

    public async Task<string> DownloadWithProgressAsync(
        string url,
        string fileName,
        IProgress<double>? progress = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!Uri.TryCreate(BuildUrl(url), UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid download URL: {url}", nameof(url));

        string extension = Path.GetExtension(uri.AbsolutePath);

        if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)) && !string.IsNullOrEmpty(extension))
        {
            fileName += extension;
        }

        string downloadsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads"
        );

        string path = Path.Combine(downloadsPath, fileName);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(
                    header.Key,
                    header.Value
                );
            }
        }

        using HttpResponseMessage response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        long total = response.Content.Headers.ContentLength ?? -1;
        long read = 0;

        await using Stream stream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        await using System.IO.FileStream file = new(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );

        byte[] buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await file.WriteAsync(
                buffer.AsMemory(0, bytesRead),
                cancellationToken
            );

            read += bytesRead;

            if (total > 0)
                progress?.Report((double)read / total * 100);
        }

        return path;
    }

    private string BuildUrl(string url)
    {
        return string.IsNullOrWhiteSpace(_baseUrl) ? url
            : $"{_baseUrl}{url}";
    }

    private static ApiException HandleException(
        Exception ex,
        CancellationToken? cancellationToken
    )
    {
        return ex switch
        {
            HttpRequestException httpEx when httpEx.StatusCode.HasValue =>
                new ApiException(
                    httpEx.Message,
                    httpEx,
                    httpEx.StatusCode
                ),

            HttpRequestException httpEx =>
                new ApiException(
                    httpEx.Message,
                    httpEx
                ),

            TaskCanceledException taskCanceledEx
            when taskCanceledEx.CancellationToken == cancellationToken =>
                new ApiException(
                    "Request was canceled.",
                    taskCanceledEx
                ),

            JsonException jsonEx =>
                new ApiException("Error deserializing the response.", jsonEx),

            _ => new ApiException(ex.Message, ex)
        };

    }
}
