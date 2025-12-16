
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ins_tech_frontend_coding_task_blazor.Services;

public sealed class FleetApiClient
{
    private readonly HttpClient _http;

    public FleetApiClient(HttpClient http)
    {
        _http = http;
    }

    public Task<FleetResponse?> GetRandomFleetAsync(CancellationToken ct = default)
        => _http.GetFromJsonAsync<FleetResponse>("https://proxy.corsfix.com/?https://esa.instech.no/api/fleets/random", ct);
}

public sealed class FleetResponse
{
    [JsonPropertyName("anchorageSize")]
    public AnchorageSize AnchorageSize { get; set; } = new();

    [JsonPropertyName("fleets")]
    public List<FleetItem> Fleets { get; set; } = new();
}

public sealed class AnchorageSize
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

public sealed class FleetItem
{
    [JsonPropertyName("singleShipDimensions")]
    public ShipDimensions SingleShipDimensions { get; set; } = new();

    [JsonPropertyName("shipDesignation")]
    public string ShipDesignation { get; set; } = string.Empty;

    [JsonPropertyName("shipCount")]
    public int ShipCount { get; set; }
}

public sealed class ShipDimensions
{
    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}