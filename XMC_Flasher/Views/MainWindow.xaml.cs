using System;
using System.Windows;
using System.Windows.Input;

namespace XMC_Flasher.Views
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
            ViewModelMain.ViewModelFlash.GetBoardFamilies();
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
            ViewModelMain.ViewModelFlash.GetBoardFamilies(); 
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
            ViewModelMain.ViewModelTest.ValidateBoardFirmware();
        }
    }
}
