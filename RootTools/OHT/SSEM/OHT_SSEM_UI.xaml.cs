using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.OHT.SSEM
{
    /// <summary>
    /// OHT_SSEM_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OHT_SSEM_UI : UserControl
    {
        public OHT_SSEM_UI()
        {
            InitializeComponent();
        }

        OHT_SSEM m_oht;
        public void Init(OHT_SSEM oht)
        {
            m_oht = oht;
            this.DataContext = oht;
            treeRootUI.Init(oht.m_treeRoot);
            oht.RunTree(Tree.eMode.Init);
            InitTimer();
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        public Queue<OHTBase.History> m_qHistory = new Queue<OHTBase.History>();
        OHTHistory_UI m_lastUI = null;
        private void M_timer_Tick(object sender, EventArgs e)
        {
            while (m_qHistory.Count > 0)
            {
                OHTBase.History history = m_qHistory.Dequeue();
                OHTHistory_UI ui = new OHTHistory_UI();
                ui.Init(history);
                if (stackHistory.Children.Count >= m_oht.m_lHistory) stackHistory.Children.RemoveAt(0);
                if (m_lastUI != null) m_lastUI.Width = 36;
                stackHistory.Children.Add(ui);
                m_lastUI = ui;
            }
        }
    }
}
