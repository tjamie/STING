using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using STING.DataConversions;
using STING.Prompts;
using STING.Services;
using STING.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STING
{
    internal class NwiButton : Button
    {
        protected override async void OnClick()
        {
            // TODO apply standardized symbology to the resulting layer

            string testMsg = "NWI button clicked";
            Debug.Print(testMsg);

            // Get timestamp and apply to feature class name
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string newClassName = $"NWI_{timestamp}";

            // Placeholder -- eventually have user select gdb
            var gdb_path = CoreModule.CurrentProject.DefaultGeodatabasePath;

            // Get user to select envelope/boundary feature
            var featureSelector = new FeatureSelector();
            BoxCoordinates featureBoxCoordinates = await featureSelector.PromptForFeatureExtent();

            // Only continue if a valid extent has been selected
            if (featureBoxCoordinates.IsNonzero())
            {
                // Start new NRCS service
                Debug.Print("Starting NWI Service");
                var nwiService = new NwiService();
                var geojsonResponse = await nwiService.GetWetlandFeatures(featureBoxCoordinates);

                // Only continue if response not null
                if (geojsonResponse != null)
                {
                    // Define attribute fields to take from geojson
                    var attributeFields = new Dictionary<string, FieldType>
                    {
                        { @"Wetlands.ATTRIBUTE", FieldType.String },
                        { @"Wetlands.WETLAND_TYPE", FieldType.String }
                    };
                    // Attempt conversion
                    Debug.Print("Converting geojson");
                    var geojsonConverter = new GeojsonConverter();
                    geojsonConverter.ConvertToFeatureClass(geojsonResponse, gdb_path, newClassName, attributeFields);
                }
            }
            else
            {
                await QueuedTask.Run(() =>
                {
                    MessageBox.Show("Invalid extent.", "Error");
                });
            }
        }
    }
}
