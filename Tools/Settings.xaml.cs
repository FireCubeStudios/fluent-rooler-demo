using System;
using System.Windows.Input;

namespace Rooler
{
    public partial class Settings : FlyoutTool
    {
        public Settings(IScreenServiceHost host) : base(host)
        {
            this.InitializeComponent();
            new Dragger(this);

            this.Focusable = true;

            this.ToleranceSlider.Value = ScreenCoordinates.ColorTolerance;

            this.ToleranceSlider.ValueChanged += delegate
            {
                ScreenCoordinates.ColorTolerance = this.ToleranceSlider.Value;
            };
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            this.ToleranceSlider.Value += e.Delta * .05;

            /*	if (.CurrentService != null)
                    this.CurrentService.Update();*/
        }

        private void CloseSettings(object sender, EventArgs e)
        {
            this.CloseService();
        }
    }
}