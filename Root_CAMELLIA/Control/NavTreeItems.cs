using System.Windows.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Threading;
using System.Text.RegularExpressions;

namespace Root_CAMELLIA
{
    // Definition of several NavTreeItem classes (constructor, GetMyIcion and GetMyChildren) from abstact class NavTreeItem 
    // Note that this file can be split/refactored in smaller parts

    // RootItems
    // - Special items are "RootItems" such as DriveRootItem with as children DriveItems
    //   other RootItems might be DriveNoChildRootItem, FavoritesRootItem, SpecialFolderRootItem, 
    //   (to do) LibraryRootItem, NetworkRootItem, HistoryRootItem.
    // - We use RootItem(s) as a RootNode for trees, their Children (for example DriveItems) are copied to RootChildren VM
    // - Binding in View: TreeView.ItemsSource="{Binding Path=NavTreeVm.RootChildren}"
    
    // DriveRootItem has DriveItems as children 
    public class DriveRootItem : NavTreeItem
    {
        public DriveRootItem()
        {
            //Constructor sets some properties
            FriendlyName = "DriveRoot";
            IsExpanded = true;
            FullPathName = "$xxDriveRoot$";
        }

        public DriveRootItem(string InitPath)
        {
            //Constructor sets some properties
            FriendlyName = "RootFile";
            IsExpanded = true;
            FullPathName = "$xxDriveRoot$";
            InitPathName = InitPath;
        }

        public override BitmapSource GetMyIcon()
        {
            // Note: introduce more "speaking" icons for RootItems
            //string Param = "pack://application:,,,/" + "MyImages/bullet_blue.png";
            string Param = "pack://application:,,/Resource/database.ico";
            Uri uri1 = new Uri(Param, UriKind.RelativeOrAbsolute);
            return myIcon = BitmapFrame.Create(uri1);
        }

        public override void SetExpandedIcon()
        {
            return;
        }

        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            INavTreeItem item1;
            string fn = "";

            DirectoryInfo di = new DirectoryInfo(InitPathName);

            //string[] LastPath = LastPathName.Split('\\');
            //string[] CalcPath = di.FullName.Split('\\');


            item1 = new DriveItem();

            item1.FullPathName = di.FullName;
            item1.FriendlyName = di.Name;
            item1.LastPathName = InitPathName;
            item1.InitPathName = InitPathName;
            item1.IsExpanded = true;
            item1.IncludeFileChildren = this.IncludeFileChildren;
            childrenList.Add(item1);
            //if (drive.IsReady && drive.Name.Contains("C"))
            //{
            //    item1 = new DriveItem();

            //    // Some processing for the FriendlyName
            //    fn = drive.Name.Replace(@"\", "");
            //    item1.FullPathName = fn;
            //    if (drive.VolumeLabel == string.Empty)
            //    {
            //        //fn = drive.DriveType.ToString() + " (" + fn + ")";
            //        fn =" (" + fn + ")";
            //    }
            //    else if (drive.DriveType == DriveType.CDRom)
            //    {
            //        fn = drive.DriveType.ToString() + " " + drive.VolumeLabel + " (" + fn + ")";
            //    }
            //    else
            //    {
            //        fn = drive.VolumeLabel + " (" + fn + ")";
            //    }

            //    item1.FriendlyName = fn;
            //    item1.InitPathName = InitPathName;
            //    item1.IsExpanded = true;
            //    item1.IncludeFileChildren = this.IncludeFileChildren;
            //    childrenList.Add(item1);
            //}

