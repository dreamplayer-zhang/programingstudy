using Root_ASIS.AOI;
using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_ASIS.Teachs
{
    /// <summary>
    /// Teach_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Teach_UI : UserControl
    {
        public Teach_UI()
        {
            InitializeComponent();
        }

        Teach m_teach; 
        public void Init(Teach teach)
        {
            m_teach = teach;
            DataContext = teach;
            memoryViewerUI.Init(teach.m_memoryPool.m_viewer, false);
            listViewAOI.ItemsSource = teach.p_aAOI;
            listViewListAOI.ItemsSource = teach.m_aEnableAOI;
            treeViewROI.ItemsSource = teach.p_aROI; 
            treeAOIUI.Init(teach.m_treeRootAOI);
            teach.RunTreeAOI(Tree.eMode.Init);
            treeSetupUI.Init(teach.m_treeRootSetup);
            teach.RunTreeSetup(Tree.eMode.Init);
            InitAOI();
            InitTimer(); 
        }

        #region timer
        DispatcherTimer m_timer = new DispatcherTimer(); 
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            bool bDoneROI = (m_teach.m_nROIReady == 0) && (m_teach.m_nROIActive == 0);
            gridParameter.IsEnabled = bDoneROI;
            buttonAllocate.IsEnabled = bDoneROI;
            buttonViewAll.IsEnabled = bDoneROI;
            buttonViewROI.Background = (m_teach.p_eDraw == AOIData.eDraw.ROI) ? Brushes.LightYellow : Brushes.LightGray;
            buttonViewAll.Background = (m_teach.p_eDraw == AOIData.eDraw.All) ? Brushes.LightYellow : Brushes.LightGray;
        }
        #endregion

        #region AOI
        void InitAOI()
        {
            listViewListAOI.PreviewMouseLeftButtonDown += ListViewListAOI_PreviewMouseLeftButtonDown;
            listViewAOI.PreviewMouseLeftButtonDown += ListViewAOI_PreviewMouseLeftButtonDown;
            listViewListAOI.PreviewMouseMove += ListViewListAOI_PreviewMouseMove;
            listViewAOI.PreviewMouseMove += ListViewAOI_PreviewMouseMove;
            listViewListAOI.Drop += ListViewListAOI_Drop;
            listViewAOI.Drop += ListViewAOI_Drop;
        }

        bool m_bDragList = false; 
        private void ListViewListAOI_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = DragDropLeftButtonDown(sender, e); 
            m_bDragList = (item != null); 
        }

        bool m_bDrag = false;
        private void ListViewAOI_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = DragDropLeftButtonDown(sender, e);
            m_bDrag = (item != null);
        }

        ListViewItem DragDropLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListView listView = sender as ListView;
            Point p = e.GetPosition(listView);
            IInputElement input = listView.InputHitTest(p);
            return FindAncestor<ListViewItem>(input as DependencyObject);
        }

        const string c_sListAOI = "ListAOI"; 
        private void ListViewListAOI_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!m_bDragList || (e.LeftButton != MouseButtonState.Pressed)) return;
            m_bDragList = false;
            DragDropMouseMove(sender, e, c_sListAOI); 
        }

        const string c_sAOI = "AOI";
        private void ListViewAOI_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!m_bDrag || (e.LeftButton != MouseButtonState.Pressed)) return;
            m_bDrag = false;
            DragDropMouseMove(sender, e, c_sAOI);
        }

        void DragDropMouseMove(object sender, MouseEventArgs e, string sDragDrop)
        {
            ListViewItem item = FindAncestor<ListViewItem>(e.OriginalSource as DependencyObject);
            if (item == null) return;
            DataObject obj = new DataObject(sDragDrop, item.Content);
            System.Windows.DragDrop.DoDragDrop(item, obj, DragDropEffects.Copy);
        }

        private void ListViewListAOI_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(c_sAOI))
            {
                IAOI aoi = (IAOI)e.Data.GetData(c_sAOI);
                m_teach.p_aAOI.Remove(aoi);
                m_teach.RunTreeAOI(Tree.eMode.Init);
                m_teach.RunTreeAOI(Tree.eMode.Init);
                m_teach.InvalidROI(); 
            }
        }

        private void ListViewAOI_Drop(object sender, DragEventArgs e)
        {
            IAOI aoi = null; 
            ListView listView = sender as ListView;
            Point p = e.GetPosition(listView);
            IInputElement input = listView.InputHitTest(p);
            ListViewItem item = FindAncestor<ListViewItem>(input as DependencyObject);
            int iInsert = (item == null) ? -1 : listView.Items.IndexOf(item.Content);
            if (e.Data.GetDataPresent(c_sAOI))
            {
                aoi = (IAOI)e.Data.GetData(c_sAOI);
                m_teach.p_aAOI.Remove(aoi);
                int iRemove = aoi.p_nID; 
                m_teach.p_aAOI.Remove(aoi);
                if (iInsert < 0) m_teach.p_aAOI.Add(aoi);
                else m_teach.p_aAOI.Insert(iInsert, aoi);
            }
            if (e.Data.GetDataPresent(c_sListAOI))
            {
                aoi = (IAOI)e.Data.GetData(c_sListAOI);
                if (iInsert < 0) m_teach.p_aAOI.Add(aoi.NewAOI());
                else m_teach.p_aAOI.Insert(iInsert, aoi.NewAOI());
            }
            m_teach.RunTreeAOI(Tree.eMode.Init);
            m_teach.RunTreeAOI(Tree.eMode.Init);
            m_teach.InvalidROI();
        }

        private static TAncestor FindAncestor<TAncestor>(DependencyObject dependencyObject) where TAncestor : DependencyObject
        {
            do
            {
                if (dependencyObject is TAncestor) return (TAncestor)dependencyObject;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }
            while (dependencyObject != null);
            return null;
        }
        #endregion

        #region ROI
        private void buttonAllocate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonViewROI_Click(object sender, RoutedEventArgs e)
        {
            m_teach.p_eDraw = AOIData.eDraw.ROI; 
        }

        private void buttonViewAll_Click(object sender, RoutedEventArgs e)
        {
            m_teach.p_eDraw = AOIData.eDraw.All; 
        }

        private void treeViewROI_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                m_teach.InvalidROI();
                return; 
            }
            if ((e.OriginalSource is TextBlock) == false) return; 
            TextBlock tb = (TextBlock)e.OriginalSource;
            if (tb.DataContext is AOIData)
            {
                AOIData aoiActive = m_teach.p_roiActive;
                ((AOIData)tb.DataContext).p_eROI = AOIData.eROI.Active;
                m_teach.CalcROICount();
                if (m_teach.m_nROIActive > 1) aoiActive.p_eROI = AOIData.eROI.Ready;
            }
        }
        #endregion

        #region Parameter
        private void buttonInspect_Click(object sender, RoutedEventArgs e)
        {
        }
        #endregion
    }
}
