using System.Text.Json;

namespace Demo;
public static class JsonHelper
{
    private static readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };
        
    static JsonHelper()
    {
        // register converters only once
        options.Converters.Add(new DateOnlyJsonConverter());
    }

    /// <summary>
    /// Serialize any object (or collection) to JSON string, using
    /// DateOnly converter and camel-casing.
    /// </summary>
    public static string Serialize<T>(T obj) =>
        JsonSerializer.Serialize(obj, options);

    /// <summary>
    /// Deserialize JSON back into your type.
    /// </summary>
    public static T? Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, options);
}