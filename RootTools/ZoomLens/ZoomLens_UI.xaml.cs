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

        private void HomeButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            m_ZoomLens.Home();
        }

        private void StopButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            m_ZoomLens.Stop();
        }

        private void ResetButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            m_ZoomLens.Reset();
        }

        private void EmergencyStopButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            m_ZoomLens.EmergencyStop();
        }

        private void MoveButtonClick(object sender, System.Windows.RoutedEventArgs e)
        {
            int nMovePos = int.Parse(tbxMovePos.Text);
            m_ZoomLens.AbsoluteGo(nMovePos);
        }

        private void PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Button btn = (Button)sender;
            if (btn.Name == "btnUp")
            {
                m_ZoomLens.JogMove(false);
            }
            else if (btn.Name == "btnDown")
            {
                m_ZoomLens.JogMove(true);
            }
            Console.WriteLine("PreviewMouseLeftButtonDown");
        }

        private void PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            m_ZoomLens.Stop();
            Console.WriteLine("PreviewMouseLeftButtonUp");
        }
    }
}
