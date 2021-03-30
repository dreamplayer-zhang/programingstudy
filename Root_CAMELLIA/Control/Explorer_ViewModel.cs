using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Root_CAMELLIA
{
    public class Explorer_ViewModel : ObservableObject
    {

        public virtual void OnMouseDoubleClick(object sender, System.Windows.Input.MouseEventArgs e)
        {
            int test = 10;
        }

        #region property
        //System.Windows.Controls.TreeView m_treeView = new System.Windows.Controls.TreeView();
        //public System.Windows.Controls.TreeView p_treeView
        //{
        //    get
        //    {
        //        return m_treeView;
        //    }
        //    set
        //    {
        //        SetProperty(ref m_treeView, value);
        //    }
        //}

        ObservableCollection<ImageTreeViewItem> m_collection = new ObservableCollection<ImageTreeViewItem>();
        public ObservableCollection<ImageTreeViewItem> p_collection
        {
            get
            {
                return m_collection;
            }
            set
            {
                SetProperty(ref m_collection, value);
            }
        }

        string path = @"C:\";
        List<string> pathList = new List<string>();
        #endregion
        public Explorer_ViewModel(string path)
        {
            this.path = path;
            InitPathList();
            Directory.CreateDirectory(path);
            p_collection.Clear();
            
            InitExplorer();
        }

        private void InitPathList()
        {
            string[] str = path.Split('\\');
            pathList = str.ToList();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        int deep = 1;
        private void InitExplorer()
        {
            //foreach (string str in Directory.GetLogicalDrives())   // 루트폴더
            //{
            //    if (str != "C:\\")^
            //    {
            //        continue;
            //    }
            string root = Directory.GetDirectoryRoot(path);
                try
                {
                    ImageTreeViewItem item = new ImageTreeViewItem();
                    item.Header = root;
                    item.Tag = root;
                    item.IsExpanded = true;
                    item.Expanded += new RoutedEventHandler(item_Expanded);   // 노드 확장시 추가
                    item.Collapsed += new RoutedEventHandler(item_Collapsed);
                    item.ImageUrl = new Uri("pack://application:,,/Resource/326.ico");
                    p_collection.Add(item);
                    GetSubDirectories(item);
                }

                catch (Exception)
                {
                    // MessageBox.Show(except.Message);   // 접근 거부 폴더로 인해 주석처리
                }
          //  }
        }

        private void GetSubDirectories(TreeViewItem itemParent)
        {
            deep +=1;
            if (itemParent == null) return;
            //if (itemParent.Items.Count == 0) return;

            try
            {
                string strPath = itemParent.Tag as string;
                try
                {
                    if (itemParent.Items.Count != 0 && itemParent.Items.GetItemAt(0).ToString() == "")
                        itemParent.Items.RemoveAt(0);
                }

                catch (Exception) { }
                DirectoryInfo dInfoParent = new DirectoryInfo(strPath);
                string path = "";
                for (int i = 0; i < deep; i++)
                {
                    if(i == 0)
                    {
                        path += pathList[i];
                    }
                    else
                    {
                        path += "\\" + pathList[i];
                    }
                }
                foreach (DirectoryInfo dInfo in dInfoParent.GetDirectories())
                {
        
                    if (dInfo.FullName.Contains(path) == false)
                    {
                        continue;
                    }
                    ImageTreeViewItem item = new ImageTreeViewItem();
                    item.Header = dInfo.Name;
                    item.Tag = dInfo.FullName + "\\";

                    item.Expanded += new RoutedEventHandler(item_Expanded);
                    item.Collapsed += new RoutedEventHandler(item_Collapsed);
                    itemParent.Items.Add(item);

                    if (dInfo.GetDirectories().Length >= 0)
                    {
                        item.ImageUrl = new Uri("pack://application:,,/Resource/1437.ico");
                        GetSubDirectories(item);
                        deep -= 1;
                        //item.Items.Add(dInfo.Name);
                    }
                    if (dInfo.GetFiles().Length > 0)
                    {
                        foreach (FileInfo file in dInfo.GetFiles())
                        {
                            Icon iconForFile = System.Drawing.SystemIcons.WinLogo;

                            ImageList imageList = new ImageList();
                            ImageTreeViewItem fileItem = new ImageTreeViewItem();
                            fileItem.Header = file.Name;
                            fileItem.Tag = file.FullName;
                            if (!imageList.Images.ContainsKey(file.Extension))
                            {
                                iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName);
                                if(iconForFile != null)
                                {
                                    fileItem.FileIcon = iconForFile;
                                }
                            }

                            item.Items.Add(fileItem);
                        }
                    }
                }
            }

            catch (Exception)
            {
                // MessageBox.Show(except.Message);   // 접근 거부 폴더로 인해 주석처리
            }
        }

        void item_Collapsed(object sender, RoutedEventArgs e)
        {
            ImageTreeViewItem itemParent = sender as ImageTreeViewItem;
            if (itemParent.IsExpanded)
            {
                return;
            }
            string path = itemParent.Header.ToString();
            if (path.Contains(@"\"))
            {
                return;
            }
            itemParent.ImageUrl = new Uri("pack://application:,,/Resource/1437.ico");
            string name = itemParent.Tag.ToString();

        }
        // 트리확장시 내용 추가
        void item_Expanded(object sender, RoutedEventArgs e)
        {
            ImageTreeViewItem itemParent = sender as ImageTreeViewItem;
            if (itemParent == null) return;
            if (itemParent.Items.Count == 0) return;
            string path = itemParent.Header.ToString();
            if (!path.Contains(@"\"))
            {
                itemParent.ImageUrl = new Uri("pack://application:,,/Resource/open2.png");
            }



            if (itemParent.Items.Count == 1 && itemParent.Items.GetItemAt(0).ToString() == "")
            {
                GetSubDirectories(itemParent);
            }

            //foreach (ImageTreeViewItem item in itemParent.Items)
            //{
            //    GetSubDirectories(item);
            //}
        }

        public ICommand CmdRefresh
        {
            get
            {
                return new RelayCommand(() =>
                {
                    deep = 1;
                    p_collection.Clear();
                    InitExplorer();
                });
            }
        }
    }

    public class HeaderToImageConverter : IValueConverter
    {
        public static HeaderToImageConverter Instance =
            new HeaderToImageConverter();

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if ((value as string).Contains(@"\"))
            {
                Uri uri = new Uri
                ("pack://application:,,,/Images/diskdrive.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            }
            else
            {
                Uri uri = new Uri("pack://application:,,,/Images/folder.png");
                BitmapImage source = new BitmapImage(uri);
                return source;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class ImageTreeViewItem : TreeViewItem, INotifyPropertyChanged
    {
        #region Data Member

        Uri _imageUrl = null;
        Icon _fileIcon = null;
        System.Windows.Controls.Image _image = null;
        ImageSource _imgSource = null;
        TextBlock _textBlock = null;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public Uri ImageUrl
        {
            get { return _imageUrl; }
            set
            {
                _imageUrl = value;
                IconImage = new BitmapImage(value);
                OnPropertyChanged("ImageUrl");
            }
        }

        public ImageSource IconImage
        {
            get { return _imgSource; }
            set
            {
                _imgSource = value;
                OnPropertyChanged("IconImage");
            }
        }

        public Icon FileIcon
        {
            get { return _fileIcon; }
            set
            {
                _fileIcon = value;
                Bitmap bitmap = value.ToBitmap();
                IntPtr hBitmap = bitmap.GetHbitmap();
                ImageSource wpfBitmap =
     Imaging.CreateBitmapSourceFromHBitmap(
          hBitmap, IntPtr.Zero, Int32Rect.Empty,
          BitmapSizeOptions.FromEmptyOptions());
                //bitmap.Save(@"D:\Root_Cameliia2\Root\Root_CAMELLIA\Resource\test.bmp");
                IconImage = wpfBitmap;
                OnPropertyChanged("ImageUrl");
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
            stack.Orientation = System.Windows.Controls.Orientation.Horizontal;

            _image = new System.Windows.Controls.Image();
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
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
