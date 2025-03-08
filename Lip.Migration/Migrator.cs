using System.Text.Json;

namespace Lip.Migration;

public static class Migrator
{
    public static JsonElement Migrate(JsonElement json)
    {
        if (IsLatest(json))
        {
            return json;
        }

        if (MigratorFromV2.IsMigratable(json))
        {
            return MigratorFromV2.Migrate(json);
        }

        if (MigratorFromV1.IsMigratable(json))
        {
            return MigratorFromV1.Migrate(json);
        }

        throw new JsonException("Unsupported manifest format.");
    }

    private static bool IsLatest(JsonElement json)
    {
        return json.TryGetProperty("format_version", out var version)
               && version.TryGetInt32(out var v)
               && v == 3
               && json.TryGetProperty("format_uuid", out var uuid)
               && uuid.GetString() == "289f771f-2c9a-4d73-9f3f-8492495a924d";
    }
}
