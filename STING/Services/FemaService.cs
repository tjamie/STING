using STING.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("STING.Tests")]

namespace STING.Services
{
    internal class FemaService
    {
        // Note: /query? has a limit on number of features returned
        // Connection strings
        // Layer 28 = flood hazard areas
        const string baseUrl = @"https://hazards.fema.gov/gis/nfhl/rest/services/public/NFHL/MapServer/28/query";
        const string geometryTypeString = @"?geometryType=esriGeometryEnvelope";
        // Target WGS84/EPSG:4326
        const string srs = "4326";
        const string inSrString = $@"&inSR={srs}";
        const string outSrString = $@"&outSR={srs}";
        const string returnGeometryString = @"&returnGeometry=true";
        const string fieldsString = @"&outFields=*";
        const string fString = @"&f=geojson";

        internal async Task<string> GetFloodplainFeatures(BoxCoordinates boundingBox)
        {
            // Finalize geometry string
            string xMin = boundingBox.southwest.longitude.ToString();
            string yMin = boundingBox.southwest.latitude.ToString();
            string xMax = boundingBox.northeast.longitude.ToString();
            string yMax = boundingBox.northeast.latitude.ToString();
            string geometryString = $"&geometry={{xmin: {xMin}, ymin: {yMin}, xmax: {xMax}, ymax: {yMax}}}";
            string requestUrl = $@"{baseUrl}{geometryTypeString}{geometryString}{inSrString}{outSrString}{returnGeometryString}{fieldsString}{fString}";

            // Make HTTP GET request
            // Note that FEMA responses can be absurdly large
            // TODO make a general http class with error handling for unavailable connections etc
            HttpClient httpClient = new();
            using HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            return jsonResponse;
        }
    }
}
