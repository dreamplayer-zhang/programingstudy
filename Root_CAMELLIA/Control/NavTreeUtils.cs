using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace Root_CAMELLIA
{
    // General note: Manipulation of the Tree is straightforward in MODEL or VIEWMODEL and all kind of functions 
    // can be implemented such as GetParent, AddChild, AddUniqueChild, RemoveChild, SortChildren etc.

    // Here some procedures to support update/refresh the (FileSystem) tree and restore it somewhat to the old visual state
    // First try was to use Union, Intersect and Except using the current children and the new via GetChildren()
    // This became a little complicated, so here we asume that in general not much folders are opened

    // We take a snapshot of the expanded nodes of old tree, reset/build a new tree and try to open all open snapshot nodes
    // - NavTreeUtils.TakeSnapshot(ObservableCollection<INavTreeItem> rootChildren)
    // - NavTreeUtils.ExpandSnapShotItems(List<string> SnapShot, INavTreeItem treeRootItem)

    // - Initially only DriveRootItems were supported (Hirarchical Tree namespace). Simple snapshot suffices.
    //   Now we support also favorites at the cost of a more complex and larger snapshot as a concatenation of 
    //   succesive FullPathNames from root to the expanded item.
 
    //   to do?: 
    //   use a FileSystemWatcher to get list of updates, process these


    // Supporting class, some Tree Operations are easier on (Dummy) RootNode then on RootChildren
    public class RootNode : NavTreeItem
    {
        public override BitmapSource GetMyIcon()
        {
            return myIcon = null;
        }

        public override ObservableCollection<INavTreeItem> GetMyChildren()
        {
            return new ObservableCollection<INavTreeItem> { };
        }

        public override void SetExpandedIcon()
        {
            return;
            throw new NotImplementedException();
        }
    }

    public static class NavTreeUtils
    {
        static string strSeparator = "[+]";

        private static void VisitExpandedChildrenAndTakeSnapshot
                (INavTreeItem selectedNode, string currentName, ref List<string> snapShot)
        {
            // If node not expanded we do not refresh/repaint rest of the nodes
            if (selectedNode.IsExpanded)
            {
                string newCurrentName = (currentName == strSeparator) ? selectedNode.FullPathName : currentName + strSeparator + selectedNode.FullPathName;
                snapShot.Add(newCurrentName);
                //Console.WriteLine(selectedNode.FullPathName);

                for (int i = 0; i < selectedNode.Children.Count; i++)
                {
                    VisitExpandedChildrenAndTakeSnapshot(selectedNode.Children[i], newCurrentName, ref snapShot);
                }
            }
        }

        // Procedure used in NavTreeVm. First take snapshot, reconstruct new tree, expand items in snapshot 
        public static List<string> TakeSnapshot(ObservableCollection<INavTreeItem> rootChildren)
        {
            List<string> snapShot = new List<string> { };

            // Use a dummy rootnode, is easier to work with 
            RootNode rootNode = new RootNode();
            foreach (INavTreeItem item in rootChildren) rootNode.Children.Add(item);

            // Take snapshot of all expanded nodes
            // new: For handling all kinds of namespaces we take as snapshot concatination of consecutive [Fullnames+separator]
            // For a hierachical namespace currentName+separator not needed
            // Note: it seems possible taking only longest paths and remove subpaths strings from Snapshot
            // note: Length string?? Stringbuilder? 
            rootNode.IsExpanded = true;
            string currentName = "";
            VisitExpandedChildrenAndTakeSnapshot(rootNode, currentName, ref snapShot);

            return snapShot;
        }

        // Supporting procedure
        private static void GetNodeFromNameLocal(ref INavTreeItem item, string[] pathArray, int iLevel)
        {
            string name;

            // Check children
            name = pathArray[iLevel];
            INavTreeItem child;
            INavTreeItem selected = null;

            for (int i = 0; (i <= item.Children.Count() - 1) && (selected == null); i++)
            {
                child = item.Children[i]; 
                if (name == child.FullPathName) selected = child; 
            }

            item = selected;

            // If we have a hit, step deeper
            iLevel++;
            if ((iLevel <= pathArray.Length - 1) && (item != null)) GetNodeFromNameLocal(ref item, pathArray, iLevel);
        }

        private static void GetNodeFromName(INavTreeItem rootNode, string fullPathNames, ref INavTreeItem selectedNode)
        {
            // Just setup a call to GetNodeFromNameLocal to do the work

            // note: to copy or not to copy (pointer, content), all seems ok
            selectedNode = null;

            if ((fullPathNames == null) || (fullPathNames == ""))
            {
                return;
            }

            // make a pathArray. 
            // Note now it is not anymore [(drive) (folder)] but [(drive) [(drive) (folder)]]   
            string[] separator = new string[] { strSeparator };
            string[] pathArray = fullPathNames.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            if (pathArray.Length == 0) { return; };

            // Get the node holding the Items
            selectedNode = rootNode;

            int iLevel = 0;
            GetNodeFromNameLocal(ref selectedNode, pathArray, iLevel);
        }

        // Procedure used in NavTreeVm. First take snapshot, reconstruct new tree, expand items in snapshot 
        public static void ExpandSnapShotItems(List<string> SnapShot, INavTreeItem treeRootItem)
        {
            // try to open all old snapshot nodes
            INavTreeItem Selected = null;
            for (int i = 0; i < SnapShot.Count; i++)
            {
                GetNodeFromName(treeRootItem, SnapShot[i], ref Selected);
                if (Selected != null)
                {
                    Selected.IsExpanded = true;
                }
            }
        }
    }
}



