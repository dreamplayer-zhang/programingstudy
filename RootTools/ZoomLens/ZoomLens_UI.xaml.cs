using RootTools.Trees;
using System;
using System.Windows.Controls;

namespace RootTools.ZoomLens
{
    /// <summary>
    /// ZoomLens_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ZoomLens_UI : UserControl
    {
        public ZoomLens_UI()
        {
            InitializeComponent();
        }

        ZoomLens m_ZoomLens;
        public void Init(ZoomLens zoomLens)
        {
            m_ZoomLens = zoomLens;
            this.DataContext = zoomLens;
            rs232UI.Init(zoomLens.m_rs232);
        }
    }
}
