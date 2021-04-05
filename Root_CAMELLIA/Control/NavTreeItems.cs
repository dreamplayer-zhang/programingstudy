using System.Windows.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Threading;

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

            //string[] allDrives = System.Environment.GetLogicalDrives();
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in allDrives)
                if (drive.IsReady && drive.Name.Contains("C"))
                {
                    item1 = new DriveItem();

                    // Some processing for the FriendlyName
                    fn = drive.Name.Replace(@"\", "");
                    item1.FullPathName = fn;
                    if (drive.VolumeLabel == string.Empty)
                    {
                        //fn = drive.DriveType.ToString() + " (" + fn + ")";
                        fn =" (" + fn + ")";
                    }
                    else if (drive.DriveType == DriveType.CDRom)
                    {
                        fn = drive.DriveType.ToString() + " " + drive.VolumeLabel + " (" + fn + ")";
                    }
                    else
                    {
                        fn = drive.VolumeLabel + " (" + fn + ")";
                    }

                    item1.FriendlyName = fn;
                    item1.InitPathName = InitPathName;
                    item1.IsExpanded = true;
                    item1.IncludeFileChildren = this.IncludeFileChildren;
                    childrenList.Add(item1);
                }

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
            ObservableCollection<INavTreeItem> childrenList = new ObservableCollection<INavTreeItem>() { };
            INavTreeItem item1;

            DriveInfo drive = new DriveInfo(this.FullPathName);
            if (!drive.IsReady) return childrenList;

            DirectoryInfo di = new DirectoryInfo(((DriveInfo)drive).RootDirectory.Name);
            if (!di.Exists) return childrenList;


            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                if(!(di.Name == @"C:\" && dir.Name.Contains("Camellia2")))
                {
                    continue;
                }
                item1 = new FolderItem();
                item1.FullPathName = FullPathName + "\\" + dir.Name;
                item1.FriendlyName = dir.Name;
                item1.InitPathName = InitPathName;
                item1.IsExpanded = true;
                item1.IncludeFileChildren = this.IncludeFileChildren;
                childrenList.Add(item1);
            }

            //if (this.IncludeFileChildren)
            {
                if (di.FullName.Contains("Camellia2"))
                {
                    foreach (FileInfo file in di.GetFiles())
                    {
                        item1 = new FileItem();
                        item1.FullPathName = FullPathName + "\\" + file.Name;
                        item1.FriendlyName = file.Name;
                        item1.InitPathName = InitPathName;
                        childrenList.Add(item1);
                    }
                }

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
                if (!di.Exists) return childrenList;
                string[] directorys = InitPathName.Split('\\');
                
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    if (!dir.FullName.Contains(directorys[directorys.Length - 1]))
                        continue;
                    item1 = new FolderItem();
                    item1.FullPathName = FullPathName + "\\" + dir.Name;
                    item1.FriendlyName = dir.Name;
                    item1.InitPathName = InitPathName;
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
