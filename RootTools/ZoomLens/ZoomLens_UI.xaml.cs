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
            m_ZoomLens.m_rs232.p_bConnect = true;
            rs232UI.Init(zoomLens.m_rs232);
        }

        private new void PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Name == "btnUp")
            {
                m_ZoomLens.JogMove(false);
                m_ZoomLens.GetStatus(1);
            }
            else if (btn.Name == "btnDown")
            {
                m_ZoomLens.JogMove(true);
                m_ZoomLens.GetStatus(1);
            }
        }

        private new void PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            m_ZoomLens.Stop();
            m_ZoomLens.GetStatus(1);
        }
    }
}