            return childrenList;
        }
    }

    public class DriveItem : NavTreeItem
    {
        public override BitmapSource GetMyIcon()
        {
            string Param = "pack://application:,,/Resource/database.ico";
            Uri uri1 = new Uri(Param, UriKind.RelativeOrAbsolute);
            return myIcon = BitmapFrame.Create(uri1);
            //return myIcon = Utils.GetIconFn.GetIconDll(this.FullPathName);
        }

        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            INavTreeItem item1;

            try
            {
                DirectoryInfo di = new DirectoryInfo(this.FullPathName); // may be acces not allowed
                //object paramObj = childrenList;
                //object paramObj2 = di;
                string[] deepArray = FullPathName.Split('\\');
                int deep = deepArray.Length - 1;
                if (!di.Exists) return childrenList;
                string[] directorys = InitPathName.Split('\\');

                string[] LastPath = { }, CalcPath;
                if (LastPathName != "")
                {
                    LastPath = LastPathName.Split('\\');
                    CalcPath = di.FullName.Split('\\');
                }


                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    CalcPath = dir.FullName.Split('\\');
                    int deeps = CalcPath.Length;
                    string expandedPath = "";
                    for(int i = 0; i < deeps; i++)
                    {
                        if(LastPathName == "" || LastPath.Length <= i)
                        {
                            break;
                        }
                        expandedPath += LastPath[i];
                        if (i != deeps - 1)
                        {
                            expandedPath += "\\";
                        }
                    }
                    //if (!dir.FullName.Contains(directorys[directorys.Length - 1]))
                    //    continue;
                    //if(deep == 5)
                    //{
                    //    if(!dir.Name.Equals("ResultData_Summary")){
                    //        continue;
                    //    }
                    //}
                    //if(dir.FullName)
                    bool IsExpand = false;
                    if(dir.FullName == expandedPath)
                    {
                        IsExpand = true;
                    }
                    item1 = new FolderItem();
                    item1.FullPathName = FullPathName + "\\" + dir.Name;
                    item1.FriendlyName = dir.Name;
                    item1.LastPathName = LastPathName;
                    item1.InitPathName = InitPathName;
                    item1.IsExpanded = IsExpand;
                    item1.IncludeFileChildren = this.IncludeFileChildren;
                    childrenList.Add(item1);
                }

                int count = 0;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (FileInfo file in di.GetFiles())
                    {
                        //Thread.Sleep(1);
                        if (file.Extension != ".csv")
                        {
                            //p_Progress = ((++count) * 100) / di.GetFiles().Length;
                            continue;
                        }
                        item1 = new FileItem();
                        item1.FullPathName = FullPathName + "\\" + file.Name;
                        item1.FriendlyName = file.Name;
                        item1.InitPathName = InitPathName;
                        childrenList.Add(item1);
                        //p_Progress = ((++count) * 100) / di.GetFiles().Length;
                    }
                }));
                sw.Stop();
                System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            return childrenList;
        }

        public override void SetExpandedIcon()
        {
            return;
            throw new NotImplementedException();
        }
    }

    public class FolderItem : NavTreeItem
    { 
        public override BitmapSource GetMyIcon()
        {
            string Param = "pack://application:,,/Resource/Folder.ico";
            Uri uri1 = new Uri(Param, UriKind.RelativeOrAbsolute);
            return myIcon = BitmapFrame.Create(uri1);
        }

        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            INavTreeItem item1;

            try
            {
                DirectoryInfo di = new DirectoryInfo(this.FullPathName); // may be acces not allowed
                //object paramObj = childrenList;
                //object paramObj2 = di;
                string[] deepArray = FullPathName.Split('\\');
                int deep = deepArray.Length - 1; 
                if (!di.Exists) return childrenList;
                string[] directorys = InitPathName.Split('\\');

                string[] LastPath = { }, CalcPath;
                if (LastPathName != "")
                {
                    LastPath = LastPathName.Split('\\');
                    CalcPath = di.FullName.Split('\\');
                }

                Regex regex = new Regex(@"[0-9]{4}-[0-9]{2}-[0-9]{2}");
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    if (regex.IsMatch(di.Name))
                    {
                        if (!dir.Name.Equals("ResultData_Summary"))
                            continue;
                    }
                    
                    CalcPath = dir.FullName.Split('\\');
                    int deeps = CalcPath.Length;
                    string expandedPath = "";
                    for (int i = 0; i < deeps; i++)
                    {
                        if (LastPathName == "" || LastPath.Length <= i)
                        {
                            break;
                        }

                        expandedPath += LastPath[i];
                        if (i != deeps - 1)
                        {
                            expandedPath += "\\";
                        }
                    }

                    bool IsExpand = false;
                    if (dir.FullName == expandedPath)
                    {
                        IsExpand = true;
                    }

                    item1 = new FolderItem();
                    item1.FullPathName = FullPathName + "\\" + dir.Name;
                    item1.FriendlyName = dir.Name;
                    item1.LastPathName = LastPathName;
                    item1.InitPathName = InitPathName;
                    item1.IsExpanded = IsExpand;
                    item1.IncludeFileChildren = this.IncludeFileChildren;
                    childrenList.Add(item1);
                }

                int count = 0;
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (FileInfo file in di.GetFiles())
                    {
                        //Thread.Sleep(1);
                        if (file.Extension != ".csv")
                        {
                            //p_Progress = ((++count) * 100) / di.GetFiles().Length;
                            continue;
                        }
                        bool isSelect = false;
                        if (LastPathName.Contains(".csv") && LastPathName == file.FullName)
                        {
                            isSelect = true;
                        }

                        item1 = new FileItem();
                        item1.FullPathName = FullPathName + "\\" + file.Name;
                        item1.FriendlyName = file.Name;
                        item1.IsSelected = isSelect;
                        item1.InitPathName = InitPathName;
                        childrenList.Add(item1);
                        //p_Progress = ((++count) * 100) / di.GetFiles().Length;
                    }
                }));
                sw.Stop();
                System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            return childrenList;
        }

     

        public override void SetExpandedIcon()
        {
            if (IsExpanded)
            {
                string Param = "pack://application:,,/Resource/openFolder.ico";
                Uri uri1 = new Uri(Param, UriKind.RelativeOrAbsolute);
                MyIcon = BitmapFrame.Create(uri1);
            }
            else
            {
                string Param = "pack://application:,,/Resource/Folder.ico";
                Uri uri1 = new Uri(Param, UriKind.RelativeOrAbsolute);
                MyIcon = BitmapFrame.Create(uri1);
                //MyIcon = Utils.GetIconFn.GetIconDll(this.FullPathName);
            }
            //throw new NotImplementedException();
        }
    }

    public class FileItem : NavTreeItem
    {
        public override BitmapSource GetMyIcon()
        {
            // to do, use a cache for .ext != "" or ".exe" or ".lnk"
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            //myIcon = Utils.GetIconFn.GetIconDll(this.FullPathName);
            //sw.Stop();
            //System.Diagnostics.Debug.WriteLine(sw.ElapsedMilliseconds);
            string Param = "pack://application:,,/Resource/csv.ico";
            Uri uri1 = new Uri(Param, UriKind.RelativeOrAbsolute);
            return myIcon = BitmapFrame.Create(uri1);
        }

        public override void SetExpandedIcon()
        {

            return;
        }

        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            return childrenList;
        }
    }
}
