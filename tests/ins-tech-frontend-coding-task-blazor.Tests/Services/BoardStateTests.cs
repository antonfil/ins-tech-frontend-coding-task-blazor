using FluentAssertions;
using ins_tech_frontend_coding_task_blazor.Services;

namespace ins_tech_frontend_coding_task_blazor.Tests.UnitTests.Services
{
    public class BoardStateTests
    {
        private readonly BoardState _boardState;
        private readonly FleetResponse _sampleFleetResponse;

        public BoardStateTests()
        {
            _boardState = new BoardState();
            
            _sampleFleetResponse = new FleetResponse
            {
                AnchorageSize = new AnchorageSize{Width = 10, Height = 10},
                Fleets = new List<FleetItem>
                {
                    new FleetItem
                    {
                        ShipDesignation = "Destroyer",
                        ShipCount = 2,
                        SingleShipDimensions = new ShipDimensions{Width = 3, Height = 1}
                    },
                    new FleetItem
                    {
                        ShipDesignation = "Cruiser",
                        ShipCount = 1,
                        SingleShipDimensions = new ShipDimensions{Width = 2, Height = 2}
                    }
                }
            };
        }

        #region Initialization Tests

        [Fact]
        public void Constructor_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act - done in constructor
            // Assert
            _boardState.CellSizePx.Should().Be(40);
            _boardState.AnchorageDimensions.Should().BeEquivalentTo(new AnchorageDimensions(0, 0));
            _boardState.VesselGroups.Should().BeEmpty();
            _boardState.VesselPlacements.Should().BeEmpty();
            _boardState.DraggingVessel.Should().BeNull();
            _boardState.IsDraggingSuccessfull.Should().BeFalse();
            _boardState.Initilized.Should().BeFalse();
            _boardState.DraggingVesselOffsetX.Should().Be(0);
            _boardState.DraggingVesselOffsetY.Should().Be(0);
        }

        [Fact]
        public void Initialize_ShouldSetPropertiesCorrectly()
        {
            // Act
            _boardState.Initialize(_sampleFleetResponse);

            // Assert
            _boardState.AnchorageDimensions.Should().BeEquivalentTo(new AnchorageDimensions(10, 10));
            _boardState.Initilized.Should().BeTrue();
            _boardState.VesselGroups.Should().HaveCount(2);
            
            var destroyerGroup = _boardState.VesselGroups[0];
            destroyerGroup.ShipDesignation.Should().Be("Destroyer");
            destroyerGroup.Vessels.Should().HaveCount(2);
            destroyerGroup.Color.Should().NotBeNullOrEmpty();
            
            var cruiserGroup = _boardState.VesselGroups[1];
            cruiserGroup.ShipDesignation.Should().Be("Cruiser");
            cruiserGroup.Vessels.Should().HaveCount(1);
            cruiserGroup.Color.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Initialize_ShouldAssignDifferentColorsToDifferentFleets()
        {
            // Act
            _boardState.Initialize(_sampleFleetResponse);

            // Assert
            _boardState.VesselGroups[0].Color.Should().NotBe(_boardState.VesselGroups[1].Color);
        }

        [Fact]
        public void Initialize_ShouldCreateVesselsWithCorrectProperties()
        {
            // Act
            _boardState.Initialize(_sampleFleetResponse);

            // Assert
            var destroyers = _boardState.VesselGroups[0].Vessels;
            destroyers.Should().HaveCount(2);
            
            destroyers[0].ShipDesignation.Should().Be("Destroyer");
            destroyers[0].Width.Should().Be(3);
            destroyers[0].Height.Should().Be(1);
            destroyers[0].Index.Should().Be(1);
            destroyers[0].Id.Should().NotBe(Guid.Empty);
            
            destroyers[1].Index.Should().Be(2);
        }

        [Fact]
        public void Initialize_ShouldResetStateWhenCalledMultipleTimes()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var firstVesselId = _boardState.VesselGroups[0].Vessels[0].Id;
            
            // Place a vessel
            _boardState.BeginDrag(_boardState.VesselGroups[0].Vessels[0], 0, 0);
            _boardState.PlaceVessel(0, 0);
            _boardState.EndDrag();
            
            // Act - Reinitialize
            _boardState.Initialize(_sampleFleetResponse);

            // Assert
            _boardState.VesselPlacements.Should().BeEmpty();
            _boardState.DraggingVessel.Should().BeNull();
            _boardState.IsDraggingSuccessfull.Should().BeFalse();
            _boardState.VesselGroups[0].Vessels[0].Id.Should().NotBe(firstVesselId); // New GUID
        }

