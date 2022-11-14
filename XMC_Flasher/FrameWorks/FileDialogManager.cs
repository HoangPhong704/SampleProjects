using System.Windows.Forms;

namespace XMC_Flasher.FrameWorks
{
    internal class FileDialogManager
    {
        public (bool IsSelected, string SelectedDirectory) BrowseDirectory()
        {
            using (var dialogBrowser = new FolderBrowserDialog())
            {
                DialogResult result = dialogBrowser.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialogBrowser.SelectedPath))
                {
                    return (true, dialogBrowser.SelectedPath);
                }
                else
                {
                    return (false, "");
                }
            }
        }
    }
}
