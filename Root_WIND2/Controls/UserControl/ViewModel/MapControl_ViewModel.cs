using RootTools;
using RootTools.Inspects;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Recipe = RootTools_Vision.Recipe;

namespace Root_WIND2
{
    class MapControl_ViewModel : ObservableObject
    {
        Recipe m_Recipe;

        public delegate void setMasterDie(object e);
        public event setMasterDie SetMasterDie;
        public int[] Map;
        CPoint MapSize;
        public MapControl_ViewModel(Recipe recipe = null)
        {
            if(recipe != null)
                m_Recipe = recipe;

            WorkEventManager.WorkplaceStateChanged += MapStateChanged_Callback;
        }


        SolidColorBrush brushSnap = System.Windows.Media.Brushes.LightSkyBlue;
        SolidColorBrush brushPosition = System.Windows.Media.Brushes.SkyBlue;
        SolidColorBrush brushPreInspection = System.Windows.Media.Brushes.Cornsilk;
        SolidColorBrush brushInspection = System.Windows.Media.Brushes.Gold;
        SolidColorBrush brushMeasurement = System.Windows.Media.Brushes.CornflowerBlue;
        SolidColorBrush brushComplete = System.Windows.Media.Brushes.YellowGreen;
        SolidColorBrush brushCompleteWafer = System.Windows.Media.Brushes.LimeGreen;
        SolidColorBrush brushBadChip = System.Windows.Media.Brushes.Red;

        object lockObj = new object();
        private void MapStateChanged_Callback(object obj, WorkplaceStateChangedEventArgs args)
        {
            //lock(lockObj) //여기 lock걸면 deaklock 발생
            try
            {
                Workplace workplace = args.workplace;

                int x = workplace.MapIndexX;
                int y = workplace.MapIndexY;

                if (x < 0 || y < 0) return;

                WORK_TYPE state = workplace.WorkState;

                Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    if (p_MapItems.Count == 0) return;

                    int index = (int)(y + (x * MapSize.Y));
                    if (index > p_MapItems.Count - 1)
                        return;

                    if (workplace.GetSubState(WORKPLACE_SUB_STATE.BAD_CHIP) == false)
                    {

                        Grid chip = p_MapItems[index];
                        lock (lockObj) //여기다가 lock걸면 화면 최대/최소화 시 맵이 회색으로 변함
                        {
                            switch (state)
                            { 
                                case WORK_TYPE.NONE:
                                    //tb.Background = brushPosition;
                                    break;
                                case WORK_TYPE.SNAP:
                                    chip.Background = brushPreInspection;
                                break;
                                case WORK_TYPE.ALIGNMENT:
                                    chip.Background = brushPosition;
                                break;
                                case WORK_TYPE.INSPECTION:
                                    chip.Background = brushInspection;
                                break;
                                case WORK_TYPE.DEFECTPROCESS:
                                    chip.Background = brushComplete;
                                break;
                                case WORK_TYPE.DEFECTPROCESS_WAFER:
                                    chip.Background = brushCompleteWafer;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Grid chip = p_MapItems[index];

                        lock(this.lockObj) chip.Background = brushBadChip;
                    }
                }));
            }
            catch(Exception ex)
            {
                MessageBox.Show("Map State Change Error\n");
            }
        }



