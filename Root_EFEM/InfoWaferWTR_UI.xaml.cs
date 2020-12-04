using Root_EFEM.Module;
using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Root_EFEM
{
    /// <summary>
    /// InfoWaferWTR_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InfoWaferWTR_UI : UserControl, ITool
    {
        public InfoWaferWTR_UI()
        {
            InitializeComponent();
        }

        IEngineer m_engineer; 
        List<WTRArm> m_aArm; 
        public void Init(string id, List<WTRArm> aArm, IEngineer engineer)
        {
            p_id = id; 
            m_aArm = aArm;
            m_engineer = engineer; 
            InitComboBox(); 
            InitTree();
            InitTimer(); 
        }

        #region ITool
        public string p_id { get; set; }

        public UserControl p_ui { get { return this; } }

        public void ThreadStop() { }
        #endregion

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, null);
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
            foreach (WTRArm arm in m_aArm)
            {
                if (arm.p_infoWafer != null) arm.p_infoWafer.RunTree(m_treeRoot.GetTree(arm.p_infoWafer.p_id));
            }
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

        #region UI control
        List<string> m_asArm = new List<string>(); 
        void InitComboBox()
        {
            foreach (WTRArm arm in m_aArm) m_asArm.Add(arm.m_id);
            comboArm.ItemsSource = m_asArm;
            comboArm.SelectedIndex = 0; 
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            int iArm = comboArm.SelectedIndex;
            if ((iArm < 0) || (iArm >= m_aArm.Count)) return;
            m_aArm[iArm].p_infoWafer = new InfoWafer(m_aArm[iArm].m_id + ".Recover", m_engineer); 
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            int iArm = comboArm.SelectedIndex;
            if ((iArm < 0) || (iArm >= m_aArm.Count)) return;
            m_aArm[iArm].p_infoWafer = null; 
        }
        #endregion
    }
}
