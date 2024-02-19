using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STING.Structs;
using System.Windows.Markup;
using System.Net.Http;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("STING.Tests")]

namespace STING.Services
{
    internal class NrcsService
    {
        // Write this bit to interact with USDA server(s), then start XUnit project (STING.Tests) to test without needing to instantiate ArcGIS

        // Returns data in WGS84 datum (EPSG 4326)
        const string baseUrl = @"https://SDMDataAccess.sc.egov.usda.gov/Spatial/SDMWGS84Geographic.wfs";
        const string serviceString = @"?SERVICE=WFS";
        const string versionString = @"&VERSION=1.1.0";
        const string requestString = @"&REQUEST=GetFeature";
        const string typeString = @"&TYPENAME=MapunitPoly";
        const string srs = "4326";
        const string srsString = $@"&SRSNAME=EPSG:{srs}";
        const string outputFormatString = @"&OUTPUTFORMAT=GML2";

        internal async Task<string> GetSoilFeatures(BoxCoordinates boundingBox)
        {
            // longitude1,latitude1 longitude2, latitude2
            // order apparently must be minx,miny,maxx,maxy
            string coordinatesString = String.Format(
                "{0},{1} {2},{3}",
                boundingBox.southwest.longitude.ToString(),
                boundingBox.southwest.latitude.ToString(),
                boundingBox.northeast.longitude.ToString(),
                boundingBox.northeast.latitude.ToString()
                );
            string filterString = $@"&FILTER=<Filter><BBOX><PropertyName>Geometry</PropertyName><Box srsName='EPSG:{srs}'><coordinates>{coordinatesString}</coordinates></Box></BBOX></Filter>";
            string requestUrl = $@"{baseUrl}{serviceString}{versionString}{requestString}{typeString}{filterString}{srsString}{outputFormatString}";
            //string requestUrl = baseUrl + serviceString + versionString + requestString + typeString + filterString + srsString + outputFormatString;

            #region refactor this
            // TODO create a static HttpClient elsewhere instead of this object
            // see https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient
            HttpClient httpClient = new();
            using HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();
            var xmlResponse = await response.Content.ReadAsStringAsync();
            Console.WriteLine(xmlResponse);
            // Will eventually need to convert this to a shapefile. Aspose exists but isn't free.
            return xmlResponse;
            #endregion
        }
    }
}
