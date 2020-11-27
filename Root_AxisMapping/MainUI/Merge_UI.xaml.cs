using RootTools.Trees;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Root_AxisMapping.MainUI
{
    /// <summary>
    /// Merge_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Merge_UI : UserControl
    {
        public Merge_UI()
        {
            InitializeComponent();
        }

        Merge m_merge;
        public void Init(Merge merge)
        {
            m_merge = merge;
            DataContext = merge;
            memoryViewerUI.Init(merge.m_axisMapping.m_memoryPoolMerge.m_viewer, false);
            treeUI.Init(merge.m_treeRoot);
            merge.RunTree(Tree.eMode.Init);
            InitBackgroundWorker(); 
        }

        #region Grab
        private void buttonGrab_Click(object sender, RoutedEventArgs e)
        {
            m_bgw.RunWorkerAsync(); 
        }

        BackgroundWorker m_bgw = new BackgroundWorker(); 
        void InitBackgroundWorker()
        {
            m_bgw.DoWork += M_bgw_DoWork;
            m_bgw.RunWorkerCompleted += M_bgw_RunWorkerCompleted;
        }

        private void M_bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            m_merge.Run(); 
        }

        private void M_bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }
        #endregion

        #region Inspect
        private void buttonInspect_Click(object sender, RoutedEventArgs e)
        {
            m_merge.Inspect(); 
        }
        #endregion
    }
}