        #endregion

        #region Drag and Drop Tests

        [Fact]
        public void BeginDrag_ShouldSetDraggingVesselAndOffsets()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            const double offsetX = 5.5;
            const double offsetY = 10.2;

            // Act
            _boardState.BeginDrag(vessel, offsetX, offsetY);

            // Assert
            _boardState.DraggingVessel.Should().Be(vessel);
            _boardState.DraggingVesselOffsetX.Should().Be(offsetX);
            _boardState.DraggingVesselOffsetY.Should().Be(offsetY);
            _boardState.IsDraggingSuccessfull.Should().BeFalse();
        }

        [Fact]
        public void BeginDrag_ShouldClearOccupiedMapIfVesselWasPlaced()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            
            // Place the vessel first
            _boardState.BeginDrag(vessel, 0, 0);
            _boardState.PlaceVessel(2, 2);
            _boardState.EndDrag();
            
            // Act - Start dragging the placed vessel
            _boardState.BeginDrag(vessel, 0, 0);

            // Assert - Should be able to place at the original location
            _boardState.CanPlaceVessel(vessel, 2, 2).Should().BeTrue();
        }

        [Fact]
        public void EndDrag_ShouldClearDraggingState()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            _boardState.BeginDrag(vessel, 5, 5);

            // Act
            _boardState.EndDrag();

            // Assert
            _boardState.DraggingVessel.Should().BeNull();
            _boardState.DraggingVesselOffsetX.Should().Be(0);
            _boardState.DraggingVesselOffsetY.Should().Be(0);
        }

        [Fact]
        public void EndDrag_ShouldRemoveVesselIfDragWasNotSuccessful()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            
            // Place the vessel
            _boardState.BeginDrag(vessel, 0, 0);
            _boardState.PlaceVessel(2, 2);
            _boardState.EndDrag();
            
            // Start new drag but don't place successfully
            _boardState.BeginDrag(vessel, 0, 0);
            // Don't call PlaceVessel - simulate failed placement

            // Act
            _boardState.EndDrag();

            // Assert
            _boardState.IsVesselPlaced(vessel.Id).Should().BeFalse();
        }

        [Fact]
        public void EndDrag_ShouldNotRemoveVesselIfDragWasSuccessful()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            
            // Place the vessel successfully
            _boardState.BeginDrag(vessel, 0, 0);
            _boardState.PlaceVessel(2, 2); // Successful placement

            // Act
            _boardState.EndDrag();

            // Assert
            _boardState.IsVesselPlaced(vessel.Id).Should().BeTrue();
        }

        #endregion

        #region Placement Tests

        [Fact]
        public void PlaceVessel_ShouldReturnFalseWhenNoDraggingVessel()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);

            // Act
            var result = _boardState.PlaceVessel(0, 0);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void PlaceVessel_ShouldPlaceVesselSuccessfully()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            _boardState.BeginDrag(vessel, 0, 0);

            // Act
            var result = _boardState.PlaceVessel(2, 2);

            // Assert
            result.Should().BeTrue();
            _boardState.VesselPlacements.Should().ContainKey(vessel.Id);
            _boardState.IsVesselPlaced(vessel.Id).Should().BeTrue();
            _boardState.IsDraggingSuccessfull.Should().BeTrue();
            
            var placement = _boardState.VesselPlacements[vessel.Id];
            placement.X.Should().Be(2);
            placement.Y.Should().Be(2);
            placement.Vessel.Should().Be(vessel);
        }

        [Fact]
        public void PlaceVessel_ShouldReturnFalseWhenPositionIsInvalid()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0]; // 3x1 vessel
            _boardState.BeginDrag(vessel, 0, 0);

            // Act - Try to place outside bounds
            var result = _boardState.PlaceVessel(8, 8); // Would extend to 11,9 which is outside 10x10

            // Assert
            result.Should().BeFalse();
            _boardState.VesselPlacements.Should().NotContainKey(vessel.Id);
        }

        [Fact]
        public void PlaceVessel_ShouldReturnFalseWhenPositionIsOccupied()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel1 = _boardState.VesselGroups[0].Vessels[0];
            var vessel2 = _boardState.VesselGroups[0].Vessels[1];
            
            // Place first vessel
            _boardState.BeginDrag(vessel1, 0, 0);
            _boardState.PlaceVessel(0, 0);
            _boardState.EndDrag();
            
            // Try to place second vessel in same location
            _boardState.BeginDrag(vessel2, 0, 0);

            // Act
            var result = _boardState.PlaceVessel(0, 0);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void CanPlaceVessel_ShouldReturnFalseForNegativeCoordinates()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];

            // Act & Assert
            _boardState.CanPlaceVessel(vessel, -1, 0).Should().BeFalse();
            _boardState.CanPlaceVessel(vessel, 0, -1).Should().BeFalse();
        }

        [Fact]
        public void CanPlaceVessel_ShouldReturnFalseWhenExceedsBounds()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0]; // 3x1 vessel

            // Act & Assert
            _boardState.CanPlaceVessel(vessel, 8, 9).Should().BeFalse(); // Would be 11x10
            _boardState.CanPlaceVessel(vessel, 0, 10).Should().BeFalse(); // Y out of bounds
        }

        [Fact]
        public void CanPlaceVessel_ShouldReturnTrueForValidPosition()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];

            // Act & Assert
            _boardState.CanPlaceVessel(vessel, 0, 0).Should().BeTrue();
            _boardState.CanPlaceVessel(vessel, 7, 9).Should().BeTrue(); // 3x1 fits at 7,9 (7+3=10)
        }

        #endregion

        #region Removal Tests

        [Fact]
        public void RemoveVessel_ShouldRemovePlacedVessel()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            
            _boardState.BeginDrag(vessel, 0, 0);
            _boardState.PlaceVessel(2, 2);
            _boardState.EndDrag();

            // Act
            _boardState.RemoveVessel(vessel.Id);

            // Assert
            _boardState.VesselPlacements.Should().NotContainKey(vessel.Id);
        }

        [Fact]
        public void RemoveVessel_ShouldDoNothingForNonExistentVessel()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var nonExistentId = Guid.NewGuid();

            // Act
            _boardState.RemoveVessel(nonExistentId);

            // Assert - No exception should be thrown
            _boardState.VesselPlacements.Should().BeEmpty();
        }

        [Fact]
        public void RemoveVessel_ShouldFreeUpOccupiedCells()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0]; // 3x1 vessel
            
            _boardState.BeginDrag(vessel, 0, 0);
            _boardState.PlaceVessel(2, 2);
            _boardState.EndDrag();

            // Act - Remove the vessel
            _boardState.RemoveVessel(vessel.Id);

            // Assert - Should be able to place another vessel in same spot
            var vessel2 = _boardState.VesselGroups[0].Vessels[1];
            _boardState.CanPlaceVessel(vessel2, 2, 2).Should().BeTrue();
        }

        #endregion

        #region Rotation Tests

        [Fact]
        public void RotateVessel_ShouldSwapWidthAndHeight()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0]; // 3x1 vessel
            var originalWidth = vessel.Width;
            var originalHeight = vessel.Height;

            // Act
            _boardState.RotateVessel(vessel.Id);

            // Assert
            var rotatedVessel = _boardState.VesselGroups[0].Vessels[0];
            rotatedVessel.Width.Should().Be(originalHeight);
            rotatedVessel.Height.Should().Be(originalWidth);
        }

        [Fact]
        public void RotateVessel_ShouldDoNothingForNonExistentVesselId()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var nonExistentId = Guid.NewGuid();
            var originalVessel = _boardState.VesselGroups[0].Vessels[0];

            // Act
            _boardState.RotateVessel(nonExistentId);

            // Assert - Vessel should remain unchanged
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            vessel.Should().BeEquivalentTo(originalVessel);
        }

        #endregion

        #region Completion Tests

        [Fact]
        public void IsVesselsPlacementCompleted_ShouldReturnFalseWhenNotAllPlaced()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            
            // Place only one vessel
            _boardState.BeginDrag(vessel, 0, 0);
            _boardState.PlaceVessel(0, 0);
            _boardState.EndDrag();

            // Act
            var result = _boardState.IsVesselsPlacementCompleted();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsVesselsPlacementCompleted_ShouldReturnTrueWhenAllPlaced()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            
            // Place all 3 vessels
            foreach (var group in _boardState.VesselGroups)
            {
                foreach (var vessel in group.Vessels)
                {
                    _boardState.BeginDrag(vessel, 0, 0);
                    
                    // Find a free spot (simplified - in real test might need more logic)
                    int x = 0;
                    int y = 0;
                    bool placed = false;
                    
                    for (x = 0; x < 10 && !placed; x++)
                    {
                        for (y = 0; y < 10 && !placed; y++)
                        {
                            if (_boardState.CanPlaceVessel(vessel, x, y))
                            {
                                _boardState.PlaceVessel(x, y);
                                placed = true;
                            }
                        }
                    }
                    
                    _boardState.EndDrag();
                }
            }

            // Act
            var result = _boardState.IsVesselsPlacementCompleted();

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region Event Tests

        [Fact]
        public void ChangedEvent_ShouldFireOnInitialize()
        {
            // Arrange
            bool eventFired = false;
            _boardState.Changed += () => eventFired = true;

            // Act
            _boardState.Initialize(_sampleFleetResponse);

            // Assert
            eventFired.Should().BeTrue();
        }

        [Fact]
        public void ChangedEvent_ShouldFireOnBeginDrag()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            bool eventFired = false;
            _boardState.Changed += () => eventFired = true;
            var vessel = _boardState.VesselGroups[0].Vessels[0];

            // Act
            _boardState.BeginDrag(vessel, 0, 0);

            // Assert
            eventFired.Should().BeTrue();
        }

        [Fact]
        public void ChangedEvent_ShouldFireOnPlaceVessel()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            bool eventFired = false;
            var vessel = _boardState.VesselGroups[0].Vessels[0];
            _boardState.BeginDrag(vessel, 0, 0);
            
            _boardState.Changed += () => eventFired = true;

            // Act
            _boardState.PlaceVessel(0, 0);

            // Assert
            eventFired.Should().BeTrue();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void PlaceVessel_ShouldHandleMultipleVesselsOfDifferentSizes()
        {
            // Arrange
            _boardState.Initialize(_sampleFleetResponse);
            
            var destroyer = _boardState.VesselGroups[0].Vessels[0]; // 3x1
            var cruiser = _boardState.VesselGroups[1].Vessels[0];   // 2x2

            // Act & Assert - Place destroyer
            _boardState.BeginDrag(destroyer, 0, 0);
            _boardState.PlaceVessel(0, 0).Should().BeTrue();
            _boardState.EndDrag();

            // Try to place cruiser overlapping - should fail
            _boardState.BeginDrag(cruiser, 0, 0);
            _boardState.PlaceVessel(0, 0).Should().BeFalse(); // Overlaps
            _boardState.PlaceVessel(3, 0).Should().BeTrue(); // Should fit next to destroyer
            _boardState.EndDrag();
        }

        [Fact]
        public void Initialize_ShouldHandleEmptyFleet()
        {
            // Arrange
            var emptyResponse = new FleetResponse
            {
                AnchorageSize = new AnchorageSize{Width = 10, Height = 10},
                Fleets = new List<FleetItem>()
            };

            // Act
            _boardState.Initialize(emptyResponse);

            // Assert
            _boardState.VesselGroups.Should().BeEmpty();
            _boardState.Initilized.Should().BeTrue();
        }

        #endregion
    }

    // Supporting classes (should be in a separate file in real project)
    /*public class FleetResponse
    {
        public AnchorageDimensions AnchorageSize { get; set; }
        public List<Fleet> Fleets { get; set; } = new();
    }

    public class Fleet
    {
        public string ShipDesignation { get; set; } = string.Empty;
        public int ShipCount { get; set; }
        public AnchorageDimensions SingleShipDimensions { get; set; } = new(0, 0);
    }*/
}