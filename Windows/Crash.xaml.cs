using System.Windows;

namespace Rooler
{
    /// <summary>
    /// Interaction logic for Crash.xaml
    /// </summary>
    public partial class Crash : Window
    {
        public Crash(string text)
        {
            InitializeComponent();

            this.TB.Text = text;

        }
    }
}
