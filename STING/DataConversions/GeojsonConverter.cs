using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Internal.Geometry;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace STING.DataConversions
{
    internal class GeojsonConverter
    {
        internal async void ConvertToFeatureClass(string jsonData, string geodatabasePath, string newFeatureClassName, Dictionary<string, FieldType> attributeFields)
        {
            // Parse geojson
            var jsonDocument = JsonDocument.Parse(jsonData);
            var jsonObject = jsonDocument.RootElement;

            // Extract features array from geojson
            var featuresArray = jsonObject.GetProperty("features").EnumerateArray();

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

                // Add specified attribute fields to feature class
                FeatureClass featureClass = geodatabase.OpenDataset<FeatureClass>(newFeatureClassName);
                if (attributeFields != null)
                {
                    foreach(string field in attributeFields.Keys)
                    {
                        await AddField(featureClass, field, attributeFields[field]);
                    }
                }

                // Create row buffer and add to feature class
                TableDefinition tableDefinition = featureClass.GetDefinition();
                RowBuffer rowBuffer = featureClass.CreateRowBuffer();
                InsertCursor insertCursor = featureClass.CreateInsertCursor();

                // Iterate through array (ie, geojson's features)
                foreach (var feature in featuresArray )
                {
                    // Extract geometry
                    var geometry = feature.GetProperty("geometry");

                    // Extract properties (ie, attribute fields)
                    // For the purposes of this project, NWI will only need Wetlands.ATTRIBUTE and Wetlands.WETLAND_TYPE
                    var properties = feature.GetProperty("properties");

                    // Extract outer coordinates array
                    var outerCoordinatesArray = geometry.GetProperty("coordinates").EnumerateArray();

                    // Process each polygon in outer coordinates array
                    foreach (var polygonCoordinatesArray in outerCoordinatesArray)
                    {
                        // Process coordinates to polygon here
                        Debug.Print("Building polygon");
                        var enumerablePolygonCoordinatesArray = polygonCoordinatesArray.EnumerateArray();
                        var featurePolygon = CreatePolygonFromCoordinates(enumerablePolygonCoordinatesArray);

                        // Apply geometry to rowBuffer
                        rowBuffer["Shape"] = featurePolygon;
                        // Apply field to rowBuffer
                        if (attributeFields != null)
                        {
                            foreach(string field in attributeFields.Keys ?? new Dictionary<string, FieldType>().Keys)
                            {
                                // If specified field exists in feature's properties, add it to the rowbuffer
                                if (properties.TryGetProperty(field, out JsonElement value))
                                {
                                    // TODO refactor for non-string fields
                                    Debug.Print("Adding field");
                                    // ArcGIS replaces "." in fields with "_", so fix field name here
                                    string adjustedField = field.Replace('.', '_');
                                    rowBuffer[adjustedField] = value.ToString();
                                }
                            }
                        }
                        // Apply rowBuffer to feature class
                        insertCursor.Insert(rowBuffer);
                    }
                }
                // Save feature class/table changes
                insertCursor.Flush();

                Debug.Print("Finishing");
            });

        }

        private static Polygon CreatePolygonFromCoordinates(JsonElement.ArrayEnumerator coordinates)
        {
            List<MapPoint> pointList = new List<MapPoint>();

            // geojson coordinates structure is "coordinates":[[[x1,y1],[x2,y2], ... [xn, yn]]]
            foreach (var coordinate in coordinates)
            {
                // easting, northing
                MapPoint mapPoint = MapPointBuilder.CreateMapPoint(coordinate[0].GetDouble(), coordinate[1].GetDouble());
                pointList.Add(mapPoint);
            }

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
