using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Root_Pine2.Module
{
    /// <summary>
    /// MagazineEV_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MagazineEV_UI : UserControl
    {
        public MagazineEV_UI()
        {
            InitializeComponent();
        }

        MagazineEV m_magazineEV; 
        public void Init(MagazineEV magazineEV)
        {
            m_magazineEV = magazineEV;
            DataContext = magazineEV;
            treeRootUI.Init(magazineEV.m_treeRootQueue);
            magazineEV.RunTreeQueue(Tree.eMode.Init); 
        }

        public void OnTimer()
        {
            switch (m_magazineEV.p_eState)
            {
                case ModuleBase.eState.Init: Background = Brushes.White; break;
                case ModuleBase.eState.Home: Background = Brushes.MediumPurple; break;
                case ModuleBase.eState.Ready: Background = Brushes.LightGreen; break;
                case ModuleBase.eState.Run: Background = Brushes.Yellow; break;
                case ModuleBase.eState.Error: Background = Brushes.OrangeRed; break;
            }
            OnRunTree();
            textBlockUp.Text = (m_magazineEV.m_stack != null) ? "Stack" : ((m_magazineEV.m_aMagazine[InfoStrip.eMagazinePos.Up] != null) ? "Magazine" : "");
            textBlockDown.Text = (m_magazineEV.m_aMagazine[InfoStrip.eMagazinePos.Down] != null) ? "Magazine" : "";
            if (m_magazineEV.m_elevator.m_diProtrude.p_bIn)
            {
                gridUp.Background = Brushes.Pink; 
                gridDown.Background = Brushes.Pink;
            }
            else
            {
                gridUp.Background = m_magazineEV.m_elevator.m_bProduct[InfoStrip.eMagazinePos.Up] ? Brushes.LightBlue : Brushes.Beige;
                gridDown.Background = m_magazineEV.m_elevator.m_bProduct[InfoStrip.eMagazinePos.Down] ? Brushes.LightBlue : Brushes.Beige;
            }
        }

        int[] m_nQueue = new int[2] { 0, 0 };
        private void OnRunTree()
        {
            if ((m_magazineEV.m_qModuleRun.Count == m_nQueue[0]) && (m_magazineEV.m_qModuleRemote.Count == m_nQueue[1])) return;
            m_nQueue[0] = m_magazineEV.m_qModuleRun.Count;
            m_nQueue[1] = m_magazineEV.m_qModuleRemote.Count;
            m_magazineEV.RunTreeQueue(Tree.eMode.Init);
        }

        private void gridUp_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (EQ.p_eState == EQ.eState.Run) return;
            if (m_magazineEV.m_elevator.m_bProduct[InfoStrip.eMagazinePos.Down]) return;
            if (m_magazineEV.m_elevator.m_bProduct[InfoStrip.eMagazinePos.Up] == false) return;
            m_magazineEV.StartUnload();
        }

        private void gridDown_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (EQ.p_eState == EQ.eState.Run) return;
            if (m_magazineEV.m_elevator.m_bProduct[InfoStrip.eMagazinePos.Down] == false) return;
            m_magazineEV.StartUnload();
        }

        private void gridInfo_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (m_magazineEV.p_eState)
            {
                case ModuleBase.eState.Init: m_magazineEV.p_eState = ModuleBase.eState.Home; break;
                case ModuleBase.eState.Error: m_magazineEV.Reset(); break;
            }
        }
    }
}
