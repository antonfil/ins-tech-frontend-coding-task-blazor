namespace ins_tech_frontend_coding_task_blazor.Models;

public sealed record AnchorageDimensions(int Width, int Height);

public sealed record VesselGroup(string ShipDesignation, List<Vessel> Vessels, string Color);

public sealed record Vessel(Guid Id, string ShipDesignation, int Width, int Height, string Color, int Index);

public sealed record VesselPlacement(Vessel Vessel, int X, int Y);

public enum VesselLocation
{
  Anchorage,
  VesselPool
}