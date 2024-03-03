using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STING.Windows
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
    {
        public LoadingWindow()
        {
            InitializeComponent();
        }

        public void UpdateProgress(double value)
        {
            progressBar.Visibility = Visibility.Visible;
            progressBar.IsIndeterminate = false;
            progressBar.Value = value * 100;
            progressBar.UpdateLayout();
        }

        public void UpdateText()
        {
            infoText.Text = "Downloading data...";
            infoText.UpdateLayout();
        }
    }
}
