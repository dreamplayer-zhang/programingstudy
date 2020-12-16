using Root_EFEM.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Root_EFEM
{
    /// <summary>
    /// ListWafer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InfoWaferChild_UI : UserControl, ITool
    {
        public InfoWaferChild_UI()
        {
            InitializeComponent();
        }

        IWTRChild m_child; 
        public void Init(IWTRChild child)
        {
            m_child = child;
            DataContext = child;
            InitTree();
            InitTimer(); 
        }

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(m_child.p_id, null);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            treeRootUI.Init(m_treeRoot); 
            RunTree(Tree.eMode.Init); 
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
        }

        public void RunTree(Tree.eMode eMode)
        {
            m_treeRoot.p_eMode = eMode;
            if (m_child.p_infoWafer != null) m_child.p_infoWafer.RunTree(m_treeRoot.GetTree(m_child.p_infoWafer.p_id));
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();

        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (m_treeRoot.m_bFocus) return;
            RunTree(Tree.eMode.Init); 
        }
        #endregion

        #region Button
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            m_child.p_infoWafer = new InfoWafer(m_child.p_id, -1, ((ModuleBase)m_child).m_engineer); 
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            m_child.p_infoWafer = null; 
        }

        #endregion

        #region ITool
        public string p_id 
        { 
            get { return m_child.p_id + ".InfoWafer"; } 
            set { } 
        }

        public UserControl p_ui { get { return this; } }

        public void ThreadStop() { }
        #endregion
    }
}
