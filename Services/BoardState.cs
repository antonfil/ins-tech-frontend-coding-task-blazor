namespace ins_tech_frontend_coding_task_blazor.Services;

public sealed class BoardState
{
  public int CellSizePx { get; } = 30;
  public AnchorageDimensions AnchorageDimensions { get; private set; } = new(0, 0);
  public List<VesselGroup> VesselGroups { get; } = new();
  public Dictionary<Guid, VesselPlacement> VesselPlacements { get; } = new();
  public Vessel? DraggingVessel { get; private set; }
  public double DraggingVesselOffsetX { get; private set; } = 0;
  public double DraggingVesselOffsetY { get; private set; } = 0;
  public bool Initilized { get; private set; } = false;
  public event Action? Changed;

  private bool[,] _occupied = new bool[0, 0];
  private static readonly string[] CommonColors = new[]
  {
    "#1F77B4", // blue
    "#2CA02C", // green
    "#D62728", // red
    "#9467BD", // purple
    "#8C564B"  // brown
  };
  
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
            Color: color,
            Index: i + 1
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

  public void BeginDrag(Vessel vessel, double offsetX, double offsetY)
  {
      DraggingVessel = vessel;
      DraggingVesselOffsetX = offsetX;
      DraggingVesselOffsetY = offsetY;
      NotifyChanged();
  }

  public void EndDrag()
  {
      DraggingVessel = null;
      DraggingVesselOffsetX = 0;
      DraggingVesselOffsetY = 0;
      NotifyChanged();
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

  public void RemoveVessel(Guid vesselId)
  {
    if (!VesselPlacements.TryGetValue(vesselId, out var placement)) return;

    var vessel = placement.Vessel;
    var x = placement.X;
    var y = placement.Y;

    for (int i = x; i < x + vessel.Width; i++)
    {
      for (int j = y; j < y + vessel.Height; j++)
      {
        _occupied[i, j] = false;
      }
    }

    VesselPlacements.Remove(vesselId);

    NotifyChanged();
  }

  public bool CanPlaceVessel(Vessel vessel, int x, int y)
  {
    if (x < 0 || y < 0) return false;
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

public sealed record Vessel(Guid Id, string ShipDesignation, int Width, int Height, string Color, int Index);

public sealed record VesselPlacement(Vessel Vessel, int X, int Y);

public enum VesselLocation
{
  Anchorage,
  VesselPool
}

