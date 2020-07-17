using RootTools.Trees;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.Control.ACS
{
    /// <summary>
    /// ACS_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ACS_UI : UserControl
    {
        public ACS_UI()
        {
            InitializeComponent();
        }

        ACS m_acs; 
        public void Init(ACS acs)
        {
            m_acs = acs; 
            DataContext = acs;
            acsDIOUI.Init(acs.m_dio);
            acsListAxisUI.Init(acs.m_listAxis);
            treeRootUI.Init(acs.m_treeRoot);
            acs.RunTree(Tree.eMode.Init);
            InitTimer(); 
        }

        void InitBufferUI()
        {
            stackBuffer.Children.Clear(); 
            foreach (ACS.Buffer buffer in m_acs.m_aBuffer)
            {
                ACS_Buffer_UI ui = new ACS_Buffer_UI();
                ui.Init(buffer);
                stackBuffer.Children.Add(ui); 
            }
        }

        DispatcherTimer m_timer = new DispatcherTimer(); 
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (stackBuffer.Children.Count == m_acs.m_aBuffer.Count) return;
            InitBufferUI(); 
        }
    }
}
