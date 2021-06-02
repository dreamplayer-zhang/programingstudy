﻿using RootTools.Module;
using RootTools.Trees;
using System.Windows.Controls;
using System.Windows.Media;

namespace Root_Pine2.Module
{
    /// <summary>
    /// Loader_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Loader_UI : UserControl
    {
        public Loader_UI()
        {
            InitializeComponent();
        }

        dynamic m_loader;
        public void Init(dynamic loader)
        {
            m_loader = loader;
            DataContext = loader;
            treeRootUI.Init(loader.m_treeRootQueue);
            loader.RunTreeQueue(Tree.eMode.Init);
        }

        public void OnTimer()
        {
            switch (m_loader.p_eState)
            {
                case ModuleBase.eState.Init: Background = Brushes.AntiqueWhite; break;
                case ModuleBase.eState.Home: Background = Brushes.MediumPurple; break;
                case ModuleBase.eState.Ready: Background = Brushes.LightGreen; break;
                case ModuleBase.eState.Run: Background = Brushes.Yellow; break;
                case ModuleBase.eState.Error: Background = Brushes.OrangeRed; break;
            }
            textBlockStrip.Text = (m_loader.p_infoStrip != null) ? m_loader.p_infoStrip.p_id : ""; 
            OnRunTree();
        }

        int[] m_nQueue = new int[2] { 0, 0 };
        private void OnRunTree()
        {
            if ((m_loader.m_qModuleRun.Count == m_nQueue[0]) && (m_loader.m_qModuleRemote.Count == m_nQueue[1])) return;
            m_nQueue[0] = m_loader.m_qModuleRun.Count;
            m_nQueue[1] = m_loader.m_qModuleRemote.Count;
            m_loader.RunTreeQueue(Tree.eMode.Init);
        }
    }
}
