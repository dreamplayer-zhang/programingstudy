using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RootTools.Trees
{
    /// <summary>
    /// TreeListView_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TreeUI : UserControl
    {
        public TreeUI()
        {
            InitializeComponent();
        }

        TreeRoot m_treeRoot; 
        public void Init(TreeRoot treeRoot)
        {
            m_treeRoot = treeRoot; 
            this.DataContext = treeRoot;
        }

        private void ToggleButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {  
            if (e.Key == Key.Enter)
            {
                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                m_treeRoot.m_bFocus = false;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((ComboBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            m_treeRoot.m_bFocus = false;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ControlTree.ActualWidth < 16) return; 
            Column1.Width = ControlTree.ActualWidth / 2 - 2; 
            Column2.Width = ControlTree.ActualWidth / 2 - 2;
        }

        private void ControlTree_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Column2.Width = ControlTree.ActualWidth - Column1.ActualWidth - 4; 
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            m_treeRoot.m_bFocus = true; 
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            m_treeRoot.m_bFocus = false;
        }
    }
}
