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

namespace STING.Prompts
{
    internal class FeatureSelector
    {
        // Select layer from the active map -- this will be used to derive extent coordinates for government API calls
        internal async Task<string> PromptForLayer()
        {
            try
            {
                var activeMap = MapView.Active.Map;

                //if (activeMap != null)
                //{
                string selectedFeaturePath = null;

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
                            selectedFeaturePath = selectedItems[0].Path;
                        }
                        else
                        {
                            MessageBox.Show("No more than one layer can be selected.", "Error");
                        }
                    }
                });

                return selectedFeaturePath;
            }
            catch(Exception ex)
            {
                // No active map
                MessageBox.Show($"No active map found.\n(Exception: {ex.Message})", "Error");
                return null;
            }
        }
    }
}
