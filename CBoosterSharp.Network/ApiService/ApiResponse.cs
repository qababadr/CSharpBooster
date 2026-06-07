using System.Text.Json.Serialization;

namespace CBoosterSharp.Network.ApiService;

public sealed record ApiResponse<T>(
    [property: JsonPropertyName("data")]
    T Data,

    [property: JsonPropertyName("message")]
    string? Message = null,

    [property: JsonPropertyName("errors")]
    IReadOnlyDictionary<string, string[]>? Errors = null,

    [property: JsonPropertyName("extra")]
    IReadOnlyDictionary<string, object>? Extra = null

)
{
    public static ApiResponse<T?> Empty
        => new(default,
            null,
            new Dictionary<string, string[]>(),
            new Dictionary<string, object>()
        );
}