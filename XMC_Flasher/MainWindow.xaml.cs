using XMC_Flasher.FrameWorks;
using XMC_Flasher.Views;
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

namespace XMC_Flasher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModelMain.IsInitialized) return;
            ViewModelMain.OnEditSettingsRequested += ViewModelMain_OnEditSettingsRequested;
            ViewModelMain.OnCloseApplicationRequested += ViewModelMain_OnCloseApplicationRequested;
            ViewModelMain.GetBoardFamilies();
            ViewModelMain.IsInitialized = true; 
        }

        private void ViewModelMain_OnCloseApplicationRequested(object? sender, EventArgs e)
        {
            this.Close();  
        }

        private void ViewModelMain_OnEditSettingsRequested(object? sender, EventArgs e)
        {
            var settingsWindow = new SettingsWindow() { WindowStartupLocation = WindowStartupLocation.CenterOwner, Owner = App.Current.MainWindow};
            settingsWindow.ViewModelSettings.OnSettingsSaved += ViewModelSettings_OnSettingsSaved;
            settingsWindow.ShowDialog();
            settingsWindow.ViewModelSettings.OnSettingsSaved -= ViewModelSettings_OnSettingsSaved;
        }

        private void ViewModelSettings_OnSettingsSaved(object? sender, EventArgs e)
        { 
            ViewModelMain.GetBoardFamilies(); 
        }

        private void OnWindowDragged(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
         
        private void OnWindowClosed(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ViewModelMain.ValidateBoardFirmware();
        }
    }
}
