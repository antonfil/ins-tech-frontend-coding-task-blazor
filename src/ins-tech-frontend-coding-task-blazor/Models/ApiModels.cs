using System.Text.Json.Serialization;

namespace ins_tech_frontend_coding_task_blazor.Models;

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