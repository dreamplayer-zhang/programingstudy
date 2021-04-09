using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using RootTools;

namespace Root_CAMELLIA
{
    // Model of tree items: NavTreeItem

    // This file: (INavTabItem) - (NavTabItem abstract Class as basis) 

    // See File NavTreeItems.cs for more specific NavTreeItem classes and how we use them. 
    // These specific classes define own method for icon, children and constructor
    
    // Interface INavTreeItem, just summary of class 
    // Normally better to define smaller interfaces and then compose INavTreeItem for a SOLID basis 
    public interface INavTreeItem : INotifyPropertyChanged
    {
        // For text in treeItem
        string FriendlyName { get; set; }

        // Image used in TreeItem
        BitmapSource MyIcon { get; set; }

        // Drive/Folder/File naming scheme to retrieve children
        string FullPathName { get; set; }

        string InitPathName { get; set; }

        ObservableCollection<INavTreeItem> Children { get; }

        bool IsExpanded { get; set; }

        // Design decisions: 
        // - do we use INotifyPropertyChanged. Maybe not quite aproporiate in model, but without MVVM framework practical shortcut
        // - do we introduce IsSelected, in most cases I would advice: Yes. I use now button+command to set Path EACH time item pressed
        bool IsSelected { get; set; }
        // void DeselectAll();

        // Specific for this application, could be introduced later in more specific interface/classes
        bool IncludeFileChildren { get; set; }

        // For resetting the tree
        void DeleteChildren();
    }

    // Abstact classs next step to implementation
    public abstract class NavTreeItem : ObservableObject, INavTreeItem
    {
        // for display in tree
        public string FriendlyName { get; set; }
        public string InitPathName { get; set; }
        protected BitmapSource myIcon;
        public BitmapSource MyIcon 
        {
            get { return myIcon ?? (myIcon = GetMyIcon()); } 
            set {
                SetProperty(ref myIcon, value, "MyIcon"); } 
        }

        // .. to retrieve info
        public string FullPathName { get; set; }

        // Question/ to do. Note that to be sure we use ObservableCollection as property with a notification, remove notification??
        protected ObservableCollection<INavTreeItem> children;
        public ObservableCollection<INavTreeItem> Children
        {
            get {

                children = GetMyChildren();
                return children;
            }
            set { SetProperty(ref children, value, "Children"); }
        }
        
        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set 
            {
                SetProperty(ref isExpanded, value, "IsExpanded");
                //if (value)
                //{
                //    Children = GetMyChildren();
                //}
                SetExpandedIcon();
            }
        }

        private bool isSelected = false;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                SetProperty(ref isSelected, value);
            }
        }

        bool m_HasItem = false;
        public bool p_HasItem
        {
            get
            {
                return m_HasItem;
            }
            set
            {
                SetProperty(ref m_HasItem, value);
            }
        }

        bool m_Update = false;
        public bool p_Update
        {
            get
            {
                return m_Update;
            }
            set
            {
                SetProperty(ref m_Update, value);
            }
        }

        public double p_Progress
        {
            get
            {
                return m_Progress;
            }
            set
            {
                SetProperty(ref m_Progress, value, "p_Progress");
                //if (value < 30)
                //{
                //    p_ProgressColor = System.Windows.Media.Brushes.Red;
                //}
                //else if (value < 65)
                //{
                //    p_ProgressColor = System.Windows.Media.Brushes.Yellow;
                //}
                //else if (value < 90)
                //{
                //    p_ProgressColor = test;
                //}
                //else if (value == 100)
                //{
                //    p_ProgressColor = System.Windows.Media.Brushes.Blue;
                //}
            }
        }
        private double m_Progress = 0;

        public bool IncludeFileChildren { get; set; }

        // We will define these Methods in other derived classes ...
        public abstract BitmapSource GetMyIcon();
        public abstract void SetExpandedIcon();
        public abstract ObservableCollection<INavTreeItem> GetMyChildren();

        public void DeleteChildren()
        {
            if (children != null)
            {
                // Console.WriteLine(this.FullPathName);

                for (int i = children.Count - 1; i >= 0; i--)
                {
                    children[i].DeleteChildren();
                    //children[i] = null;
                    children.RemoveAt(i);
                }

                children = null;
            }
        }
    }
}

