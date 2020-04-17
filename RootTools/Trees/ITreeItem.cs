using System;
namespace RootTools.Trees
{
    interface ITreeItem
    {
        void SetValue(dynamic value);

        dynamic GetValue(); 
    }
}
