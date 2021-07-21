﻿using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_Pine2.Module
{
    /// <summary>
    /// Boats_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Boats_UI : UserControl
    {
        public Boats_UI()
        {
            InitializeComponent();
        }

        Boats m_boats;
        Pine2 m_pine2; 
        public void Init(Boats boats, Pine2 pine2)
        {
            m_boats = boats;
            m_pine2 = pine2; 
            DataContext = boats;
            treeRootUI.Init(boats.m_treeRootQueue);
            treeVisionUI.Init(boats.m_vision.p_treeRootQueue);
            textBlockInfo.DataContext = boats; 
            boats.RunTreeQueue(Tree.eMode.Init);
        }

        public void OnTimer()
        {
            switch (m_boats.p_eState)
            {
                case ModuleBase.eState.Init: Background = Brushes.White; break;
                case ModuleBase.eState.Home: Background = Brushes.MediumPurple; break;
                case ModuleBase.eState.Ready: Background = Brushes.LightGreen; break;
                case ModuleBase.eState.Run: Background = Brushes.Yellow; break;
                case ModuleBase.eState.Error: Background = Brushes.OrangeRed; break;
            }
            textBlockVision.Foreground = m_boats.m_vision.p_remote.p_bEnable ? Brushes.Red : Brushes.LightGray;
            if ((m_pine2.p_b3D ==false) && (m_boats.m_vision.p_eVision != eVision.Bottom))
            {
                checkBoxBlow.IsEnabled = false; 
                checkBoxRoller.IsChecked = false; 
            }
            textBlockA.Text = (m_boats.m_aBoat[eWorks.A].p_infoStrip != null) ? m_boats.m_aBoat[eWorks.A].p_infoStrip.p_id : "";
            textBlockB.Text = (m_boats.m_aBoat[eWorks.B].p_infoStrip != null) ? m_boats.m_aBoat[eWorks.B].p_infoStrip.p_id : "";
            gridStripA.Background = (m_boats.m_aBoat[eWorks.A].p_inspectStrip != null) ? Brushes.Orange : Brushes.Beige;
            gridStripB.Background = (m_boats.m_aBoat[eWorks.B].p_inspectStrip != null) ? Brushes.Orange : Brushes.Beige;
            gridA.Background = (m_boats.m_aBoat[eWorks.A].p_bWorksConnect ? Brushes.AliceBlue : Brushes.Purple);
            gridB.Background = (m_boats.m_aBoat[eWorks.B].p_bWorksConnect ? Brushes.AliceBlue : Brushes.Purple);
            textBlockStepA.Text = m_boats.m_aBoat[eWorks.A].p_eStep.ToString();
            textBlockStepB.Text = m_boats.m_aBoat[eWorks.B].p_eStep.ToString();
            OnRunTree();

            if (m_boats.m_vision.p_remote.m_client.p_bConnect == false)
            {
                m_boats.m_aBoat[eWorks.A].p_bWorksConnect = false;
                m_boats.m_aBoat[eWorks.B].p_bWorksConnect = false;
            }
        }

        int[] m_nQueue = new int[2] { 0, 0 };
        private void OnRunTree()
        {
            if ((m_boats.m_qModuleRun.Count == m_nQueue[0]) && (m_boats.m_qModuleRemote.Count == m_nQueue[1])) return;
            m_nQueue[0] = m_boats.m_qModuleRun.Count;
            m_nQueue[1] = m_boats.m_qModuleRemote.Count;
            m_boats.RunTreeQueue(Tree.eMode.Init);
        }

        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            m_boats.m_vision.p_remote.p_bEnable = !m_boats.m_vision.p_remote.p_bEnable;
        }

        private void GridA_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (EQ.p_eState != EQ.eState.Ready) return;
            if (m_boats.p_eState != ModuleBase.eState.Ready) return;
            m_boats.m_aBoat[eWorks.A].RunMove(Boat.ePos.Vision, false);  
        }

        private void GridB_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (EQ.p_eState != EQ.eState.Ready) return;
            if (m_boats.p_eState != ModuleBase.eState.Ready) return;
            m_boats.m_aBoat[eWorks.B].RunMove(Boat.ePos.Vision, false);
        }

        private void GridInfo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (m_boats.p_eState)
            {
                case ModuleBase.eState.Init: m_boats.p_eState = ModuleBase.eState.Home; break;
                case ModuleBase.eState.Error: m_boats.Reset(); break; 
            }
        }
    }
}
