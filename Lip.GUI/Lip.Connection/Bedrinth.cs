using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bedrinth;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SearchPackagesResponse))]
[JsonSerializable(typeof(PackageInfo))]
internal partial class BedrinthSourceGenerationContext : JsonSerializerContext
{
}

public class BedrinthServicesProvider
{
    private readonly HttpClient _httpClient;

    private const string Uri = "https://api.bedrinth.com/v3";

    public BedrinthServicesProvider()
    {
        _httpClient = new HttpClient { };
    }

    public async Task<SearchPackagesResponse?> SearchPackagesAsync(
        string? query = null,
        int? perPage = null,
        int? page = null,
        string? sort = null,
        string? order = null)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(query)) queryParams.Add($"q={query}");
        if (perPage.HasValue) queryParams.Add($"perPage={perPage}");
        if (page.HasValue) queryParams.Add($"page={page}");
        if (!string.IsNullOrEmpty(sort)) queryParams.Add($"sort={sort}");
        if (!string.IsNullOrEmpty(order)) queryParams.Add($"order={order}");
        string requestUrl = Uri + "/packages?" + string.Join("&", queryParams);
        HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<SearchPackagesResponse>(
            content,
            BedrinthSourceGenerationContext.Default.SearchPackagesResponse);
    }

    public async Task<PackageInfo?> GetPackageAsync(string source, string identifier)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"/packages/{source}/{identifier}");
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PackageInfo>(
            content,
            BedrinthSourceGenerationContext.Default.PackageInfo);
    }
}

public class SearchPackagesResponse
{
    [JsonPropertyName("apiVersion")]
    public string ApiVersion { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public SearchPackagesResponseData Data { get; set; } = new();

    public class SearchPackagesResponseData
    {
        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("items")]
        public List<PackageInfo> Items { get; set; } = [];
    }
}

public class PackageInfo
{
    [JsonPropertyName("identifier")]
    public string Identifier { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("author")]
    public string Author { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("avatarUrl")]
    public string AvatarUrl { get; set; } = string.Empty;

    [JsonPropertyName("hotness")]
    public double Hotness { get; set; }

    [JsonPropertyName("updated")]
    public string Updated { get; set; } = string.Empty;

    [JsonPropertyName("projectUrl")]
    public string ProjectUrl { get; set; } = string.Empty;

    [JsonPropertyName("contributors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Contributor>? Contributors { get; set; } = null;

    [JsonPropertyName("versions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<VersionInfo>? Versions { get; set; } = null;

    public class Contributor
    {

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("contributions")]
        public int Contributions { get; set; }
    }

    public class VersionInfo
    {

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;

        [JsonPropertyName("releasedAt")]
        public string ReleasedAt { get; set; } = string.Empty;

        [JsonPropertyName("source")]
        public string Source { get; set; } = string.Empty;

        [JsonPropertyName("packageManager")]
        public string PackageManager { get; set; } = string.Empty;

        [JsonPropertyName("platformVersionRequirement")]
        public string PlatformVersionRequirement { get; set; } = string.Empty;
    }
}
