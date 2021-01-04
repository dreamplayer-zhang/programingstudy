using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.UserControls
{
    class BasicTreeView_ViewModel : ObservableObject
    {
        ObservableCollection<TreeData> treeItems;
        public ObservableCollection<TreeData> TreeItems
        {
            get => this.treeItems;
            set
            {
                SetProperty<ObservableCollection<TreeData>>(ref this.treeItems, value);
            }
        }

        public void Init()
        {
            ObservableCollection<TreeData> child11 = new ObservableCollection<TreeData>();
            child11.Add(new TreeData() { Name = "aa" });
            child11.Add(new TreeData() { Name = "bb" });

            ObservableCollection<TreeData> child1 = new ObservableCollection<TreeData>();
            child1.Add(new TreeData() { Name = "AAAA" });
            child1.Add(new TreeData() { Name = "BBBB" });
            child1.Add(new TreeData() { Name = "CCCC", Children = child11 });

            ObservableCollection<TreeData> child2 = new ObservableCollection<TreeData>();
            child2.Add(new TreeData() { Name = "TTT" });
            child2.Add(new TreeData() { Name = "FF" });
            child2.Add(new TreeData() { Name = "QQQQQ" });

            treeItems = new ObservableCollection<TreeData>();
            treeItems.Add(new TreeData() { Name = "1", Children = child1 });
            treeItems.Add(new TreeData() { Name = "2", Children = child2 });
        }


        public class TreeData : ObservableObject
        {
            private string name;

            public string Name
            {
                get => name;
                set
                {
                    SetProperty<string>(ref this.name, value);
                }
            }


            private ObservableCollection<TreeData> children;

            public ObservableCollection<TreeData> Children
            {
                get { return children; }
                set
                {
                    SetProperty<ObservableCollection<TreeData>>(ref this.children, value);
                }
            }

        }
    }
}
