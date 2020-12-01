using Root_ASIS.AOI;
using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Root_AxisMapping.MainUI
{
    /// <summary>
    /// Align_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Align_UI : UserControl
    {
        public Align_UI()
        {
            InitializeComponent();
        }

        Align m_align;
        public void Init(Align align)
        {
            m_align = align;
            memoryViewerAlignUI.Init(align.m_axisMapping.m_memoryPool.m_viewer, false);
            listViewROI.ItemsSource = align.m_aROI;
            treeGrabUI.Init(align.m_treeRootGrab);
            align.RunTreeGrab(Tree.eMode.Init);
            treeInspectUI.Init(align.m_treeRootInspect);
            align.RunTreeInspect(Tree.eMode.Init);
            treeRotateUI.Init(align.m_treeRootRotate);
            align.RunTreeRotate(Tree.eMode.Init);
            InitTimer();
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            IsEnabled = (m_align.m_axisMapping.p_eState != ModuleBase.eState.Run) || EQ.p_bSimulate;
            buttonInspect.IsEnabled = (m_align.m_aROI[0].p_eROI == AOIData.eROI.Done) && (m_align.m_aROI[1].p_eROI == AOIData.eROI.Done);
            buttonRotate.IsEnabled = (m_align.m_degRotate != 0);
        }
        #endregion

        #region Grab
        private void buttonGrab_Click(object sender, RoutedEventArgs e)
        {
            m_align.RunGrab();
            m_align.InvalidROI();
        }
        #endregion

        #region ROI
        private void listViewROI_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int nSelect = listViewROI.SelectedIndex;
            if (nSelect < 0) return;
            if (nSelect >= m_align.m_aROI.Count) return;
            m_align.ClearActive();
            m_align.m_aROI[nSelect].p_eROI = AOIData.eROI.Active;
            m_align.Draw(AOIData.eDraw.ROI);
        }
        #endregion

        #region Inspect
        private void buttonInspect_Click(object sender, RoutedEventArgs e)
        {
            m_align.Inspect();
            m_align.Draw(AOIData.eDraw.Inspect);
            m_align.RunTreeRotate(Tree.eMode.Init);
        }
        #endregion

        #region Rotate
        private void buttonRotate_Click(object sender, RoutedEventArgs e)
        {
            m_align.RunRotate();
            m_align.InvalidROI();
        }
        #endregion

        private void comboFocus_GotFocus(object sender, RoutedEventArgs e)
        {
            checkFocus.IsChecked = true; 
        }

        private void comboFocus_LostFocus(object sender, RoutedEventArgs e)
        {
            checkFocus.IsChecked = false; 
        }
    }
}
