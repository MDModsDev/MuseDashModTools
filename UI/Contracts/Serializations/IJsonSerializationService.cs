using System.Text.Json;

namespace MuseDashModToolsUI.Contracts;

public interface IJsonSerializationService : ISerializationService
{
    T? Deserialize<T>(string json, JsonSerializerOptions? options);

    ValueTask<T?> DeserializeAsync<T>(
        Stream utf8Json,
        JsonSerializerOptions? options = default,
        CancellationToken cancellationToken = default);

    ValueTask<T?> DeserializeIndentedAsync<T>(
        Stream utf8Json,
        CancellationToken cancellationToken = default);

    string Serialize<T>(T value, JsonSerializerOptions? options);

    Task SerializeAsync<T>(
        Stream utf8Json,
        T value,
        JsonSerializerOptions? options = default,
        CancellationToken cancellationToken = default);

    Task SerializeIndentedAsync<T>(
        Stream utf8Json,
        T value,
        CancellationToken cancellationToken = default);
}