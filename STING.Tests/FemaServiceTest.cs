using System;
using Xunit;
using STING;
using STING.Services;
using STING.Structs;
using System.Reflection;
using STING.DataConversions;

namespace STING.Tests
{
    public class FemaServiceTest
    {
        [Fact]
        public async Task GetFloodplainFeatures_ReturnsGeojson()
        {
            // Arrange
            var femaService = new FemaService();
            var coordsSW = new Coordinates(-77.662260f, 39.389419f);
            var coordsNE = new Coordinates(-77.649369f,39.396397f);
            var boundingBox = new BoxCoordinates(coordsSW, coordsNE);

            // Act
            var response = await femaService.GetFloodplainFeatures(boundingBox);

            // Assert
            Assert.NotNull(response);
        }
    }
}