﻿using RootTools;
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
        public MapViewer_ViewModel()
        {
            ChipItems = new ObservableCollection<Rectangle>();
        }


        public void CreateMap(int _sizeX, int _sizeY)
        {
            this.MapSizeX = _sizeX;
            this.MapSizeY = _sizeY;

            CreateMap();
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

        #region [Draw Method]
        private void CreateMap()
        {
            if (this.mapSizeX == 0 || this.mapSizeY == 0) return;

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
                    rect.Fill = Brushes.DimGray;

                    Canvas.SetZIndex(rect, 99);
                    //rect.MouseLeftButtonDown += ChipMouseLeftButtonDown;

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
                MessageBox.Show("Chip Index를 벗어났습니다.");
            }
        }


        public void Clear()
        {
            this.ChipItems.Clear();
        }

        #endregion

    }
}