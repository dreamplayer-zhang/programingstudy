using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_CAMELLIA
{
    /// <summary>
    /// Explorer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Explorer : UserControl
    {
        public Explorer()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitExplorer();
        }

        private void InitExplorer()
        {
            foreach (string str in Directory.GetLogicalDrives())   // 루트폴더
            {
                if (str != "C:\\")
                {
                    continue;
                }
                try
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = str;
                    item.Tag = str;
                    item.IsExpanded = true;
                    item.Expanded += new RoutedEventHandler(item_Expanded);   // 노드 확장시 추가

                    ExplorerTree.Items.Add(item);
                    GetSubDirectories(item);
                }

                catch (Exception)
                {
                    // MessageBox.Show(except.Message);   // 접근 거부 폴더로 인해 주석처리
                }
            }
        }

        private void GetSubDirectories(TreeViewItem itemParent)
        {
            if (itemParent == null) return;
            //if (itemParent.Items.Count == 0) return;

            try
            {
                string strPath = itemParent.Tag as string;
                try
                {
                    if (itemParent.Items.GetItemAt(0).ToString() == "")
                        itemParent.Items.RemoveAt(0);
                }
                
                catch (Exception) { }
                DirectoryInfo dInfoParent = new DirectoryInfo(strPath);
                foreach (DirectoryInfo dInfo in dInfoParent.GetDirectories())
                {
                    if (dInfo.FullName.Contains(@"C:\Camellia2") == false)
                    {
                        continue;
                    }
                    ImageTreeViewItem item = new ImageTreeViewItem();
                    item.Header = dInfo.Name;
                    item.Tag = dInfo.FullName + "\\";
                    item.Expanded += new RoutedEventHandler(item_Expanded);
                    itemParent.Items.Add(item);

                    if(dInfo.GetDirectories().Length > 0)
                    {
                        item.Items.Add("");
                    }
                }
            }

            catch (Exception)
            {
                // MessageBox.Show(except.Message);   // 접근 거부 폴더로 인해 주석처리
            }
        }

        // 트리확장시 내용 추가
        void item_Expanded(object sender, RoutedEventArgs e)
        {
            ImageTreeViewItem itemParent = sender as ImageTreeViewItem;
            if (itemParent == null) return;
            if (itemParent.Items.Count == 0) return;

            if (itemParent.Items.Count == 1 && itemParent.Items.GetItemAt(0).ToString() == "")
            {
                GetSubDirectories(itemParent);
            }

            //foreach (ImageTreeViewItem item in itemParent.Items)
            //{
            //    GetSubDirectories(item);
            //}
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ExplorerTree.Items.Clear();
            InitExplorer();
        }
    }

    public class ImageTreeViewItem : TreeViewItem
    {
        #region Data Member

        Uri _imageUrl = null;
        Image _image = null;
        TextBlock _textBlock = null;

        #endregion

        #region Properties

        public Uri ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                _image.Source = new BitmapImage(value);
            }
        }

        public string Text
        {
            get { return _textBlock.Text; }
            set { _textBlock.Text = value; }
        }

        #endregion

        #region Constructor

        public ImageTreeViewItem()
        {
            CreateTreeViewItemTemplate();
        }

        #endregion

        #region Private Methods

        private void CreateTreeViewItemTemplate()
        {
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            _image = new Image();
            _image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            _image.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            _image.Width = 16;
            _image.Height = 16;
            _image.Margin = new Thickness(2);

            stack.Children.Add(_image);

            _textBlock = new TextBlock();
            _textBlock.Margin = new Thickness(2);
            _textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            stack.Children.Add(_textBlock);

            Header = stack;
        }

        #endregion
    }
}
