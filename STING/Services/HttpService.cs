using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using STING.Windows;

namespace STING.Services
{
    internal class HttpService
    {
        readonly HttpClient httpClient = new();
        internal async Task<string> GetResponse(string requestUrl, string sourceEntityName)
        {
            // Display loading window
            LoadingWindow loadingWindow = new LoadingWindow();
            loadingWindow.Owner = FrameworkApplication.Current.MainWindow;
            loadingWindow.Title = $"Fetching Data - {sourceEntityName}";
            await FrameworkApplication.Current.Dispatcher.InvokeAsync(() =>
            {
                loadingWindow.Show();
            });

            try
            {
                using HttpResponseMessage response = await httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                // Get content length to calculate progress
                long contentLength = response.Content.Headers.ContentLength ?? -1;
                long bytesRead = 0;

                // Create a buffer to read response content
                byte[] buffer = new byte[8192];
                using (Stream stream = await response.Content.ReadAsStreamAsync())
                {
                    int bytesReadThisChunk;
                    await FrameworkApplication.Current.Dispatcher.InvokeAsync(() =>
                    {
                        loadingWindow.UpdateText();
                    });

                    do
                    {
                        bytesReadThisChunk = await stream.ReadAsync(buffer, 0, buffer.Length);
                        bytesRead += bytesReadThisChunk;

                        // Update progress
                        if (contentLength > 0)
                        {
                            double progress = (double)bytesRead / contentLength;
                            await FrameworkApplication.Current.Dispatcher.InvokeAsync(() =>
                            {
                                loadingWindow.UpdateProgress(progress);
                            });
                        }
                    } while (bytesReadThisChunk > 0);
                }

                loadingWindow.Close();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                string exMessage = $"Exception thrown while attemping to connect to {requestUrl}:\n{ex.Message}";
                loadingWindow.Close();
                MessageBox.Show(exMessage, "Connection Error");
                return null;
            }
        }
    }
}
