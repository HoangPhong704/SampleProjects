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
using System.Windows.Shapes;

namespace XMC_Flasher.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModelSettings.IsInitialized) return;
            ViewModelSettings.OnSettingsSaved += ViewModelSettings_OnSettingsSaved;
            ViewModelSettings.GetSettings();
            ViewModelSettings.IsInitialized = true;
        }

        private void ViewModelSettings_OnSettingsSaved(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}
