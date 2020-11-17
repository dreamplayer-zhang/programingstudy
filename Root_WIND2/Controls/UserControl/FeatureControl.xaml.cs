using RootTools;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Root_WIND2
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
        private CPoint m_Offset = new CPoint();
        public CPoint p_Offset
        {
            get
            {
                return m_Offset;
            }
            set
            {
                offset.Text = value.ToString();
                m_Offset = value;
            }
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
            fc.p_Offset = this.p_Offset;
            fc.p_ImageSource = this.m_ImageSource;
            return fc;
        }
    }
}
