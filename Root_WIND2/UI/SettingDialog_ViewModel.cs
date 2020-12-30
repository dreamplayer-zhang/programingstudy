using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    class SettingDialog_ViewModel : ObservableObject
    {

        TreeDataCollection menuTree;
        public TreeDataCollection MenuTree
        {
            get => this.menuTree;
            set
            {
                SetProperty<TreeDataCollection>(ref this.menuTree, value);
            }
        }



        public void Init()
        {
            TreeDataCollection setupChild = new TreeDataCollection();
            setupChild.Add(new TreeData() { Name = "FrontSide" });
            setupChild.Add(new TreeData() { Name = "BackSide" });
            setupChild.Add(new TreeData() { Name = "Edge" });
            setupChild.Add(new TreeData() { Name = "EBR" });
            TreeData setup = new TreeData() { Name = "Setup", Children = setupChild };

            TreeDataCollection reviewChild = new TreeDataCollection();
            reviewChild.Add(new TreeData() { Name = "FrontSide" });
            reviewChild.Add(new TreeData() { Name = "BackSide" });
            reviewChild.Add(new TreeData() { Name = "Edge"});
            reviewChild.Add(new TreeData() { Name = "EBR" });
            TreeData review = new TreeData() { Name = "Review", Children = reviewChild };

            TreeDataCollection runChild = new TreeDataCollection();
            runChild.Add(new TreeData() { Name = "FrontSide" });
            runChild.Add(new TreeData() { Name = "BackSide" });
            runChild.Add(new TreeData() { Name = "Edge" });
            runChild.Add(new TreeData() { Name = "EBR" });
            TreeData run= new TreeData() { Name = "Run", Children = runChild };

            menuTree = new TreeDataCollection();
            menuTree.Add(setup);
            menuTree.Add(review);
            menuTree.Add(run);
        }

        public class TreeDataCollection : ObservableCollection<TreeData>
        {
            public TreeDataCollection() : base()
            {
                
            }
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

            private bool _isSelected;
            public bool IsSelected
            {
                get { return _isSelected; }
                set
                {
                    if (_isSelected != value)
                    {
                        _isSelected = value;
                        SetProperty<bool>(ref this._isSelected, value);
                    }
                }
            }

        }
    }
}
