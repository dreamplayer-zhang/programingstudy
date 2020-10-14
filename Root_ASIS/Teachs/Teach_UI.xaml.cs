using RootTools.Trees;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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
            listViewAOI.ItemsSource = teach.m_aAOI;
            listViewListAOI.ItemsSource = teach.m_aListAOI; 
            treeAOIUI.Init(teach.m_treeRootAOI);
            teach.RunTreeAOI(Tree.eMode.Init); 
            treeSetupUI.Init(teach.m_treeRootSetup);
            teach.RunTreeSetup(Tree.eMode.Init);
            InitAOI();
        }

        private void buttonInspect_Click(object sender, RoutedEventArgs e)
        {

        }

        #region AOI Drag n Drop
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
                m_teach.m_aAOI.Remove(aoi);
                m_teach.RunTreeAOI(Tree.eMode.Init);
                m_teach.RunTreeAOI(Tree.eMode.Init);
            }
        }

        private void ListViewAOI_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(c_sAOI))
            {
                IAOI aoi = (IAOI)e.Data.GetData(c_sAOI);
                m_teach.m_aAOI.Remove(aoi);
                ListView listView = sender as ListView;
                Point p = e.GetPosition(listView);
                IInputElement input = listView.InputHitTest(p);
                ListViewItem item = FindAncestor<ListViewItem>(input as DependencyObject);
                int iRemove = aoi.p_nID; 
                m_teach.m_aAOI.Remove(aoi);
                if (item == null) m_teach.m_aAOI.Add(aoi);
                else
                {
                    int iInsert = listView.Items.IndexOf(item.Content);
                    m_teach.m_aAOI.Insert(iInsert, aoi); 
                }
                m_teach.RunTreeAOI(Tree.eMode.Init);
                m_teach.RunTreeAOI(Tree.eMode.Init);
            }
            if (e.Data.GetDataPresent(c_sListAOI))
            {
                IAOI aoi = (IAOI)e.Data.GetData(c_sListAOI);
                m_teach.m_aAOI.Add(aoi.NewAOI());
                m_teach.RunTreeAOI(Tree.eMode.Init); 
            }
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
    }
}
