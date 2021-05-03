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

        // public ICommand SelectedPathFromTreeCommand moved to ViewModel

        // a Name to bind to the NavTreeTabs
        private string treeName = "";
        public string TreeName
        {
            get { return treeName; }
            set { SetProperty(ref treeName, value, "TreeName"); }
        }

        // RootNr determines nr of RootItem that is used as RootNode 
        private int rootNr;
        public int RootNr
        {
            get { return rootNr; }
            set { SetProperty(ref rootNr, value, "RootNr"); }
        }

        // RootChildren are used to bind to TreeView
        private ObservableCollection<INavTreeItem> rootChildren = new ObservableCollection<INavTreeItem> { };
        public ObservableCollection<INavTreeItem> RootChildren
        {
            get { return rootChildren; }
            set { SetProperty(ref rootChildren, value, "RootChildren"); }
        }

        public void RebuildTree(int pRootNr = 0, bool pIncludeFileChildren = false, string InitPath = "")
        {
            // First take snapshot of current expanded items
            List<String> SnapShot = NavTreeUtils.TakeSnapshot(rootChildren);

            // As a matter of fact we delete and construct the tree//RoorChildren again.....
            // Delete all rootChildren
            foreach (INavTreeItem item in rootChildren) item.DeleteChildren();
            rootChildren.Clear();

            // Create treeRootItem 
            if (pRootNr != -1) RootNr = pRootNr;
            NavTreeItem treeRootItem = NavTreeRootItemUtils.ReturnRootItem(RootNr, pIncludeFileChildren);
            if (pRootNr != -1) TreeName = treeRootItem.FriendlyName;

            if (InitPath != "")
                treeRootItem.InitPathName = InitPath;

            treeRootItem.LastPathName = p_CurrentPath;
            // Copy children treeRootItem to RootChildren, set up new tree 
            foreach (INavTreeItem item in treeRootItem.Children) { RootChildren.Add(item); }

            //Expand previous snapshot
            NavTreeUtils.ExpandSnapShotItems(SnapShot, treeRootItem);
        }

        // Constructors
        public Explorer_ViewModel(int pRootNumber = 0, bool pIncludeFileChildren = false,  string InitPath = "")
        {
            // create a new RootItem given rootNumber using convention
            RootNr = pRootNumber;
            NavTreeItem treeRootItem = NavTreeRootItemUtils.ReturnRootItem(pRootNumber, pIncludeFileChildren, InitPath);
            TreeName = treeRootItem.FriendlyName;
            if(InitPath != "")
            {
                p_InitPath = InitPath;
                p_CurrentPath = InitPath;
                treeRootItem.InitPathName = p_InitPath;
            }
            treeRootItem.LastPathName = "";
            // Delete RootChildren and init RootChildren ussing treeRootItem.Children
            foreach (INavTreeItem item in RootChildren) { item.DeleteChildren(); }
            RootChildren.Clear();

            foreach (INavTreeItem item in treeRootItem.Children) { RootChildren.Add(item); }
        }

        string m_InitPath = "";
        public string p_InitPath
        {
            get
            {
                return m_InitPath;
            }
            private set
            {
                m_InitPath = value;
            }
        }

        string m_CurrentPath = "";
        public string p_CurrentPath
        {
            get
            {
                return m_CurrentPath;
            }
            set
            {
                SetProperty(ref m_CurrentPath, value);
            }
        }


        // Well I suppose with the implicit values these are just for the record/illustration  
        public Explorer_ViewModel(int rootNumber) : this(rootNumber, false) { }
        public Explorer_ViewModel() : this(0) { }


        public ICommand CmdSelectItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //int i = 10;
                });
            }
        }

        public ICommand CmdRefresh
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RebuildTree(pIncludeFileChildren: true, InitPath: p_InitPath);
                    ItemRefreshEvent();
                });
            }
        }

        CustomRelayCommand selectedPathFromTreeCommand;
        public ICommand CmdSelectPath
        {
            get
            {
                return selectedPathFromTreeCommand ?? 
                    (selectedPathFromTreeCommand =  new CustomRelayCommand(x => p_CurrentPath = (x as string)));
            }
        }

        public void test(string path)
        {

        }

        public event System.EventHandler ItemClicked;
        public event System.EventHandler ItemRefresh;




        void ItemClickedEvent(object path)
        {
            if (ItemClicked != null)
                OnItemClicked(path);
        }

        protected virtual void OnItemClicked(object path)
        {
            if (ItemClicked != null)
            {
                ItemClicked.Invoke(path, new EventArgs());
            }
        }

        void ItemRefreshEvent()
        {
            if (ItemRefresh != null)
                OnItemRefresh();
        }
        protected void OnItemRefresh()
        {
            if (ItemRefresh != null)
                ItemRefresh.Invoke(this, new EventArgs());
        }

        public void OnMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Controls.TreeView sen = (System.Windows.Controls.TreeView)sender;
            INavTreeItem selectedItem = (INavTreeItem)sen.SelectedItem;
            //sen.Background = System.Windows.Media.Brushes.Red;
            //selectedItem.
            if (selectedItem == null)
            {
                return;
            }
            string path = (string)selectedItem.FullPathName;
            p_CurrentPath = path;
            RootChildren[0].LastPathName = p_CurrentPath;
            //System.Windows.Forms.MessageBox.Show(path);
            try
            {
                FileAttributes chkAtt = File.GetAttributes(path);
                if ((chkAtt & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    ItemClickedEvent((object)path);
                }
            }
            catch(Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
                CustomMessageBox.Show(ex.Message);
            }
            
        }

        public void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            System.Windows.Controls.TreeView sen = (System.Windows.Controls.TreeView)sender;
            INavTreeItem selectedItem = (INavTreeItem)sen.SelectedItem;
            //sen.Background = System.Windows.Media.Brushes.Red;
            //selectedItem.
            if (selectedItem == null)
            {
                return;
            }
            string path = (string)selectedItem.FullPathName;
            p_CurrentPath = path;
            RootChildren[0].LastPathName = p_CurrentPath;
            //System.Windows.Forms.MessageBox.Show(path);
            try
            {
                FileAttributes chkAtt = File.GetAttributes(path);
                if ((chkAtt & FileAttributes.Directory) != FileAttributes.Directory)
                {
                    ItemClickedEvent((object)path);
                }
            }catch(Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);
                CustomMessageBox.Show(ex.Message);
            }

        }

        public class CustomRelayCommand : ICommand
        {
            #region Fields

            readonly Action<object> _execute;
            readonly Predicate<object> _canExecute;

            #endregion // Fields

            #region Constructors

            // constructor no CanExecute
            public CustomRelayCommand(Action<object> execute)
                : this(execute, null)
            {
            }

            public CustomRelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                if (execute == null)
                    throw new ArgumentNullException("execute");

                _execute = execute;
                _canExecute = canExecute;
            }
            #endregion

            #region ICommand Members

            public bool CanExecute(object parameter)
            {
                return _canExecute == null ? true : _canExecute(parameter);
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public void Execute(object parameter)
            {
                _execute(parameter);
            }

            #endregion // ICommand Members
        }
    }






    //    public event System.EventHandler DoubleClicked;



    //    void DoubleClickedEvent(object path)
    //    {
    //        if (DoubleClicked != null)
    //            OnDoubleClicked(path);
    //    }

    //    protected virtual void OnDoubleClicked(object path)
    //    {
    //        if (DoubleClicked != null)
    //        {
    //            DoubleClicked.Invoke(path, new EventArgs());
    //        }
    //    }
    //    public void OnMouseDoubleClick(object sender, System.Windows.Input.MouseEventArgs e)
    //    {
    //        //TreeView item = sender as TreeView;
    //        System.Windows.Controls.TreeView sen = (System.Windows.Controls.TreeView)sender;
    //        //((TreeViewItem)sen.SelectedItem).HasItems
    //        ImageTreeViewItem selectedItem = (ImageTreeViewItem)sen.SelectedItem;
    //        if(selectedItem == null)
    //        {
    //            return;
    //        }
    //        string path = (string)selectedItem.Tag;
    //        FileAttributes chkAtt = File.GetAttributes(path);
    //        if ((chkAtt & FileAttributes.Directory) != FileAttributes.Directory)
    //        {
    //            DoubleClickedEvent((object)path);
    //        }
    //    }

    //    #region property
    //    //System.Windows.Controls.TreeView m_treeView = new System.Windows.Controls.TreeView();
    //    //public System.Windows.Controls.TreeView p_treeView
    //    //{
    //    //    get
    //    //    {
    //    //        return m_treeView;
    //    //    }
    //    //    set
    //    //    {
    //    //        SetProperty(ref m_treeView, value);
    //    //    }
    //    //}

    //    ObservableCollection<ImageTreeViewItem> m_collection = new ObservableCollection<ImageTreeViewItem>();
    //    public ObservableCollection<ImageTreeViewItem> p_collection
    //    {
    //        get
    //        {
    //            return m_collection;
    //        }
    //        set
    //        {
    //            SetProperty(ref m_collection, value);
    //        }
    //    }

    //    string path = @"C:\";
    //    List<string> pathList = new List<string>();
    //    #endregion
    //    public Explorer_ViewModel()
    //    {

    //    }
    //    public Explorer_ViewModel(string path)
    //    {
    //        this.path = path;
    //        InitPathList();
    //        Directory.CreateDirectory(path);
    //        p_collection.Clear();

    //        InitExplorer();
    //    }

    //    private void InitPathList()
    //    {
    //        string[] str = path.Split('\\');
    //        pathList = str.ToList();
    //    }

    //    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    //    {

    //    }

    //    int deep = 1;
    //    private void InitExplorer()
    //    {
    //        //foreach (string str in Directory.GetLogicalDrives())   // 루트폴더
    //        //{
    //        //    if (str != "C:\\")^
    //        //    {
    //        //        continue;
    //        //    }
    //        string root = Directory.GetDirectoryRoot(path);
    //        try
    //        {
    //            ImageTreeViewItem item = new ImageTreeViewItem();
    //            item.Header = root;
    //            item.Tag = root;
    //            item.IsExpanded = true;
    //            item.Expanded += new RoutedEventHandler(item_Expanded);   // 노드 확장시 추가
    //            item.Collapsed += new RoutedEventHandler(item_Collapsed);
    //            item.ImageUrl = new Uri("pack://application:,,/Resource/326.ico");
    //            p_collection.Add(item);
    //            isInit = true;
    //            GetInitSubDirectories(item);
    //            isInit = false;
    //        }

    //        catch (Exception)
    //        {
    //                // MessageBox.Show(except.Message);   // 접근 거부 폴더로 인해 주석처리
    //            }
    //      //  }
    //    }

    //    private void GetInitSubDirectories(TreeViewItem itemParent)
    //    {
    //        deep += 1;
    //        if(deep > pathList.Count)
    //        {
    //            return;
    //        }
    //        if (itemParent == null) return;

    //        try
    //        {
    //            string strPath = itemParent.Tag as string;
    //            try
    //            {
    //                if (itemParent.Items.Count != 0 && itemParent.Items.GetItemAt(0).ToString() == "")
    //                    itemParent.Items.RemoveAt(0);
    //            }

    //            catch (Exception) { }
    //            DirectoryInfo dInfoParent = new DirectoryInfo(strPath);
    //            string path = "";
    //            for (int i = 0; i < deep; i++)
    //            {
    //                if (deep > pathList.Count) break;
    //                if (i == 0)
    //                {
    //                    path += pathList[i];
    //                }
    //                else
    //                {
    //                    path += "\\" + pathList[i];
    //                }
    //            }
    //            foreach (DirectoryInfo dInfo in dInfoParent.GetDirectories())
    //            {

    //                if (dInfo.FullName.Contains(path) == false)
    //                {
    //                    continue;
    //                }
    //                ImageTreeViewItem item = new ImageTreeViewItem();
    //                item.Header = dInfo.Name;
    //                item.Tag = dInfo.FullName + "\\";

    //                item.Expanded += new RoutedEventHandler(item_Expanded);
    //                item.Collapsed += new RoutedEventHandler(item_Collapsed);
    //                item.ImageUrl = new Uri("pack://application:,,/Resource/1437.ico");

    //                if (dInfo.GetDirectories().Length >= 0)
    //                {
    //                    if(deep == pathList.Count - 1)
    //                    {
    //                        item.Items.Add(new ImageTreeViewItem(""));
    //                        itemParent.Items.Add(item);
    //                        return;
    //                    }
    //                    GetInitSubDirectories(item);
    //                    deep -= 1;
    //                    //item.Items.Add(new ImageTreeViewItem("")); //Dummy
    //                }

    //                itemParent.Items.Add(item);
    //            }
    //            //if (dInfo.GetFiles().Length > 0)
    //            //{
    //            //    foreach (FileInfo file in dInfo.GetFiles())
    //            //    {
    //            //        Icon iconForFile = System.Drawing.SystemIcons.WinLogo;

    //            //        ImageList imageList = new ImageList();
    //            //        ImageTreeViewItem fileItem = new ImageTreeViewItem();
    //            //        fileItem.Header = file.Name;
    //            //        fileItem.Tag = file.FullName;
    //            //        if (!imageList.Images.ContainsKey(file.Extension))
    //            //        {
    //            //            iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName);
    //            //            if (iconForFile != null)
    //            //            {
    //            //                fileItem.FileIcon = iconForFile;
    //            //            }
    //            //        }

    //            //        item.Items.Add(fileItem);
    //            //    }
    //            //}
    //        }

    //        catch (Exception)
    //        {
    //            // MessageBox.Show(except.Message);   // 접근 거부 폴더로 인해 주석처리
    //        }
    //    }

    //    bool isInit = true;
    //    private void GetSubDirectories(TreeViewItem itemParent)
    //    {

    //        if (itemParent == null) return;
    //        //if (itemParent.Items.Count == 0) return;

    //        try
    //        {
    //            string strPath = itemParent.Tag as string;
    //            try
    //            {
    //                if (itemParent.Items.Count != 0 && itemParent.Items.GetItemAt(0).ToString() == "")
    //                    itemParent.Items.RemoveAt(0);
    //            }

    //            catch (Exception) { }
    //            DirectoryInfo dInfoParent = new DirectoryInfo(strPath);
    //            string path = "";
    //            if (!isInit)
    //                for (int i = 0; i < pathList.Count; i++)
    //                {
    //                    //if (deep > pathList.Count) break ;
    //                    if (i == 0)
    //                    {
    //                        path += pathList[i];
    //                    }
    //                    else
    //                    {
    //                        path += "\\" + pathList[i];
    //                    }
    //                }
    //            else
    //            {
    //                deep +=1;
    //                for (int i = 0; i < deep; i++)
    //                {
    //                    //if (deep > pathList.Count) break ;
    //                    if (i == 0)
    //                    {
    //                        path += pathList[i];
    //                    }
    //                    else
    //                    {
    //                        path += "\\" + pathList[i];
    //                    }
    //                }
    //            }
    //            foreach (DirectoryInfo dInfo in dInfoParent.GetDirectories())
    //            {

    //                if (dInfo.FullName.Contains(path) == false)
    //                {
    //                    continue;
    //                }

    //                ImageTreeViewItem item = new ImageTreeViewItem();
    //                item.Header = dInfo.Name;
    //                item.Tag = dInfo.FullName + "\\";
    //                item.ImageUrl = new Uri("pack://application:,,/Resource/1437.ico");
    //                item.Expanded += new RoutedEventHandler(item_Expanded);
    //                item.Collapsed += new RoutedEventHandler(item_Collapsed);
    //                //item.Items.Add(new ImageTreeViewItem(""));

    //                if (dInfo.GetDirectories().Length >= 0 || dInfo.GetFiles().Length >= 0)
    //                {
    //                    //item.ImageUrl = new Uri("pack://application:,,/Resource/1437.ico");
    //                    //GetSubDirectories(item);
    //                    item.Items.Add(new ImageTreeViewItem(""));
    //                    deep -= 1;
    //                }


    //                itemParent.Items.Add(item);
    //            }

    //                StopWatch sw = new StopWatch();
    //            App.Current.Dispatcher.BeginInvoke(new Action(delegate
    //            {
    //                if (dInfoParent.GetFiles().Length > 0)
    //                {
    //                    foreach (FileInfo file in dInfoParent.GetFiles())
    //                    {
    //                        sw.Start();
    //                        Icon iconForFile = System.Drawing.SystemIcons.WinLogo;

    //                        ImageList imageList = new ImageList();
    //                        ImageTreeViewItem fileItem = new ImageTreeViewItem();
    //                        fileItem.Header = file.Name;
    //                        fileItem.Tag = file.FullName;
    //                        if (!imageList.Images.ContainsKey(file.Extension))
    //                        {
    //                            iconForFile = System.Drawing.Icon.ExtractAssociatedIcon(file.FullName);
    //                            if (iconForFile != null)
    //                            {
    //                                fileItem.FileIcon = iconForFile;
    //                            }
    //                        }
    //                        itemParent.Items.Add(fileItem);
    //                        sw.Start();
    //                        // System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
    //                    }
    //                }
    //            }));


    //        }

    //        catch (Exception)
    //        {
    //            // MessageBox.Show(except.Message);   // 접근 거부 폴더로 인해 주석처리
    //        }
    //    }

    //    void item_Collapsed(object sender, RoutedEventArgs e)
    //    {
    //        ImageTreeViewItem itemParent = sender as ImageTreeViewItem;
    //        if (itemParent.IsExpanded)
    //        {
    //            return;
    //        }
    //        string path = itemParent.Header.ToString();
    //        if (path.Contains(@"\"))
    //        {
    //            return;
    //        }
    //        itemParent.ImageUrl = new Uri("pack://application:,,/Resource/1437.ico");
    //        string name = itemParent.Tag.ToString();

    //    }
    //    // 트리확장시 내용 추가
    //    void item_Expanded(object sender, RoutedEventArgs e)
    //    {

    //        ImageTreeViewItem itemParent = sender as ImageTreeViewItem;
    //        if (itemParent == null) return;
    //        if (itemParent.Items.Count == 0) return;
    //        string path = itemParent.Header.ToString();

    //        if (path.Contains(@"\") || !itemParent.IsExpanded)
    //        {
    //            return;
    //        }
    //        if (!path.Contains(@"\"))
    //        {
    //            itemParent.ImageUrl = new Uri("pack://application:,,/Resource/open2.png");
    //        }
    //        ImageTreeViewItem item = itemParent.Items.GetItemAt(0) as ImageTreeViewItem;
    //        string str =(string)item.Tag;
    //        if (str == "")
    //        {
    //            itemParent.Items.RemoveAt(0);
    //            GetSubDirectories(itemParent);
    //        }
    //        else
    //        {
    //            //GetSubDirectories(itemParent);
    //        }

    //        //foreach (ImageTreeViewItem item in itemParent.Items)
    //        //{
    //        //    GetSubDirectories(item);
    //        //}
    //    }

    //    public ICommand CmdRefresh
    //    {
    //        get
    //        {
    //            return new RelayCommand(() =>
    //            {
    //                deep = 1;
    //                p_collection.Clear();
    //                InitExplorer();
    //            });
    //        }
    //    }
    //}

    //public class HeaderToImageConverter : IValueConverter
    //{
    //    public static HeaderToImageConverter Instance =
    //        new HeaderToImageConverter();

    //    public object Convert(object value, Type targetType,
    //        object parameter, CultureInfo culture)
    //    {
    //        if ((value as string).Contains(@"\"))
    //        {
    //            Uri uri = new Uri
    //            ("pack://application:,,,/Images/diskdrive.png");
    //            BitmapImage source = new BitmapImage(uri);
    //            return source;
    //        }
    //        else
    //        {
    //            Uri uri = new Uri("pack://application:,,,/Images/folder.png");
    //            BitmapImage source = new BitmapImage(uri);
    //            return source;
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType,
    //        object parameter, CultureInfo culture)
    //    {
    //        throw new NotSupportedException("Cannot convert back");
    //    }
    //}

    //public class ImageTreeViewItem : TreeViewItem, INotifyPropertyChanged
    //{
    //    #region Data Member

    //    Uri _imageUrl = null;
    //    Icon _fileIcon = null;
    //    System.Windows.Controls.Image _image = null;
    //    ImageSource _imgSource = null;
    //    TextBlock _textBlock = null;

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    #endregion

    //    #region Properties

    //    public Uri ImageUrl
    //    {
    //        get { return _imageUrl; }
    //        set
    //        {
    //            _imageUrl = value;
    //            IconImage = new BitmapImage(value);
    //            OnPropertyChanged("ImageUrl");
    //        }
    //    }

    //    public ImageSource IconImage
    //    {
    //        get { return _imgSource; }
    //        set
    //        {
    //            _imgSource = value;
    //            OnPropertyChanged("IconImage");
    //        }
    //    }

    //    public Icon FileIcon
    //    {
    //        get { return _fileIcon; }
    //        set
    //        {
    //            _fileIcon = value;
    //            Bitmap bitmap = value.ToBitmap();
    //            IntPtr hBitmap = bitmap.GetHbitmap();
    //            ImageSource wpfBitmap =
    // Imaging.CreateBitmapSourceFromHBitmap(
    //      hBitmap, IntPtr.Zero, Int32Rect.Empty,
    //      BitmapSizeOptions.FromEmptyOptions());
    //            //bitmap.Save(@"D:\Root_Cameliia2\Root\Root_CAMELLIA\Resource\test.bmp");
    //            IconImage = wpfBitmap;
    //            OnPropertyChanged("ImageUrl");
    //        }
    //    }

    //    public string Text
    //    {
    //        get { return _textBlock.Text; }
    //        set { _textBlock.Text = value; }
    //    }

    //    #endregion

    //    #region Constructor

    //    public ImageTreeViewItem()
    //    {
    //        CreateTreeViewItemTemplate();
    //    }
    //    public ImageTreeViewItem(string str)
    //    {
    //        Tag = str;
    //    }

    //    #endregion

    //    #region Private Methods

    //    private void CreateTreeViewItemTemplate()
    //    {
    //        StackPanel stack = new StackPanel();
    //        stack.Orientation = System.Windows.Controls.Orientation.Horizontal;

    //        _image = new System.Windows.Controls.Image();
    //        _image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
    //        _image.VerticalAlignment = System.Windows.VerticalAlignment.Center;
    //        _image.Width = 16;
    //        _image.Height = 16;
    //        _image.Margin = new Thickness(2);

    //        stack.Children.Add(_image);

    //        _textBlock = new TextBlock();
    //        _textBlock.Margin = new Thickness(2);
    //        _textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;

    //        stack.Children.Add(_textBlock);

    //        Header = stack;
    //    }
    //    #endregion
    //    protected void OnPropertyChanged(string name)
    //    {
    //        PropertyChangedEventHandler handler = PropertyChanged;
    //        if (handler != null)
    //        {
    //            handler(this, new PropertyChangedEventArgs(name));
    //        }
    //    }
    //}
}
