using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2
{
    public class SettingDialog_ViewModel : ObservableObject, IDialogRequestClose
    {
        #region [Parameters]
        TreeItemCollection menuItems;
        public TreeItemCollection MenuItems
        {
            get => this.menuItems;
            set
            {
                SetProperty<TreeItemCollection>(ref this.menuItems, value);
            }
        }

        List<ISettingData> settingDataList;
        public List<ISettingData> SettingDataList
        {
            get => this.settingDataList;
            set
            {
                this.settingDataList = value;
            }
        }

        private ISettingData selectedSettingData;

        public ISettingData SelectedSettingData
        {
            get { return selectedSettingData; }
            set
            {
                SetProperty<ISettingData>(ref this.selectedSettingData, value);
            }
        }

        #endregion

        #region [Commands]
        private ICommand selectedItemCommand;

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public ICommand SelectedItemCommand
        {
            get
            {
                return selectedItemCommand ?? (selectedItemCommand = new RelayCommand(OnSelectedItemChanged_Callback));
            }
        }

        public RelayCommand btnOKCommand
        {
            get
            {
                return new RelayCommand(OnButtonClickedOK);
            }
        }
        public RelayCommand btnCancelCommand
        {
            get
            {
                return new RelayCommand(OnButtonClickedCancel);
            }
        }

        #endregion

        #region [Commnads Callback]



        public void OnButtonClickedOK()
        {
            Save();
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        public void OnButtonClickedCancel()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }

        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                });
            }
        }

        public void OnSelectedItemChanged_Callback()
        {
            TreeItem item = MenuItems.GetSelectedItem();
            string[] FullPath = item.FullPath;

            foreach(SettingData data in SettingDataList)
            {
                if (data.GetTreeViewPath().Length != item.FullPath.Length)
                    continue;

                for (int i = 0; i < FullPath.Length; i++)
                {
                    
                    if(data.GetTreeViewPath()[i] == item.FullPath[i])
                    {
                        if(i == (FullPath.Length - 1))
                        {
                            SelectedSettingData = data;
                            return;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        #endregion


        public SettingDialog_ViewModel()
        {
            MenuItems = new TreeItemCollection();
            SettingDataList = new List<ISettingData>();


            SetMenuItems();
            Load();
        }

        public void Save()
        {
            for(int i = 0; i < SettingDataList.Count; i++)

            {
                SettingDataList[i].Save();
            }
        }

        public void Load()
        {
            for(int i = 0; i < SettingDataList.Count; i++)
            {
                SettingDataList[i].Load(SettingDataList[i]);
            }
        }

        public void SetMenuItems()
        {
            menuItems.SetTreeItem("Setup");
            SettingDataList.Add(new SettingData_Setup(new string[] { "Setup" }));

            menuItems.SetTreeItem("Setup", "Frontside");
            SettingDataList.Add(new SettingData_SetupFrontside(new string[] { "Setup", "Frontside" }));

            menuItems.SetTreeItem("Setup", "Frontside", "Get out of Here");
            SettingDataList.Add(new SettingData_SetupFrontside(new string[] { "Setup", "Frontside", "Get out of Here" }));

            menuItems.SetTreeItem("Setup", "Backside");
            SettingDataList.Add(new SettingData_SetupBackside(new string[] { "Setup", "Backside" }));

            menuItems.SetTreeItem("Setup", "Edge");
            menuItems.SetTreeItem("Setup", "EBR");

            menuItems.SetTreeItem("Review");
            menuItems.SetTreeItem("Review", "Frontside");
            menuItems.SetTreeItem("Review", "Backside");
            menuItems.SetTreeItem("Review", "Edge");
            menuItems.SetTreeItem("Review", "EBR");

            menuItems.SetTreeItem("Run", "Frontside");
            menuItems.SetTreeItem("Run", "Backside");
            menuItems.SetTreeItem("Run", "Edge");
            menuItems.SetTreeItem("Run", "EBR");
        }

        public class TreeItemCollection : ObservableCollection<TreeItem>
        {
            public TreeItemCollection() : base()
            {
                
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="treeName">Tree 깊이 순서로 TreeName을 넣어준다.</param>
            public void SetTreeItem(params string[] treeItemNames)
            {
                TreeItem item = null;
                TreeItem parent = null;
                TreeItemCollection itemCollection = this;
                for (int level = 0; level < treeItemNames.Length; level++)
                {
                    item = null;
                    foreach (TreeItem temp in itemCollection)
                    {
                        if (temp.Name == treeItemNames[level])
                        {
                            item = temp;
                            parent = temp;
                            itemCollection = temp.Children;
                            break;
                        }
                    }
                    if (item == null)
                    {
                        TreeItem newItem = new TreeItem() { Name = treeItemNames[level], Parent = parent };
                        itemCollection.Add(newItem);
                        parent = newItem;
                        itemCollection = newItem.Children;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="treeName">Tree 깊이 순서로 TreeName을 넣어준다.</param>
            private TreeItem GetItem(params string[] treeItemNames)
            {
                TreeItem item = null;
                TreeItemCollection itemCollection = this;
                for(int level = 0; level < treeItemNames.Length; level++)
                {
                    item = null;
                    foreach (TreeItem temp in itemCollection)
                    {
                        if (temp.Name == treeItemNames[level])
                        {
                            item = temp;
                            itemCollection = temp.Children;
                            break;
                        }                        
                    }
                }

                return item;
            }

            public TreeItem GetSelectedItem()
            {
                return FindSelectedItem(this);
            }

            private TreeItem FindSelectedItem(TreeItemCollection treeItems)
            {
                foreach(TreeItem item in treeItems)
                {
                    if (item.IsSelected == true)
                        return item;

                    if (item.HasChildren == true)
                    {
                        TreeItem temp = FindSelectedItem(item.Children);
                        if (temp != null) return temp;
                    }
                }

                return null;
            }
        }


        public class TreeItem : ObservableObject
        {
            public TreeItem()
            {
                Children = new TreeItemCollection();
            }

            private string name;

            public string Name 
            { 
                get => name; 
                set 
                {
                    SetProperty<string>(ref this.name, value);
                }
            }

            private TreeItem parent;
            public TreeItem Parent
            {
                get { return this.parent; }
                set { this.parent = value; }
            }

            private TreeItemCollection children;

            public TreeItemCollection Children
            {
                get { return children; }
                set 
                { 
                    SetProperty<TreeItemCollection>(ref this.children, value); 
                }
            }

            private bool isSelected;
            public bool IsSelected
            {
                get { return isSelected; }
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        SetProperty<bool>(ref this.isSelected, value);
                    }
                }
            }

            public bool HasChildren
            {
                get { return (Children.Count > 0); }
            }

            public string[] FullPath
            {
                get
                {
                    return GetFullPathString(this).Split('.');
                }
            }

            private string GetFullPathString(TreeItem item)
            {
                if (item.Parent == null)
                    return item.Name;
                else
                    return GetFullPathString(item.Parent) + "." + item.Name;

            }
        }
    }
}
