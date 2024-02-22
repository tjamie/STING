using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Framework;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Core.Data;
using System.Diagnostics;
using System.Windows.Documents;
using STING.Structs;
using ArcGIS.Core.Geometry;

namespace STING.Prompts
{
    internal class FeatureSelector
    {
        // Select layer from the active map -- this will be used to derive extent coordinates for government API calls
        internal async Task<BoxCoordinates> PromptForFeatureExtent()
        {
            try
            {
                var activeMap = MapView.Active.Map;

                //string selectedFeaturePath = null;
                Item selectedFeature = null;

                // Create dialog to prompt user for relevant layer
                var openItemDialog = new OpenItemDialog
                {
                    Title = "Select Boundary Feature",
                    //InitialLocation = activeMap,
                    MultiSelect = false,
                    Filter = ItemFilters.FeatureClasses_All
                };

                // show dialog -- must be invoked with current UI thread
                await FrameworkApplication.Current.Dispatcher.InvokeAsync(() =>
                {
                    bool? result = openItemDialog.ShowDialog();

                    if (result == true)
                    {
                        // Get selected layer
                        var selectedItems = openItemDialog.Items;

                        // Continue only if exactly 1 item is selected
                        if (selectedItems.Count == 1)
                        {
                            //selectedFeaturePath = selectedItems[0].Path;
                            selectedFeature = selectedItems[0];
                        }
                        else
                        {
                            MessageBox.Show("No more than one layer can be selected.", "Error");
                        }
                    }
                });

                // Returns a box of 0 area if selectedFeature is null
                if (selectedFeature != null)
                {
                    BoxCoordinates boxCoordinates = await CalculateExtent(selectedFeature);
                    return boxCoordinates;
                }
                return new BoxCoordinates(Coordinates.Zero, Coordinates.Zero);
            }
            catch(Exception ex)
            {
                // No active map
                MessageBox.Show($"Exception: {ex.Message}", "Error");
                return new BoxCoordinates(Coordinates.Zero, Coordinates.Zero);
            }
        }

        private async Task<BoxCoordinates> CalculateExtent(Item feature)
        {
            // Why can't Esri make this simple
            float xMin = 0f, xMax = 0f, yMin = 0f, yMax = 0f;

            await QueuedTask.Run(() =>
            {
                // Set gdb object to feature's parent directory
                // TODO Just letting this throw an exception for now if target item isn't in a .gdb as required by new Geodatabase()
                Geodatabase gdb = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(System.IO.Path.GetDirectoryName(feature.Path))));

                // Load feature
                FeatureClass fc = gdb.OpenDataset<FeatureClass>(feature.Name);

                // Get envelope
                Envelope extent = fc.GetExtent();

                // Convert to WGS84 (EPSG 4326) if feature class is not already in that coordinate system
                if (extent.SpatialReference.Wkid != 4326)
                {
                    Geometry projectedGeometry = GeometryEngine.Instance.Project(extent, SpatialReferences.WGS84);
                    extent = projectedGeometry.Extent;
                }
                

                // Get envelope
                xMin = (float)extent.XMin;
                xMax = (float)extent.XMax;
                yMin = (float)extent.YMin;
                yMax = (float)extent.YMax;

                // Southwest, Northeast
            });
            return new BoxCoordinates(new Coordinates(xMin, yMin), new Coordinates(xMax, yMax));
        }
    }
}