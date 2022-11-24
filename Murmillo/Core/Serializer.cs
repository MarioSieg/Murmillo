using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Murmillo.Core;

public static class Serializer
{
    /// <summary>
    ///     Serializes the object to a JSON file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool SerializeToJson<T>(string filePath, in T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        try
        {
            string json = JsonConvert.SerializeObject(value, Formatting.Indented, new StringEnumConverter());
            File.WriteAllText(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to serialize typeof({typeof(T).Name}) to file '{filePath}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    ///     Deserializes an object from a JSON file.
    ///     If the serialization fails, a new instance of T is returned.
    /// </summary>
    /// <param name="filePath"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static T DeserializeFromJson<T>(string filePath) where T : new()
    {
        try
        {
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json)) throw new SerializationException($"Json file is empty: '{filePath}");
            T? obj = JsonConvert.DeserializeObject<T>(json);
            if (obj == null) throw new SerializationException("Json serializer returned null");
            return obj;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize typeof({typeof(T).Name}) to file '{filePath}': {ex.Message}");
            return new T();
        }
    }
}