using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Windows; 
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
            switch (m_boats.m_vision.p_eVision)
            {
                case eVision.Top3D: checkBoxRoller.Visibility = Visibility.Hidden; break;
                case eVision.Top2D: checkBoxRoller.Visibility = m_pine2.p_b3D ? Visibility.Visible : Visibility.Hidden; break;
                case eVision.Bottom: checkBoxRoller.Visibility = Visibility.Visible; break;
            }
            if (checkBoxRoller.Visibility == Visibility.Hidden) checkBoxRoller.IsChecked = false; 
            textBlockA.Text = (m_boats.m_aBoat[eWorks.A].p_infoStrip != null) ? m_boats.m_aBoat[eWorks.A].p_infoStrip.p_id : "";
            textBlockB.Text = (m_boats.m_aBoat[eWorks.B].p_infoStrip != null) ? m_boats.m_aBoat[eWorks.B].p_infoStrip.p_id : "";
            gridStripA.Background = GetBrush(m_boats.m_aBoat[eWorks.A].p_infoStrip);
            gridStripB.Background = GetBrush(m_boats.m_aBoat[eWorks.B].p_infoStrip);
            gridA.Background = GetBrush(m_boats.m_aBoat[eWorks.A]);
            gridB.Background = GetBrush(m_boats.m_aBoat[eWorks.B]);
            textBlockStepA.Text = m_boats.m_aBoat[eWorks.A].p_eStep.ToString();
            textBlockStepB.Text = m_boats.m_aBoat[eWorks.B].p_eStep.ToString();
            OnRunTree();

            if (m_boats.m_vision.p_remote.m_client.p_bConnect == false)
            {
                m_boats.m_aBoat[eWorks.A].p_bWorksConnect = false;
                m_boats.m_aBoat[eWorks.B].p_bWorksConnect = false;
            }
        }

        Brush GetBrush(InfoStrip infoStrip)
        {
            if (infoStrip == null) return Brushes.Beige; 
            return infoStrip.p_bInspect ? Brushes.Orange : Brushes.Beige;
        }

        Brush GetBrush(Boat boat)
        {
            if (boat.p_bWorksConnect == false) return Brushes.OrangeRed; 
            switch (boat.p_eStep)
            {
                case Boat.eStep.Init: return Brushes.White;
                case Boat.eStep.Ready: return Brushes.LightGreen;
                case Boat.eStep.Run: return Brushes.Yellow;
                case Boat.eStep.Done: return Brushes.LightBlue;
                case Boat.eStep.RunReady: return Brushes.DarkGreen;
            }
            return Brushes.White; 
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
