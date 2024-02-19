using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Core;
using System.Diagnostics;
using ArcGIS.Desktop.Mapping;
using System.Reflection.Metadata;

namespace STING.DataConversions
{
    internal class GmlConverter
    {
        internal async void ToShp(string xmlData, string geodatabasePath, string newFeatureClassName)
        {
            // parse xml
            XDocument xdoc = XDocument.Parse(xmlData);
            XNamespace gml = "http://www.opengis.net/gml";

            // Get feature members
            IEnumerable<XElement> featureMembers = xdoc.Descendants().Where(e => e.Name.LocalName == "featureMember");


            // Create feature class
            await QueuedTask.Run(async () =>
            {
                // Create GDB object
                FileGeodatabaseConnectionPath geodatabaseConnectionPath = new FileGeodatabaseConnectionPath(new Uri(geodatabasePath));
                Geodatabase geodatabase = new Geodatabase(geodatabaseConnectionPath);


                // Create a feature class
                var spatialReference = SpatialReferenceBuilder.CreateSpatialReference(4326);
                var outputFeatureClass = Path.Combine(geodatabasePath, newFeatureClassName);
                List<string> arguments = new()
                {
                    geodatabase.GetPath().AbsolutePath, // .gdb connection path
                    newFeatureClassName,                // New feature class name
                    "POLYGON",                          // Geometry type
                    "",                                 // No template
                    "DISABLED",                         // No m values
                    "DISABLED",                         // No z values
                    spatialReference.Wkid.ToString()    // Spatial reference
                };

                var argsArray = Geoprocessing.MakeValueArray(arguments.ToArray());
                IGPResult gpCreateFeatureClassResult = await Geoprocessing.ExecuteToolAsync("management.CreateFeatureclass", argsArray);


                // Add relevant attribute fields to feature class
                FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(newFeatureClassName);
                await AddField(featureClass, "AREASYMBOL", FieldType.String);
                await AddField(featureClass, "MUSYM", FieldType.String);
                await AddField(featureClass, "NATIONALMUSYM", FieldType.String);
                await AddField(featureClass, "MUKEY", FieldType.String);


                // Create row buffer and add to feature class
                TableDefinition tableDefinition = featureClass.GetDefinition();
                RowBuffer rowBuffer = featureClass.CreateRowBuffer();
                InsertCursor insertCursor = featureClass.CreateInsertCursor();

                foreach (var featureMember in featureMembers)
                {
                    // Get attributes
                    string areaSymbol = featureMember.Descendants().Where(e => e.Name.LocalName.ToLower() == "areasymbol").FirstOrDefault()?.Value;
                    string musym = featureMember.Descendants().Where(e => e.Name.LocalName.ToLower() == "musym").FirstOrDefault()?.Value;
                    string nationalMusym = featureMember.Descendants().Where(e => e.Name.LocalName.ToLower() == "nationalmusym").FirstOrDefault()?.Value;
                    string mukey = featureMember.Descendants().Where(e => e.Name.LocalName.ToLower() == "mukey").FirstOrDefault()?.Value;
                    // Convert coordinates to geometry -- target polygon, not bounding box
                    //var coordinates = featureMember.Descendants()
                    //    .Where(e => e.Name.LocalName.ToLower() == "polygon")
                    //    .Where(e => e.Name.LocalName.ToLower() == "coordinates").FirstOrDefault()?.Value;
                    //var coordinates = featureMember.Descendants("Polygon").Where(e => e.Name.LocalName.ToLower() == "coordinates").FirstOrDefault()?.Value;
                    var coordinates = featureMember.Descendants().Where(e => e.Name.LocalName.ToLower() == "polygon")
                        .Descendants().Where(n => n.Name.LocalName.ToLower() == "coordinates").FirstOrDefault()?.Value;
                    var polygon = CreatePolygonFromCoordinates(coordinates);
                    // Apply geometry and fields to rowBuffer
                    rowBuffer["Shape"] = polygon;
                    rowBuffer["AREASYMBOL"] = areaSymbol;
                    rowBuffer["MUSYM"] = musym;
                    rowBuffer["NATIONALMUSYM"] = nationalMusym;
                    rowBuffer["MUKEY"] = mukey;
                    // Apply rowBuffer to feature class
                    insertCursor.Insert(rowBuffer);
                }
                // Save feature class/table changes
                insertCursor.Flush();


                Debug.Print("Finishing");
                //LayerFactory.Instance.CreateLayer(new Uri(outputFeatureClass), MapView.Active.Map);
            });
            
        }

        private static Polygon CreatePolygonFromCoordinates(string coordinates)
        {
            //var pointList = coordinates.Split(' ')
            //    .Select(coord => coord.Split(','))
            //    .Select(coords => MapPointBuilder.CreateMapPoint(double.Parse(coords[1]), double.Parse(coords[0])))
            //    .ToList();
            List<MapPoint> pointList = new List<MapPoint>();
            var coordsArr = coordinates.Split(' ');
            for (int i = 0; i < coordsArr.Length - 1; i++)
            {
                var coords = coordsArr[i].Split(",");
                MapPoint mapPoint = MapPointBuilder.CreateMapPoint(double.Parse(coords[1]), double.Parse(coords[0]));
                pointList.Add(mapPoint);
            };

            var polygon = PolygonBuilder.CreatePolygon(pointList);
            return polygon;
        }

        private static async Task AddField(FeatureClass featureClass, string fieldName, FieldType fieldType)
        {
            List<string> arguments = new()
            {
                featureClass.GetPath().AbsolutePath,    // Target existing feature class
                fieldName,                              // Name of field to be created
                fieldType.ToString()                    // Data type of new field
            };
            var argsArray = Geoprocessing.MakeValueArray(arguments.ToArray());
            IGPResult addFieldResult = await Geoprocessing.ExecuteToolAsync("management.AddField", argsArray);

            Debug.Print($"Attempted AddField({fieldName})");
        }
    }
}
