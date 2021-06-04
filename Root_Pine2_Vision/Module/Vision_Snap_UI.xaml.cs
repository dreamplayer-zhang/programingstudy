﻿using Microsoft.Win32;
using RootTools;
using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;

namespace Root_Pine2_Vision.Module
{
    /// <summary>
    /// Vision_Snap_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Vision_Snap_UI : UserControl, ITool
    {
        public Vision_Snap_UI()
        {
            InitializeComponent();
        }

        Vision m_vision;
        Vision.SnapData m_snapData;
        public void Init(Vision vision)
        {
            m_vision = vision;
            m_snapData = new Vision.SnapData(vision); 
            DataContext = vision;
            InitTree();
        }

        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(m_vision.p_id, null);
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
            m_snapData.RunTree(m_treeRoot, true); 
        }
        #endregion

        const string c_sExt = "snap"; 
        private void buttonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            string sExt = "." + c_sExt;
            dlg.DefaultExt = sExt;
            dlg.Filter = "Snap Data File (*" + sExt + ")|*" + sExt;
            dlg.InitialDirectory = EQ.c_sPathRecipe;
            if (dlg.ShowDialog() == false) return;
            Job job = new Job(dlg.FileName, false, m_vision.m_log);
            m_treeRoot.m_job = job;
            m_treeRoot.p_eMode = Tree.eMode.JobOpen;
            m_snapData.RunTree(m_treeRoot, true); 
            job.Close();
            RunTree(Tree.eMode.Init); 
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            string sExt = "." + c_sExt;
            dlg.DefaultExt = sExt;
            dlg.Filter = "Snap Data File (*" + sExt + ")|*" + sExt;
            dlg.InitialDirectory = EQ.c_sPathRecipe;
            if (dlg.ShowDialog() == false) return;
            Job job = new Job(dlg.FileName, true, m_vision.m_log);
            m_treeRoot.m_job = job;
            m_treeRoot.p_eMode = Tree.eMode.JobSave;
            m_snapData.RunTree(m_treeRoot, true);
            job.Close();
        }

        #region ITool
        public string p_id
        {
            get { return m_vision.p_id + ".SnapData"; }
            set { }
        }

        public UserControl p_ui { get { return this; } }

        public void ThreadStop() { }
        #endregion

    }
}