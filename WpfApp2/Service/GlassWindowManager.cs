namespace WpfApp2;

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public class GlassWindowManager
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    public static void EnableGlassEffect(Window window)
    {
        if (!IsDwmEnabled())
            return;

        window.SourceInitialized += (sender, e) =>
        {
            var hWnd = new WindowInteropHelper(window).Handle;
            SetWindowAttributes(hWnd);
        };
    }
    
    
    

    private static void SetWindowAttributes(IntPtr hwnd)
    {
        int value = 2; // DWMNCRP_ENABLED
        DwmSetWindowAttribute(hwnd, 2, ref value, sizeof(int));

        MARGINS margins = new MARGINS() { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref margins);
    }

    private static bool IsDwmEnabled()
    {
        // This function can check if DWM is enabled on the system
        // For simplicity, assuming it's always enabled on supported systems
        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }
}
