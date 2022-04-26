using System.Windows.Controls;
using Microsoft.Toolkit.Diagnostics;

namespace Rooler
{
    public class FlyoutTool : Tool
    {
        private Panel ParentPanel;

        public FlyoutTool(IScreenServiceHost host) : base(host) { }

        public void Show(Panel parentPanel)
        {
            Guard.IsNotNull(parentPanel, nameof(parentPanel));
            if (ParentPanel != parentPanel)
            {
                parentPanel.Children.Add(this.Visual);
                this.ParentPanel = parentPanel;
            }
        }

        protected override void OnClosing()
        {
            if (this.ParentPanel != null)
            {
                this.ParentPanel.Children.Remove(this);
                this.ParentPanel = null;
            }
        }
    }
}