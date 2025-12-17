using FluentAssertions;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using ins_tech_frontend_coding_task_blazor.Models;
using ins_tech_frontend_coding_task_blazor.Services;

namespace ins_tech_frontend_coding_task_blazor.Tests.UnitTests.Services
{
    public class FleetApiClientTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly FleetApiClient _fleetApiClient;

        public FleetApiClientTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _fleetApiClient = new FleetApiClient(_httpClient);
        }

        [Fact]
        public async Task GetRandomFleetAsync_ShouldDeserializeJsonCorrectly()
        {
            // Arrange
            var json = @"
            {
                ""anchorageSize"": { 
                    ""width"": 12, 
                    ""height"": 10 
                },
                ""fleets"": [
                    {
                        ""singleShipDimensions"": { 
                            ""width"": 3, 
                            ""height"": 1 
                        },
                        ""shipDesignation"": ""Destroyer"",
                        ""shipCount"": 2
                    },
                    {
                        ""singleShipDimensions"": { 
                            ""width"": 2, 
                            ""height"": 2 
                        },
                        ""shipDesignation"": ""Cruiser"",
                        ""shipCount"": 1
                    }
                ]
            }";

            SetupHttpResponse(json);

            // Act
            var result = await _fleetApiClient.GetRandomFleetAsync();

            // Assert
            result.Should().NotBeNull();
            
            // Test AnchorageSize
            result!.AnchorageSize.Should().NotBeNull();
            result.AnchorageSize.Width.Should().Be(12);
            result.AnchorageSize.Height.Should().Be(10);
            
            // Test Fleets
            result.Fleets.Should().HaveCount(2);
            
            // Test first fleet
            var destroyerFleet = result.Fleets[0];
            destroyerFleet.ShipDesignation.Should().Be("Destroyer");
            destroyerFleet.ShipCount.Should().Be(2);
            destroyerFleet.SingleShipDimensions.Should().NotBeNull();
            destroyerFleet.SingleShipDimensions.Width.Should().Be(3);
            destroyerFleet.SingleShipDimensions.Height.Should().Be(1);
            
            // Test second fleet
            var cruiserFleet = result.Fleets[1];
            cruiserFleet.ShipDesignation.Should().Be("Cruiser");
            cruiserFleet.ShipCount.Should().Be(1);
            cruiserFleet.SingleShipDimensions.Should().NotBeNull();
            cruiserFleet.SingleShipDimensions.Width.Should().Be(2);
            cruiserFleet.SingleShipDimensions.Height.Should().Be(2);
        }

        [Fact]
        public async Task GetRandomFleetAsync_ShouldHandleEmptyFleet()
        {
            // Arrange
            var json = @"
            {
                ""anchorageSize"": { 
                    ""width"": 8, 
                    ""height"": 8 
                },
                ""fleets"": []
            }";

            SetupHttpResponse(json);

            // Act
            var result = await _fleetApiClient.GetRandomFleetAsync();

            // Assert
            result.Should().NotBeNull();
            result!.AnchorageSize.Width.Should().Be(8);
            result.AnchorageSize.Height.Should().Be(8);
            result.Fleets.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRandomFleetAsync_ShouldThrowJsonException_WhenJsonIsInvalid()
        {
            // Arrange
            var invalidJson = "{ invalid json }";
            SetupHttpResponse(invalidJson);

            // Act
            Func<Task> act = async () => await _fleetApiClient.GetRandomFleetAsync();

            // Assert - Should throw JsonException
            await act.Should().ThrowAsync<JsonException>();
        }

        [Fact]
        public async Task GetRandomFleetAsync_ShouldThrowHttpRequestException_WhenHttpFails()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            Func<Task> act = async () => await _fleetApiClient.GetRandomFleetAsync();

            // Assert - Should throw HttpRequestException
            await act.Should().ThrowAsync<HttpRequestException>()
                .WithMessage("Network error");
        }

        [Fact]
        public void Models_ShouldDeserializeMinimalJson()
        {
            // Arrange
            var minimalJson = @"
            {
                ""anchorageSize"": { 
                    ""width"": 5, 
                    ""height"": 5 
                },
                ""fleets"": [
                    {
                        ""singleShipDimensions"": { 
                            ""width"": 1, 
                            ""height"": 1 
                        },
                        ""shipDesignation"": ""Test"",
                        ""shipCount"": 1
                    }
                ]
            }";

            // Act
            var result = JsonSerializer.Deserialize<FleetResponse>(
                minimalJson, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            result.Should().NotBeNull();
            result!.AnchorageSize.Width.Should().Be(5);
            result.AnchorageSize.Height.Should().Be(5);
            result.Fleets[0].ShipDesignation.Should().Be("Test");
            result.Fleets[0].ShipCount.Should().Be(1);
        }

        [Fact]
        public void Models_ShouldSerializeWithCorrectPropertyNames()
        {
            // Arrange
            var fleetResponse = new FleetResponse
            {
                AnchorageSize = new AnchorageSize { Width = 10, Height = 8 },
                Fleets = new List<FleetItem>
                {
                    new FleetItem
                    {
                        ShipDesignation = "Destroyer",
                        ShipCount = 2,
                        SingleShipDimensions = new ShipDimensions { Width = 3, Height = 1 }
                    }
                }
            };

            // Act
            var json = JsonSerializer.Serialize(fleetResponse);

            // Assert
            json.Should().Contain("\"anchorageSize\"");
            json.Should().Contain("\"width\"");
            json.Should().Contain("\"height\"");
            json.Should().Contain("\"fleets\"");
            json.Should().Contain("\"shipDesignation\"");
            json.Should().Contain("\"shipCount\"");
            json.Should().Contain("\"singleShipDimensions\"");
        }

        private void SetupHttpResponse(string jsonContent)
        {
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonContent)
                });
        }
    }
}