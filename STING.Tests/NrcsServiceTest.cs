using System;
using Xunit;
using STING;
using STING.Services;
using STING.Structs;

namespace STING.Tests
{
    public class NrcsServiceTest
    {
        [Fact]
        public async Task GetSoilFeatures_ReturnsXml()
        {
            // Arrange
            var nrcsService = new NrcsService();
            var coordsSW = new Coordinates(-77.662260f, 39.389419f);
            var coordsNE = new Coordinates(-77.649369f,39.396397f);
            var boundingBox = new BoxCoordinates(coordsSW, coordsNE);

            // Act
            var response = await nrcsService.GetSoilFeatures(boundingBox);

            // Assert
            Assert.NotNull(response);
        }
    }
}