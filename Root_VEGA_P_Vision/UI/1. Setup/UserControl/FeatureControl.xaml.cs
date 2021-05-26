using RootTools;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Root_VEGA_P_Vision
{
    /// <summary>
    /// FeatureControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FeatureControl : UserControl, ICloneable
    {
        public FeatureControl()
        {
            InitializeComponent();
        }
        private BitmapSource m_ImageSource;
        public BitmapSource p_ImageSource
        {
            get
            {
                return m_ImageSource;
            }
            set
            {
                image.Source = value;
                m_ImageSource = value;
            }
        }

        public object Clone()
        {
            FeatureControl fc = new FeatureControl();
            fc.p_ImageSource = this.m_ImageSource;
            return fc;
        }
    }
}
