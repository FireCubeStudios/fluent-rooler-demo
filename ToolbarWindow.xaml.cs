using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rooler
{
    /// <summary>
    /// Interaction logic for ToolbarWindow.xaml
    /// </summary>
    public partial class ToolbarWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_EX_STYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;
        MainWindow MainWindow { get; }
        IntRect ScreenBounds = ScreenShot.FullScreenBounds;
        public ToolbarWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            Owner = mainWindow;
            Topmost = mainWindow.Topmost;
            ShowInTaskbar = mainWindow.ShowInTaskbar;
            MainWindow = mainWindow;
            Top = ScreenBounds.Top;
            Loaded += delegate
            {
                Left = (ScreenBounds.Width - ActualWidth) / 2;
            };
        }
        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
            var cr = new CornerRadius(8);
            if (Top <= 0)
            {
                cr.TopLeft = 0;
                cr.TopRight = 0;
            }
            if (Left <= 0)
            {
                cr.TopLeft = 0;
                cr.BottomLeft = 0;
            }
            if (Top + Height >= ScreenBounds.Bottom)
            {
                cr.BottomLeft = 0;
                cr.BottomRight = 0;
            }
            if (Left + Width >= ScreenBounds.Right)
            {
                cr.TopRight = 0;
                cr.BottomRight = 0;
            }
            Toolbar.CornerRadius = cr;
        }
        private void StartBounds(object sender, EventArgs e)
        {
            MakeNonClickThrough(MainWindow);
            MainWindow.CurrentService = new BoundsTool(MainWindow);
        }

        private void StartStretch(object sender, EventArgs e)
        {
            MakeNonClickThrough(MainWindow);
            MainWindow.CurrentService = new DistanceTool(StretchMode.NorthSouthEastWest, MainWindow);
        }

        private void StartStretchNS(object sender, EventArgs e)
        {
            MakeNonClickThrough(MainWindow);
            MainWindow.CurrentService = new DistanceTool(StretchMode.NorthSouth, MainWindow);
        }

        private void StartStretchEW(object sender, EventArgs e)
        {
            MakeNonClickThrough(MainWindow);
            MainWindow.CurrentService = new DistanceTool(StretchMode.EastWest, MainWindow);
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
        private void StartMagnify(object sender, EventArgs e)
        {
            Magnifier magnifierFlyout;
            if (!MainWindow.magnifier.TryGetTarget(out magnifierFlyout))
            {
                magnifierFlyout = new(MainWindow);
                MainWindow.magnifier.SetTarget(magnifierFlyout);
            }
            MakeNonClickThrough(MainWindow);
            magnifierFlyout.Show(MainWindow.ContentRoot);
        }

        public void OpenSettings(object sender, EventArgs e)
        {
            Settings settingsFlyout;
            if (!MainWindow.settings.TryGetTarget(out settingsFlyout))
            {
                settingsFlyout = new Settings(MainWindow);
                MainWindow.settings.SetTarget(settingsFlyout);
            }
            MakeNonClickThrough(MainWindow);
            settingsFlyout.Show(MainWindow.ContentRoot);
        }
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);
        public static void MakeClickThrough(Window win)
        {
            var hwnd = new WindowInteropHelper(win).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
        public static void MakeNonClickThrough(Window win)
        {
            var hwnd = new WindowInteropHelper(win).Handle;
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle & ~WS_EX_TRANSPARENT);
        }
        private void StopMagnify(object sender, EventArgs e)
        {
            if (MainWindow.magnifier.TryGetTarget(out var magnifierFlyout))
                magnifierFlyout.CloseService();
        }

        private void StartCapture(object sender, EventArgs e)
        {
            MainWindow.CurrentService = new Capture(MainWindow);
        }
        private void Close(object sender, EventArgs e)
        {
            this.Close();
            MainWindow.Close();
        }

        bool Collapsing = false;

        private void RequestClickThrough(object sender, RoutedEventArgs e)
        {
            MakeClickThrough(MainWindow);
        }

        private void CollapseClick(object sender, RoutedEventArgs e)
        {
            Collapsing = !Collapsing;
            CollapsableRegion.Visibility = Collapsing ? Visibility.Collapsed : Visibility.Visible;
            CollapseIcon.Glyph = Collapsing ? "\uE76C" : "\uE76B";
        }
    }
}
