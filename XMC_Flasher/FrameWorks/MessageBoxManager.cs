using System.Windows;

namespace XMC_Flasher.FrameWorks
{
    internal static class MessageBoxManager
    {
        public static void ShowError(string message)
        {
            MessageBox.Show(message,"Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
