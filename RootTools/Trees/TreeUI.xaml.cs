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

        public void Init(Tree root)
        {
            this.DataContext = root;
        }

        private void ToggleButton_PreviewKeyDown(object sender, KeyEventArgs e)
        {  
            if (e.Key == Key.Enter)
                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ControlTree.ActualWidth < 16) return; 
            Column1.Width = ControlTree.ActualWidth / 2; 
            Column2.Width = ControlTree.ActualWidth / 2;
        }

        private void ControlTree_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Column2.Width = ControlTree.ActualWidth - Column1.ActualWidth; 
        }
    }
}
