using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace RootTools_Vision
{
    public class MapViewer_ViewModel : ObservableObject
    {
        private class MapViewerColorDefines
        {
            public static SolidColorBrush NoChip = Brushes.LightGray;
            public static SolidColorBrush Normal = Brushes.DimGray;
            public static SolidColorBrush Snap = Brushes.LightSkyBlue;
            public static SolidColorBrush Position = Brushes.LightYellow;
            public static SolidColorBrush Inspection = Brushes.Gold;
            public static SolidColorBrush ProcessDefect = Brushes.YellowGreen;
            public static SolidColorBrush ProcessDefectWafer = Brushes.Green;
        }

        public void CreateMap(int _sizeX, int _sizeY, int[] mapData = null)
        {
            this.MapSizeX = _sizeX;
            this.MapSizeY = _sizeY;

            CreateMap(mapData);
        }

        #region [Properties]
        private ObservableCollection<Rectangle> chipItems;
        public ObservableCollection<Rectangle> ChipItems
        {
            get => this.chipItems;
            set
            {
                SetProperty<ObservableCollection<Rectangle>>(ref this.chipItems, value);
            }
        }


        private int mapSizeX;
        public int MapSizeX
        {
            get => this.mapSizeX;
            set
            {
                SetProperty<int>(ref this.mapSizeX, value);
            }
        }
        private int mapSizeY;
        public int MapSizeY
        {
            get => this.mapSizeY;
            set
            {
                SetProperty<int>(ref this.mapSizeY, value);
            }
        }

        public Point SelectPoint { get; set; } = new Point(-1, -1);

        // OneWayToSource
        public double CanvasWidth { get; set; }
        public double CanvasHeight { get; set; }
        #endregion

        #region [Command]

        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RedrawMap();
                });
            }
        }
        public ICommand SizeChangedCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RedrawMap();
                });
            }
        }
        #endregion

        #region [Event]
        public virtual void Chip_MouseLeave(object sender, MouseEventArgs e) { }

        public virtual void Chip_MouseEnter(object sender, MouseEventArgs e) { }

        public virtual void Chip_MouseLeftUp(object sender, MouseEventArgs e) { }
        #endregion

        #region [Draw Method]
        private void CreateMap(int[] mapData = null)
        {
            if (this.mapSizeX == 0 || this.mapSizeY == 0) return;

            if (ChipItems == null) ChipItems = new ObservableCollection<Rectangle>();

            ChipItems.Clear();

            int sizeX = this.mapSizeX;
            int sizeY = this.mapSizeY;

            double chipWidth = (double)CanvasWidth / (double)sizeX;
            double chipHeight = (double)CanvasHeight / (double)sizeY;

            Point originPt = new Point(0, 0);

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    Rectangle rect = new Rectangle();
                    rect.Width = chipWidth;
                    rect.Height = chipHeight;
                    Canvas.SetLeft(rect, originPt.X + (chipWidth * x));
                    Canvas.SetTop(rect, originPt.Y + (chipHeight * y));

                    rect.Tag = new CPoint(x, y);
                    rect.ToolTip = string.Format("({0}, {1})", x, y); // chip index
                    rect.Stroke = Brushes.Transparent;
                    rect.Opacity = 0.7;
                    rect.StrokeThickness = 2;

                    if(mapData != null)
                    {
                        if(mapData[y * sizeX + x] == (int)CHIP_TYPE.NO_CHIP)
                        {
                            rect.Fill = Brushes.DimGray;
                        }
                        else if (mapData[y * sizeX + x] == (int)CHIP_TYPE.NORMAL)
                        {
                            rect.Fill = Brushes.YellowGreen;
                            rect.MouseEnter += Chip_MouseEnter;
                            rect.MouseLeave += Chip_MouseLeave;
                            rect.MouseLeftButtonUp += Chip_MouseLeftUp;
                        }
                        else if (mapData[y * sizeX + x] == (int)CHIP_TYPE.EXTRA)
                        {
                            rect.Fill = Brushes.Yellow;
                        }
                    }
                    else
                    {
                        rect.Fill = Brushes.DimGray;
                    }
                    
                    Canvas.SetZIndex(rect, 99);
                    ChipItems.Add(rect);
                }
            }

            RedrawMap();
        }

        private void RedrawMap()
        {
            if (this.mapSizeX == 0 || this.mapSizeY == 0) return;

            int sizeX = this.mapSizeX;
            int sizeY = this.mapSizeY;

            double chipWidth = (double)CanvasWidth / (double)sizeX;
            double chipHeight = (double)CanvasHeight / (double)sizeY;

            Point originPt = new Point(0, 0);

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    int index = x + y * sizeX;
                    Rectangle rect = ChipItems[index];

                    rect.Width = chipWidth;
                    rect.Height = chipHeight;
                    Canvas.SetLeft(rect, originPt.X + (chipWidth * x));
                    Canvas.SetTop(rect, originPt.Y + (chipHeight * y));
                }
            }
        }

        public void ResetMapColor()
        {
            SelectPoint = new Point();
            if (this.mapSizeX == 0 || this.mapSizeY == 0) return;

            int sizeX = this.mapSizeX;
            int sizeY = this.mapSizeY;

            double chipWidth = (double)CanvasWidth / (double)sizeX;
            double chipHeight = (double)CanvasHeight / (double)sizeY;

            Point originPt = new Point(0, 0);

            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    int index = x + y * sizeX;
                    Rectangle rect = ChipItems[index];

                    rect.Width = chipWidth;
                    rect.Height = chipHeight;
                    rect.Fill = Brushes.DimGray;

                    Canvas.SetLeft(rect, originPt.X + (chipWidth * x));
                    Canvas.SetTop(rect, originPt.Y + (chipHeight * y));
                }
            }
        }

        public void SetChipColor(int x, int y, SolidColorBrush color)
        {
            if (x < 0 || y < 0) return;

            int index = x + y * this.mapSizeX;
            if (index < ChipItems.Count)
            {
                ChipItems[index].Fill = color;
            }
            else
            {
                //MessageBox.Show("Chip Index를 벗어났습니다.");
            }
        }

        public void RefreshMap()
        {
            RedrawMap();
        }


        public void Clear()
        {
            this.ChipItems.Clear();
        }

        #endregion

    }
}