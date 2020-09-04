using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RootTools_Vision
{
    public class GridHelper
    {
        #region [MapSize Property]
        public static readonly DependencyProperty MapSizeProperty =
        DependencyProperty.RegisterAttached(
        "MapSize", typeof(Point), typeof(GridHelper),
        new PropertyMetadata(new Point(0, 0), MapSizeChanged));

        public static Point GetMapSize(DependencyObject obj)
        {
            return (Point)obj.GetValue(MapSizeProperty);
        }

        // Set
        public static void SetMapSize(DependencyObject obj, Point value)
        {
            obj.SetValue(MapSizeProperty, value);
        }

        public static void MapSizeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Point ptSize = (Point)e.NewValue;
            if (!(obj is Grid) || ptSize.X < 0 || ptSize.Y < 0)
                return;

            Grid grid = (Grid)obj;

            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            grid.ColumnDefinitions.Clear();


            for (int i = 0; i < (int)ptSize.X; i++)
                grid.RowDefinitions.Add(
                    new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            for (int i = 0; i < (int)ptSize.Y; i++)
                grid.ColumnDefinitions.Add(
                    new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            ObservableCollection<UIElement> elements = new ObservableCollection<UIElement>();
            for (int y = 0; y < ptSize.Y; y++)
            {
                for (int x = 0; x < ptSize.X; x++)
                {

                    double cx = (double)(ptSize.X) / 2.0f;
                    double cy = (double)(ptSize.Y) / 2.0f;

                    double r = Math.Sqrt((x - cx ) * (x - cx) + (y - cy ) * (y - cy));

                    TextBox tb = new TextBox();
                   
                    if (r <= cx)
                    {
                        tb.Background = Brushes.Aqua;
                    }
                    else
                    {
                        tb.Background = Brushes.LightGray;
                    }

                    //((ObservableCollection<UIElement>)items.ItemsSource).Add(tb);

                    elements.Add(tb);
                    Grid.SetRow(tb, y);
                    Grid.SetColumn(tb, x);
                }
            }

            SetMapSize(grid, ptSize);
            SetCellItems(grid, elements);
        }
        #endregion

        #region [CellItems Property]

        /// <summary>
        /// Adds the specified number of Rows to RowDefinitions. 
        /// Default Height is Auto
        /// </summary>
        public static readonly DependencyProperty CellItemsProperty =
            DependencyProperty.RegisterAttached(
                "CellItems", typeof(ObservableCollection<UIElement>), typeof(GridHelper),
                new PropertyMetadata(new ObservableCollection<UIElement>(), CellItemsChanged));

        // Get
        public static ObservableCollection<UIElement> GetCellItems(DependencyObject obj)
        {
            return (ObservableCollection<UIElement>)obj.GetValue(CellItemsProperty);
        }

        // Set
        public static void SetCellItems(DependencyObject obj, ObservableCollection<UIElement> value)
        {
            obj.SetValue(CellItemsProperty, value);
        }

        // Change Event - Adds the Rows
        public static void CellItemsChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            Grid grid = (Grid)obj;

            ObservableCollection<UIElement> elements = (ObservableCollection<UIElement>)e.NewValue;
            grid.Children.Clear();
            foreach(UIElement element in elements)
            {
                grid.Children.Add(element);
            }
            SetCellItems(obj, (ObservableCollection<UIElement>)e.NewValue);
        }

        #endregion
    }
}
