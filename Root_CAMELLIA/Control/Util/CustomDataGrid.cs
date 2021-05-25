using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Root_CAMELLIA
{
    class CustomDataGrid
    {
        public static readonly DependencyProperty SelectingItemProperty = DependencyProperty.RegisterAttached(
            "SelectingItem",
            typeof(int),
            typeof(CustomDataGrid),
            new PropertyMetadata(default(int), OnSelectingItemChanged));

        public static int GetSelectingItem(DependencyObject target)
        {
            return (int)target.GetValue(SelectingItemProperty);
        }

        public static void SetSelectingItem(DependencyObject target, int value)
        {
            target.SetValue(SelectingItemProperty, value);
        }

        static void OnSelectingItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as DataGrid;

            grid.Dispatcher.BeginInvoke((Action)(() =>
            {
                grid.UpdateLayout();
                if (grid.Items.Count <= 0)
                {
                    return;
                }
                //grid.sor(0);
                //grid.Items.SortDescriptions.Add(new SortDescription("No", ListSortDirection.Descending));
                //grid.CanUserSortColumns = true;
                //grid.ColumnFromDisplayIndex(0).SortDirection = ListSortDirection.Descending;
                //grid.Items.SortDescriptions.

                grid.ScrollIntoView(grid.Items[Convert.ToInt32(e.NewValue)]);
            }));
            //var grid = sender as DataGrid;
            //if (grid == null || grid.SelectedItem == null)
            //    return;

            //// Works with .Net 4.5
            //grid.Dispatcher.InvokeAsync(() =>
            //{
            //    grid.UpdateLayout();
            //    grid.ScrollIntoView(grid.SelectedItem, null);
            //});

            //// Works with .Net 4.0
            //grid.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    grid.UpdateLayout();
            //    grid.ScrollIntoView(grid.SelectedItem, null);
            //}));
        }
    }
}
