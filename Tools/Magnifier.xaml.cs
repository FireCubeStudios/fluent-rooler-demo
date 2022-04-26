using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Rooler
{
    public partial class Magnifier : FlyoutTool
    {

        private bool isPaused = false;

        public static RoutedCommand CopyCommand = new RoutedCommand("Copy", typeof(Magnifier));
        public static RoutedCommand SetBasePointCommand = new RoutedCommand("SetBasePoint", typeof(Magnifier));

        private DateTime lastCapture = DateTime.Today;
        private IntPoint lastMousePoint = new IntPoint();
        private IntPoint basePoint = new IntPoint();

        public Magnifier(IScreenServiceHost host) : base(host)
        {
            this.InitializeComponent();

            new Dragger(this);

            this.Focusable = true;

            this.Scale = 8;

            DispatcherTimer timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(.03),
                IsEnabled = true,
            };
            timer.Tick += this.HandleTick;

            this.Loaded += delegate
            {
                this.Focus();
            };

            this.InputBindings.Add(new InputBinding(Magnifier.CopyCommand, new KeyGesture(Key.C, ModifierKeys.Control)));
            this.CommandBindings.Add(new CommandBinding(Magnifier.CopyCommand, this.CopyCommandExecuted));

            this.InputBindings.Add(new InputBinding(Magnifier.SetBasePointCommand, new KeyGesture(Key.Enter)));
            this.CommandBindings.Add(new CommandBinding(Magnifier.SetBasePointCommand, this.SetBasePointExecuted));
        }

        private void CloseMagnifier(object sender, EventArgs e)
        {
            this.CloseService();
        }


        private void CopyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetText(this.PixelColor.Text);
        }

        private void SetBasePointExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.basePoint = NativeMethods.GetCursorPos();
        }

        public double Scale { get; set; }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {

            if (e.Delta > 0)
                this.Scale += 1;
            else
                this.Scale = Math.Max(1, this.Scale - 1);

            e.Handled = true;
            base.OnMouseWheel(e);
        }

        private void HandleTick(object sender, EventArgs e)
        {

            IntPoint mousePoint = NativeMethods.GetCursorPos();

            if (mousePoint == this.lastMousePoint && DateTime.Now - this.lastCapture < TimeSpan.FromSeconds(.2))
            {
                return;
            }

            this.lastCapture = DateTime.Now;
            this.lastMousePoint = mousePoint;

            this.MouseX.Text = string.Format(@"X: {0}", mousePoint.X - this.basePoint.X);
            this.MouseY.Text = string.Format(@"Y: {0}", mousePoint.Y - this.basePoint.Y);

            if (this.isPaused)
                return;


            double width = this.Image.ActualWidth / this.Scale;
            double height = this.Image.ActualHeight / this.Scale;



            double left = (mousePoint.X - width / 2).Clamp(ScreenShot.FullScreenBounds.Left, ScreenShot.FullScreenBounds.Width - width);
            double top = (mousePoint.Y - height / 2).Clamp(ScreenShot.FullScreenBounds.Top, ScreenShot.FullScreenBounds.Height - height);


            double deltaX = left - (mousePoint.X - width / 2);
            double deltaY = top - (mousePoint.Y - height / 2);

            if (deltaX != 0)
                this.CenterX.Width = new GridLength((this.Image.ActualWidth / 2 - deltaX * this.Scale) + 2 * this.Scale);
            else
                this.CenterX.Width = new GridLength(this.Image.ActualWidth / 2 + 8);

            if (deltaY != 0)
                this.CenterY.Height = new GridLength((this.Image.ActualHeight / 2 - deltaY * this.Scale) + 2 * this.Scale);
            else
                this.CenterY.Height = new GridLength(this.Image.ActualHeight / 2 + 8);



            IntRect rect = new IntRect((int)left, (int)top, (int)width, (int)height);

            ScreenShot screenShot = new ScreenShot(rect);

            FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            newFormatedBitmapSource.BeginInit();
            newFormatedBitmapSource.Source = screenShot.Image;
            newFormatedBitmapSource.DestinationFormat = PixelFormats.Rgb24;
            newFormatedBitmapSource.EndInit();

            this.Image.Source = newFormatedBitmapSource;

            if (width == 0 || height == 0)
                return;

            uint centerPixel = (uint)screenShot.GetScreenPixel((int)mousePoint.X, (int)mousePoint.Y);
            centerPixel = centerPixel | 0xFF000000;
            byte r = (byte)((centerPixel >> 16) & 0xFF);
            byte g = (byte)((centerPixel >> 8) & 0xFF);
            byte b = (byte)((centerPixel >> 0) & 0xFF);

            Brush brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            this.ColorSwatch.Fill = brush;
            uint centerPixelX = (uint)screenShot.GetScreenPixel((int)mousePoint.X + 90, (int)mousePoint.Y + 90);
            centerPixel = centerPixelX | 0xFF000000;
            byte rX = (byte)((centerPixelX >> 16) & 0xFF);
            byte gX = (byte)((centerPixelX >> 8) & 0xFF);
            byte bX = (byte)((centerPixelX >> 0) & 0xFF);
            Brush brushX = new SolidColorBrush(Color.FromRgb(rX, gX, bX));
            this.ColorSwatchX.Fill = brushX;
            uint centerPixelY = (uint)screenShot.GetScreenPixel((int)mousePoint.X - 90, (int)mousePoint.Y - 90);
            centerPixel = centerPixelY | 0xFF000000;
            byte rY = (byte)((centerPixelY >> 16) & 0xFF);
            byte gY = (byte)((centerPixelY >> 8) & 0xFF);
            byte bY = (byte)((centerPixelY >> 0) & 0xFF);
            Brush brushY = new SolidColorBrush(Color.FromRgb(rY, gY, bY));
            this.ColorSwatchY.Fill = brushY;
            this.PixelColor.Text = "HEX: " + string.Format(@"#{0:X8}", centerPixel);
            PixelRGBColor.Text = "RGB: " + r + ", " + g + ", " + b;
            float _R = (r / 255f);
            float _G = (g / 255f);
            float _B = (b / 255f);

            float _Min = Math.Min(Math.Min(_R, _G), _B);
            float _Max = Math.Max(Math.Max(_R, _G), _B);
            float _Delta = _Max - _Min;

            float H = 0;
            float S = 0;
            float L = (float)((_Max + _Min) / 2.0f);

            if (_Delta != 0)
            {
                if (L < 0.5f)
                {
                    S = (float)(_Delta / (_Max + _Min));
                }
                else
                {
                    S = (float)(_Delta / (2.0f - _Max - _Min));
                }


                if (_R == _Max)
                {
                    H = (_G - _B) / _Delta;
                }
                else if (_G == _Max)
                {
                    H = 2f + (_B - _R) / _Delta;
                }
                else if (_B == _Max)
                {
                    H = 4f + (_R - _G) / _Delta;
                }
            }
            PixelHSLColor.Text = "HSL: " + Math.Round(H * 100) + ", " + Math.Round(S * 100).ToString() + "%, " + Math.Round(L * 100).ToString() + "%";
        }
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            //this.isPaused = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            //this.isPaused = false;
        }
    }
}
