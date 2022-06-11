using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace Rooler
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IScreenServiceHost
    {
        private IScreenService currentService = null;

        private List<IScreenService> currentServices = new List<IScreenService>();

        public readonly WeakReference<Magnifier> magnifier = new(null);

        public readonly WeakReference<Settings> settings = new(null);

        [DllImport("User32.dll")]
        private static extern int SetWindowBand(IntPtr hWnd, IntPtr hwndInsertAfter, uint dwBand);

        public MainWindow()
        {

            this.InitializeComponent();

#if !DEBUG
			// Makes it a royal pain to debug...
			this.Topmost = true;
			this.ShowInTaskbar = false;
			//this.CaptureButton.Visibility = Visibility.Collapsed;
#endif

            Loaded += delegate
            {
                Toolbar.Visibility = Visibility.Hidden;
                new ToolbarWindow(this).Show();
                new Dragger(this.Toolbar);
            };
            // very painfull to debug but i have two monitors :cool:
            this.Topmost = true;
            this.DataContext = this;

            IntRect screenBounds = ScreenShot.FullScreenBounds;
            this.Top = screenBounds.Top;
            this.Left = screenBounds.Left;
            this.Width = screenBounds.Width;
            this.Height = screenBounds.Height;
        }

        private bool preserveAnnotations;
        public bool PreserveAnnotations
        {
            get { return this.preserveAnnotations; }
            set
            {
                this.preserveAnnotations = value;
                this.OnPropertyChanged("PreserveAnnotations");
            }
        }

        protected override void OnDeactivated(EventArgs e)
        {

            this.CurrentService = null;


            base.OnDeactivated(e);
        }

        private void StartBounds(object sender, EventArgs e)
        {
            this.CurrentService = new BoundsTool(this);
        }

        private void StartStretch(object sender, EventArgs e)
        {
            this.CurrentService = new DistanceTool(StretchMode.NorthSouthEastWest, this);
        }

        private void StartStretchNS(object sender, EventArgs e)
        {
            this.CurrentService = new DistanceTool(StretchMode.NorthSouth, this);
        }

        private void StartStretchEW(object sender, EventArgs e)
        {
            this.CurrentService = new DistanceTool(StretchMode.EastWest, this);
        }

        private void StartMagnify(object sender, EventArgs e)
        {
            Magnifier magnifierFlyout;
            if (!this.magnifier.TryGetTarget(out magnifierFlyout))
            {
                magnifierFlyout = new(this);
                this.magnifier.SetTarget(magnifierFlyout);
            }

            magnifierFlyout.Show(this.ContentRoot);
        }

        public void OpenSettings(object sender, EventArgs e)
        {
            Settings settingsFlyout;
            if (!this.settings.TryGetTarget(out settingsFlyout))
            {
                settingsFlyout = new Settings(this);
                this.settings.SetTarget(settingsFlyout);
            }

            settingsFlyout.Show(this.ContentRoot);
        }

        private void StopMagnify(object sender, EventArgs e)
        {
            if (magnifier.TryGetTarget(out var magnifierFlyout))
                magnifierFlyout.CloseService();
        }

        private void StartCapture(object sender, EventArgs e)
        {
            this.CurrentService = new Capture(this);
        }

        public IScreenService CurrentService
        {
            get { return this.currentService; }
            set
            {
                if (this.currentService != null)
                {
                    if (!this.PreserveAnnotations && !this.currentService.IsFrozen)
                    {
                        this.currentService.CloseService();
                        Debug.Assert(this.currentService == null);
                    }
                }

                this.currentService = value;

                if (this.currentService != null)
                {
                    this.currentServices.Add(this.currentService);
                    this.ContentRoot.Children.Add(this.currentService.Visual);
                    this.currentService.ServiceClosed += this.ServiceClosed;
                }
            }
        }

        private IScreenShot lastScreenshot;

        public IScreenShot CurrentScreen
        {
            get
            {
                if (this.currentServices.Count == 0)
                {
                    this.lastScreenshot = new VirtualizedScreenShot();
                    //this.lastScreenshot = new ScreenShot();
                }
                return this.lastScreenshot;
            }
        }

        private void ServiceClosed(object sender, EventArgs e)
        {

            IScreenService service = (IScreenService)sender;

            service.ServiceClosed -= this.ServiceClosed;

            this.currentServices.Remove(service);
            this.ContentRoot.Children.Remove(service.Visual);
            //service.Visual.Close();

            if (service == this.currentService)
                this.currentService = null;

            foreach (UIElement child in this.Toggles.Children)
            {
                ToggleButton tb = child as ToggleButton;
                if (tb != null)
                    tb.IsChecked = false;
            }
        }

        private void Close(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            //this.DragMove();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                this.CloseAllServices();
            }
        }

        private void CloseAllServices()
        {
            List<IScreenService> currentServices = new List<IScreenService>(this.currentServices);
            foreach (IScreenService service in currentServices)
            {
                service.CloseService();
            }

            StopMagnify(null, EventArgs.Empty);

            if (settings.TryGetTarget(out var settingsFlyout))
                settingsFlyout.CloseService();

            Debug.Assert(this.ContentRoot.Children.Count == 0);
            Debug.Assert(this.currentServices.Count == 0);
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            Debug.Assert(this.GetType().GetProperty(propertyName) != null);
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        bool Collapsing = false;
        private void COllapseClick(object sender, RoutedEventArgs e)
        {
            Collapsing = !Collapsing;
            CollapsableRegion.Visibility = Collapsing ? Visibility.Collapsed : Visibility.Visible;
            CollapseIcon.Glyph = Collapsing ? "\uE76C" : "\uE76B";
        }
    }
}
