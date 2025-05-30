namespace MuseDashModTools.Abstractions;

public interface ISerializationService
{
    T? Deserialize<T>(string text);

    string Serialize<T>(T value);
}