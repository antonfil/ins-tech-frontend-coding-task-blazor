namespace ins_tech_frontend_coding_task_blazor.Services;

public sealed class BoardState
{
  public static readonly string[] CommonColors = new[]
  {
    "#FF0000",
    "#00FF00",
    "#0000FF",
    "#A52A2A",
    "#800080"
  };
  public event Action? Changed;
  public AnchorageDimensions AnchorageDimensions { get; private set; } = new(0, 0);
  public List<VesselGroup> VesselGroups { get; } = new();
  public Dictionary<Guid, VesselPlacement> VesselPlacements { get; } = new();
  public Vessel? DraggingVessel { get; private set; }
  private bool[,] _occupied = new bool[0, 0];
  public bool Initilized { get; private set; } = false;
  
  public void Initialize(FleetResponse data)
  {
    AnchorageDimensions = new AnchorageDimensions(data.AnchorageSize.Width, data.AnchorageSize.Height);
    _occupied = new bool[AnchorageDimensions.Width, AnchorageDimensions.Height];

    VesselGroups.Clear();
    VesselPlacements.Clear();
    DraggingVessel = null;

    for (int fleetIndex = 0; fleetIndex < data.Fleets.Count; fleetIndex++)
    {
      var fleet = data.Fleets[fleetIndex];
      var color = CommonColors[fleetIndex % CommonColors.Length];
      var vessels = new List<Vessel>();

      for (int i = 0; i < fleet.ShipCount; i++)
      {
        vessels.Add(new Vessel(
            Id: Guid.NewGuid(),
            ShipDesignation: fleet.ShipDesignation,
            Width: fleet.SingleShipDimensions.Width,
            Height: fleet.SingleShipDimensions.Height,
            Color: color
        ));
      }

      VesselGroups.Add(new VesselGroup(
          ShipDesignation: fleet.ShipDesignation,
          Vessels: vessels,
          Color: color
      ));
    }

    Initilized = true;

    NotifyChanged();
  }

  public void BeginDrag(Vessel vessel)
  {
      DraggingVessel = vessel;
      // NotifyChanged();
  }

  public void EndDrag()
  {
      DraggingVessel = null;
      // NotifyChanged();
  }

  public bool PlaceVessel(int x, int y)
  {
    var vessel = DraggingVessel;
    if (vessel == null) return false;

    if (!CanPlaceVessel(vessel, x, y)) return false;

    VesselPlacements[vessel.Id] = new VesselPlacement(vessel, x, y);
    for (int i = x; i < x + vessel.Width; i++)
    {
      for (int j = y; j < y + vessel.Height; j++)
      {
        _occupied[i, j] = true;
      }
    }

    NotifyChanged();
    
    return true;
  }

  public bool CanPlaceVessel(Vessel vessel, int x, int y)
  {
    if (x + vessel.Width > AnchorageDimensions.Width) return false;
    if (y + vessel.Height > AnchorageDimensions.Height) return false;

    for (int i = x; i < x + vessel.Width; i++)
    {
      for (int j = y; j < y + vessel.Height; j++)
      {
        if (_occupied[i, j]) return false;
      }
    }

    return true;
  }

  public bool IsVesselPlaced(Guid vesselId) => VesselPlacements.ContainsKey(vesselId);

  private void NotifyChanged() => Changed?.Invoke();
}

public sealed record AnchorageDimensions(int Width, int Height);

public sealed record VesselGroup(string ShipDesignation, List<Vessel> Vessels, string Color);

public sealed record Vessel(Guid Id, string ShipDesignation, int Width, int Height, string Color);

public sealed record VesselPlacement(Vessel Vessel, int X, int Y);

