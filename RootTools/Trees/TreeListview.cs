using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.Trees
{
    public class TreeListView : TreeView
    {
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(GridViewColumnCollection), typeof(TreeListView), new FrameworkPropertyMetadata(new GridViewColumnCollection(), FrameworkPropertyMetadataOptions.None));

        public GridViewColumnCollection Columns
        {
            get { return (GridViewColumnCollection)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        public TreeListView() : base()
        {
            DefaultStyleKey = typeof(TreeListView);
            Columns = new GridViewColumnCollection();
            this.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(SelEventHandler);
        }

        void SelEventHandler(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (SelectedItem != null) SetValue(SelectedItem_Property, SelectedItem);
        }

        public object SelectedItem_
        {
            get { return (object)GetValue(SelectedItem_Property); }
            set { SetValue(SelectedItem_Property, value); }
        }
        public static readonly DependencyProperty SelectedItem_Property = DependencyProperty.Register("SelectedItem_", typeof(object), typeof(TreeListView), new UIPropertyMetadata(null));

    }

    public class TreeListViewItem : TreeViewItem
    {
        private int level = -1;
        public int Level
        {
            get
            {
                if (level != -1) { return level; }

                var parent = ItemsControlFromItemContainer(this) as TreeListViewItem;
                level = (parent != null) ? parent.Level + 1 : 0;
                return level;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        public TreeListViewItem()
        {
            DefaultStyleKey = typeof(TreeListViewItem);
        }
    }

    public class TreeViewHelper
    {
        private static Dictionary<DependencyObject, TreeViewSelectedItemBehavior> behaviors = new Dictionary<DependencyObject, TreeViewSelectedItemBehavior>();

        public static object GetSelectedItem(DependencyObject obj)
        {
            return (object)obj.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.RegisterAttached("SelectedItem", typeof(object), typeof(TreeViewHelper), new UIPropertyMetadata(null, SelectedItemChanged));

        private static void SelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is TreeView)) return;
            if (!behaviors.ContainsKey(obj)) behaviors.Add(obj, new TreeViewSelectedItemBehavior(obj as TreeView));
            TreeViewSelectedItemBehavior view = behaviors[obj];
            view.ChangeSelectedItem(e.NewValue);
        }

        private class TreeViewSelectedItemBehavior
        {
            TreeView view;
            public TreeViewSelectedItemBehavior(TreeView view)
            {
                this.view = view;
                view.SelectedItemChanged += (sender, e) => SetSelectedItem(view, e.NewValue);
            }

            internal void ChangeSelectedItem(object p)
            {
                TreeViewItem item = (TreeViewItem)view.ItemContainerGenerator.ContainerFromItem(p);
                item.IsSelected = true;
            }
        }
    }
}