        public void CreateMapUI(int[] map = null, CPoint mapsize = null)
        {
            // 여기 예외처리 이상함
            if (map == null ||  map.Length == 0)
            {
                if (Map == null || Map.Length == 0) return;

                map = Map;
                mapsize = MapSize;
            }
           
            double chipWidth = p_width / mapsize.X;
            double chipHeight = p_height / mapsize.Y;


            p_MapItems.Clear();

            for (int i = 0; i < mapsize.X; i++)
                for (int j = 0; j < mapsize.Y; j++)
                {
                    Grid chip = new Grid();
                    chip.Width = chipWidth *0.9;
                    chip.Height = chipHeight *0.9;
                    chip.Margin = new Thickness(3);
                    chip.Tag = new CPoint(i, j);

                    Canvas.SetTop(chip, j * chipHeight);
                    Canvas.SetLeft(chip,  i * chipWidth);
                    if (map[i + (j * mapsize.X)] == 0)
                        chip.Background = Brushes.LightGray;
                    else
                        chip.Background = Brushes.Gray;

                    p_MapItems.Add(chip);
                }
        }
        public void CreateMap_OriginToolUI(bool addEvent, CPoint masterDie, int[] map = null, CPoint mapsize = null)
        {
            if (map == null)
            {
                map = Map;
                mapsize = MapSize;
            }

            double chipWidth = p_width / mapsize.X;
            double chipHeight = p_height / mapsize.Y;


            p_MapItems.Clear();

            for (int i = 0; i < mapsize.X; i++)
                for (int j = 0; j < mapsize.Y; j++)
                {
                    Grid chip = new Grid();
                    chip.Width = chipWidth * 0.9;
                    chip.Height = chipHeight * 0.9;
                    chip.Margin = new Thickness(3);
                    chip.Tag = new CPoint(i, j);

                    Canvas.SetTop(chip, j * chipHeight);
                    Canvas.SetLeft(chip, i * chipWidth);
                    if (map[i + (j * mapsize.X)] == (int)CHIP_TYPE.NO_CHIP)
                        chip.Background = Brushes.LightGray;
                    else // (int)CHIP_TYPE.NORMAL
                        chip.Background = Brushes.Green;


                    p_MapItems.Add(chip);

                    if(addEvent)
                        chip.MouseLeftButtonDown += MAP_MouseLeftButtonDown;
                }

            if(masterDie.X != -1)
                p_MapItems[this.MapSize.Y * masterDie.X + masterDie.Y].Background = Brushes.Purple;
        }
        public void SetMap(int[] map = null, CPoint mapsize = null)
        {
            if (map == null)
            {
                map = Map;
                mapsize = MapSize;
            }

            MapSize = new CPoint(mapsize.X, mapsize.Y);
            Map = new int[mapsize.X * mapsize.Y];
            Map = map;

            CreateMapUI();
            
        }
        public void SetMap(bool addEvent, CPoint masterDie, int[] map = null, CPoint mapsize = null)
        {
            if (map == null)
            {
                map = Map;
                mapsize = MapSize;
            }

            MapSize = new CPoint(mapsize.X, mapsize.Y);
            Map = new int[mapsize.X * mapsize.Y];
            Map = map;

            CreateMap_OriginToolUI(addEvent, masterDie);
        }
        private void MAP_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Grid selected = (Grid)sender;
            CPoint pos = (CPoint)selected.Tag;

            if (this.Map[this.MapSize.X * pos.Y + pos.X] != (int)CHIP_TYPE.NO_CHIP)
            {
                p_MapItems[this.MapSize.Y * m_Recipe.WaferMap.MasterDieX + m_Recipe.WaferMap.MasterDieY].Background = Brushes.Green;             
                selected.Background = Brushes.Purple;

                m_Recipe.WaferMap.MasterDieX = pos.X;
                m_Recipe.WaferMap.MasterDieY = pos.Y;

                SetMasterDie(pos);
            }
        }
        public void ChangeMasterImage(int dieX, int dieY)
        {
            if (this.Map.Length == 0) return;

            if(this.Map[this.MapSize.X * dieY + dieX] != (int)CHIP_TYPE.NO_CHIP)
            {
                p_MapItems[this.MapSize.Y * m_Recipe.WaferMap.MasterDieX + m_Recipe.WaferMap.MasterDieY].Background = Brushes.Green;
                p_MapItems[this.MapSize.Y * dieX + dieY].Background = Brushes.Purple;
            }
        }
        public int[] GetMap()
        {
            return Map;
        }

        private ObservableCollection<Grid> m_MapItems = new ObservableCollection<Grid>();
        public ObservableCollection<Grid> p_MapItems
        {
            get
            {
                return m_MapItems;
            }
            set
            {
                SetProperty(ref m_MapItems, value);
            }
        }

        private double m_width = 100;
        public double p_width
        {
            get
            {
                return m_width;
            }
            set
            {
                if (value == 0)
                    return;
                SetProperty(ref m_width, value);
                CreateMapUI();
            }
        }
        private double m_height = 100;
        public double p_height
        {
            get
            {
                return m_height;
            }
            set
            {
                if (value == 0)
                    return;
                SetProperty(ref m_height, value);
                CreateMapUI();

            }
        }
    }
}
