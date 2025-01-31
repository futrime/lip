using System.Reflection;
using System.Text.Json.Serialization;

namespace Lip;

public partial class Lip
{
    public record ConfigDeleteArgs { }
    public record ConfigGetArgs { }
    public record ConfigListArgs { }
    public record ConfigSetArgs { }

    public async Task ConfigDelete(List<string> keys, ConfigDeleteArgs _)
    {
        if (keys.Count == 0)
        {
            throw new ArgumentException("No configuration keys provided.", nameof(keys));
        }

        Dictionary<string, string> allKeyValuePairs = typeof(RuntimeConfig).GetProperties()
            .Where(prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>() != null)
            .ToDictionary(
                prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name,
                prop => prop.GetValue(new RuntimeConfig())!.ToString()!
            );

        // Set the configuration keys to their default values.
        Dictionary<string, string> keyValuePairs = keys.ToDictionary(
            key => key,
            key => allKeyValuePairs.GetValueOrDefault(key) ?? throw new ArgumentException($"Unknown configuration key: '{key}'.", nameof(keys))
        );

        await ConfigSet(keyValuePairs, new());
    }

    public Dictionary<string, string> ConfigGet(List<string> keys, ConfigGetArgs args)
    {
        if (keys.Count == 0)
        {
            throw new ArgumentException("No configuration keys provided.", nameof(keys));
        }

        Dictionary<string, string> allKeyValuePairs = ConfigList(new());

        Dictionary<string, string> keyValuePairs = keys.ToDictionary(
            key => key,
            key => allKeyValuePairs.GetValueOrDefault(key) ?? throw new ArgumentException($"Unknown configuration key: '{key}'.", nameof(keys))
        );

        return keyValuePairs;
    }

    public Dictionary<string, string> ConfigList(ConfigListArgs _)
    {
        Dictionary<string, string> allKeyValuePairs = typeof(RuntimeConfig).GetProperties()
            .Where(prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>() != null)
            .ToDictionary(
                prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name,
                prop => prop.GetValue(_runtimeConfig)!.ToString()!
            );

        return allKeyValuePairs;
    }

    public async Task ConfigSet(Dictionary<string, string> keyValuePairs, ConfigSetArgs _)
    {
        if (keyValuePairs.Count == 0)
        {
            throw new ArgumentException("No configuration items provided.", nameof(keyValuePairs));
        }

        // Clone the current runtime configuration to avoid modifying the original one.
        RuntimeConfig newRuntimeConfig = _runtimeConfig with { };

        // Update the configuration with the new values.
        foreach ((string key, string value) in keyValuePairs)
        {
            PropertyInfo matchedProperty = typeof(RuntimeConfig).GetProperties()
                .Where(prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>() != null)
                .FirstOrDefault(prop => prop.GetCustomAttribute<JsonPropertyNameAttribute>()!.Name == key)
                ?? throw new ArgumentException($"Unknown configuration key: '{key}'.", nameof(keyValuePairs));

            object convertedValue = Convert.ChangeType(value, matchedProperty.PropertyType);
            matchedProperty.SetValue(newRuntimeConfig, convertedValue);
        }

        await _context.FileSystem.File.WriteAllBytesAsync(_pathManager.RuntimeConfigPath, newRuntimeConfig.ToJsonBytes());
    }
}
