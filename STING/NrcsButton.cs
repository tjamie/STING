using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using STING.DataConversions;
using STING.Services;
using STING.Structs;
using STING.Prompts;
using System.Diagnostics;
using System;

namespace STING
{
    internal class NrcsButton : Button
    {
        protected override async void OnClick()
        {
            // TODO apply standardized symbology to the resulting layer

            string testMsg = "NRCS button clicked";
            Debug.Print(testMsg);

            // Get timestamp and apply to feature class name
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string newClassName = $"NRCS_{timestamp}";

            // Placeholder -- eventually have user select gdb
            var gdb_path = CoreModule.CurrentProject.DefaultGeodatabasePath;

            // Get user to select envelope/boundary feature
            var featureSelector = new FeatureSelector();
            BoxCoordinates featureBoxCoordinates = await featureSelector.PromptForFeatureExtent();

            // Only continue if a valid extent has been selected
            if (featureBoxCoordinates.IsNonzero())
            {
                // Start new NRCS service
                Debug.Print("Starting NRCS Service");
                var nrcsService = new NrcsService();
                var xmlResponse = await nrcsService.GetSoilFeatures(featureBoxCoordinates);

                if (xmlResponse != null)
                {
                    // Attempt conversion
                    Debug.Print("Converting GML");
                    var gmlConverter = new GmlConverter();
                    gmlConverter.ConvertToFeatureClass(xmlResponse, gdb_path, newClassName);
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
