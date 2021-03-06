//      *********    DO NOT MODIFY THIS FILE     *********
//      This file is regenerated by a design tool. Making 
//      changes to this file can cause errors.
namespace Expression.Blend.SampleData.BoundsData
{
    public class BoundsData : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        public BoundsData()
        {
            try
            {
                System.Uri resourceUri = new System.Uri("/Rooler;component/SampleData/BoundsData/BoundsData.xaml", System.UriKind.Relative);
                if (System.Windows.Application.GetResourceStream(resourceUri) != null)
                {
                    System.Windows.Application.LoadComponent(this, resourceUri);
                }
            }
            catch (System.Exception)
            { }
        }

        private double _Left = 0;
        public double Left
        {
            get
            {
                return this._Left;
            }
            set
            {
                if (this._Left != value)
                {
                    this._Left = value;
                    this.OnPropertyChanged("Left");
                }
            }
        }

        private double _Top = 0;
        public double Top
        {
            get
            {
                return this._Top;
            }
            set
            {
                if (this._Top != value)
                {
                    this._Top = value;
                    this.OnPropertyChanged("Top");
                }
            }
        }
    }
}
