using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.RTC5s
{
    /// <summary>
    /// RTC5_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RTC5_UI : UserControl
    {
        public RTC5_UI()
        {
            InitializeComponent();
        }

        RTC5 m_rtc5;
        public void Init(RTC5 rtc5)
        {
            m_rtc5 = rtc5;
            this.DataContext = rtc5;
            treeRootUI.Init(rtc5.m_treeRoot);
            rtc5.RunTree(Tree.eMode.Init);
            rtc5DesignUI.Init(rtc5.m_design);
        }

        private void buttonLaserOn_Click(object sender, RoutedEventArgs e)
        {
            m_rtc5.p_bLaserOn = !m_rtc5.p_bLaserOn;
        }

        private void buttonMarkClear_Click(object sender, RoutedEventArgs e)
        {
            m_rtc5.ClearMarkTest();
        }

        private void buttonMarkAdd_Click(object sender, RoutedEventArgs e)
        {
            m_rtc5.AddMarkTest();
        }

        private void buttonMark_Click(object sender, RoutedEventArgs e)
        {
            m_rtc5.MarkTest();
        }
    }
}
