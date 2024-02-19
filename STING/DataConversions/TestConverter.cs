using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Core;
using System.Xml.Linq;
using ArcGIS.Desktop.Mapping;
using System.Diagnostics;

namespace STING.DataConversions
{
    internal class TestConverter
    {
        internal async void ToShp(string geodatabasePath, string newFeatureClassName)
        {
            string rawCoordinates = "38.508767,-77.377019 38.508850,-77.377055 38.509208,-77.376810";
            //XDocument xdoc = XDocument.Parse(rawCoordinates);

            await QueuedTask.Run(async () =>
            {
                // Create GDB object
                FileGeodatabaseConnectionPath geodatabaseConnectionPath = new FileGeodatabaseConnectionPath(new Uri(geodatabasePath));
                Geodatabase geodatabase = new Geodatabase(geodatabaseConnectionPath);

                // Create feature class definition
                // EPSG:4326 = WGS84
                var spatialReference = SpatialReferenceBuilder.CreateSpatialReference(4326);
                //var polygonDefinition = PolygonBuilder.CreatePolygon(spatialReference);
                //var featureClassDefinition = geodatabase.GetDefinition<Polygon>();
                //string outputFeatureClass = $"{geodatabasePath}\\{newFeatureClassName}";
                var outputFeatureClass = Path.Combine(geodatabasePath, newFeatureClassName);

                // bruh
                //List<string> assList = new()
                //{
                //    geodatabase.GetPath().AbsolutePath,
                //    "dumberFCtest"
                //};
                //var assArgs = Geoprocessing.MakeValueArray(geodatabase.GetPath().AbsolutePath, "dumbFCTest");
                //var assArgs2 = Geoprocessing.MakeValueArray(assList.ToArray());
                //IGPResult ass = await Geoprocessing.ExecuteToolAsync("management.CreateFeatureclass", assArgs2);

                //Debug.Print("end");
                #region shit

                // Create a feature class
                List<string> arguments = new()
                {
                    geodatabase.GetPath().AbsolutePath,
                    newFeatureClassName,
                    "POLYGON", // geometry type
                    "", // no template
                    "DISABLED", // no m values
                    "DISABLED", // no z values
                    spatialReference.Wkid.ToString()
                };

                var argsArray = Geoprocessing.MakeValueArray(arguments.ToArray());
                IGPResult gpCreateFeatureClassResult = await Geoprocessing.ExecuteToolAsync("management.CreateFeatureclass", argsArray);

                // make polygon
                var polygon = CreatePolygonFromCoordinate(rawCoordinates, spatialReference);
                // append
                //List<string> appendArguments = new()
                //{
                //    polygon.ToJson(),
                //    outputFeatureClass+".shp"
                //};
                string polyjson = polygon.ToJson();
                string polystr = polygon.ToString();
                string polyxml = polygon.ToXml();
                Debug.Print(polyjson);
                Debug.Print(polystr);
                Debug.Print(polyxml);

                //FeatureClass 

                //var appendParams = Geoprocessing.MakeValueArray(polygon.ToJson(), outputFeatureClass);
                //IGPResult gpAppendResult = await Geoprocessing.ExecuteToolAsync("management.Append", appendParams);

                FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(newFeatureClassName);
                RowBuffer rowBuffer = featureClass.CreateRowBuffer();
                rowBuffer["Shape"] = polygon;
                InsertCursor insertCursor = featureClass.CreateInsertCursor();
                insertCursor.Insert(rowBuffer);
                insertCursor.Flush();

                //Debug.Print($"Output feature class: {outputFeatureClass}");
                //Debug.Print($"Feature Class Exists: {geodatabase.ExistsDataset<FeatureClass>(newFeatureClassName)}");

                // Create layer and add to map
                Debug.Print("Finishing");
                //LayerFactory.Instance.CreateLayer(new Uri(outputFeatureClass), MapView.Active.Map);

                #endregion
            });

        }
        private static Polygon CreatePolygonFromCoordinate(string _coordinates, SpatialReference _spatialReference)
        {
            var pointList = _coordinates.Split(' ')
                .Select(coord => coord.Split(','))
                .Select(coords => MapPointBuilder.CreateMapPoint(double.Parse(coords[1]), double.Parse(coords[0])))
                .ToList();

            return PolygonBuilder.CreatePolygon(pointList, _spatialReference);
        }
    }
}
