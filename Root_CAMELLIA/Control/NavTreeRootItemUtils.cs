using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Root_CAMELLIA
{
    public static class NavTreeRootItemUtils
    {
        // See NavTreeItems for our Model of the tree with RootNode and RootItems 
        // RootNode.Children = RootItem(s) or direct RootItem(s).Children dependant RootNode.UseRootItemsAsChildren 

        // Convention: Root items end with:
        public const string LastPartRootItemName = "RootItem";

        // Using convention and reflection to construct a List<string> of RootItems defined in the code
        // Note: can we use also MEF??
        public static List<string> ListNavTreeRootItemsByConvention()
        {
            List<string> List = new List<string> { };
            // By convention: all classes that end with "RootItem" form the rootlist 
            // Use reflection for list of all NavTreeItem classes, 
            var entityTypes =
              from t in System.Reflection.Assembly.GetAssembly(typeof(NavTreeItem)).GetTypes() where t.IsSubclassOf(typeof(NavTreeItem)) select t;

            foreach (var t in entityTypes)
            {
                if (t.Name.EndsWith(LastPartRootItemName))
                {
                    //Console.Write("* Root * "); 
                    List.Add(t.Name.Replace(LastPartRootItemName, ""));
                }
                //Console.WriteLine(t.Name);
            }
            return List;
        }

        // Using convention and reflection to get RootItem iRootNr
        //If iRootNr>= ListNavTreeRootItemsByConvention().Count we use driveItem by default
        public static NavTreeItem ReturnRootItem(int iRootNr, bool includeFileChildren = false, string InitPath = "")
        {
            // Set default System.Type
            Type selectedType = typeof(DriveRootItem);
            string selectedName = "Folder";

            // Can you find other type given the conventions ..RootItem name and iRootNr
            var entityTypes =
              from t in System.Reflection.Assembly.GetAssembly(typeof(NavTreeItem)).GetTypes() where t.IsSubclassOf(typeof(NavTreeItem)) select t;

            int i = 0;
            foreach (var tt in entityTypes)
            {
                if (tt.Name.EndsWith(LastPartRootItemName))
                {
                    if (i == iRootNr)
                    {
                        selectedType = Type.GetType(tt.FullName);
                        selectedName = tt.Name.Replace(LastPartRootItemName, "");
                        break;
                    }
                    i++;
                }
            }

            // Use selectedType to create root ..         
            //NavTreeItem rootItem = (NavTreeItem)Activator.CreateInstance(selectedType, InitPath);
            NavTreeItem rootItem = (NavTreeItem)Activator.CreateInstance(selectedType, InitPath);
            rootItem.FriendlyName = selectedName;
            rootItem.IncludeFileChildren = includeFileChildren;

            return rootItem;
        }
    }
}
