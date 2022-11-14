using System.Windows.Input;

namespace XMC_Flasher.FrameWorks
{
    public static class MouseManager
    { 
        public static Cursor? LastCursor { get; private set; } 

        public static void SetBusy()
        {
            LastCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = Cursors.Wait;
        }

        public static void SetLastCursor()
        {
            Mouse.OverrideCursor = LastCursor;
        }
    }
}
