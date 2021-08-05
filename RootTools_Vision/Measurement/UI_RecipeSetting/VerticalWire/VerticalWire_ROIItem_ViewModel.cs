using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision
{
    public class VerticalWire_ROIItem_ViewModel : ObservableObject
    {
        public delegate void ChangedEventHandler();
        public event ChangedEventHandler CollectionChanged;

        public ArrangeType eArrangeType;
        public List<Point> WirePointList = new List<Point>();

        #region [Properties]
        private int itemIndex;
        public int ItemIndex
        {
            get => this.itemIndex;
            set
            {
                SetProperty<int>(ref this.itemIndex, value);
            }
        }

        private int threshold = 50;
        public int Threshold
        {
            get => this.threshold;
            set
            {
                int threshVal = (value < 0) ? 0 : value ;
                threshVal = (threshVal > 255) ? 255 : value;

                SetProperty<int>(ref this.threshold, threshVal);
            }
        }

        private int wireSize = 20;
        public int WireSize
        {
            get => this.wireSize;
            set
            {
                int wireSize = (value < 0) ? 0 : value;

                SetProperty<int>(ref this.threshold, wireSize);
            }

        }
        private string roiHeader;
        public string ROIHeader
        {
            get => this.roiHeader;
            set
            {
                SetProperty<string>(ref this.roiHeader, value);
            }
        }

        private ObservableCollection<string> arrangeMethod;
        public ObservableCollection<string> ArrangeMethod
        {
            get => this.arrangeMethod;
            set
            {
                arrangeMethod = value;
            }
        }

        private int selectedArrageMethod = 0;
        public int SelectedArrageMethod
        {
            get => this.selectedArrageMethod;
            set
            {
                SetProperty<int>(ref this.selectedArrageMethod, value);
                eArrangeType = (ArrangeType)selectedArrageMethod;
                
                if(WirePointList != null)
                    ArrangeDetectPoint();

                CollectionChanged?.Invoke();
            }
        }

        private ObservableCollection<string> refCoordList;
        public ObservableCollection<string> RefCoordList
        {
            get => this.refCoordList;
            set
            {
                refCoordList = value;
            }
        }

        private int selectedRefCoord;
        public int SelectedRefCoord
        {
            get => this.selectedRefCoord;
            set
            {
                SetProperty<int>(ref this.selectedRefCoord, value);
            }
        }

        private int refCoordNum = 0;
        public int RefCoordNum
        {
            get => this.refCoordNum;
            set
            {
                if(this.refCoordNum < value)
                {
                    int iter = value - refCoordNum;

                    for (int i = this.refCoordNum + 1; i <= this.refCoordNum + iter; i++)
                        this.RefCoordList.Add("Ref # " + i.ToString());
                }
                else
                {
                    int iter = refCoordNum - value;

                    for(int i = 0; i < iter; i++)
                        this.RefCoordList.RemoveAt(this.RefCoordList.Count() - 1);
                }
                SetProperty<int>(ref this.refCoordNum, value);
            }
        }

        private int selectedChannel = 0;
        public int SelectedChannel
        {
            get => this.selectedChannel;
            set
            {
                SetProperty<int>(ref this.selectedChannel, value);
            }
        }

        private ObservableCollection<string> channelList;
        public ObservableCollection<string> ChannelList
        {
            get => this.channelList;
            set
            {
                channelList = value;
            }
        }
        private int detectNum = 0;
        public int DetectNum
        {
            get => this.detectNum;
            set
            {
                SetProperty<int>(ref this.detectNum, value);
            }
        }
        
        #endregion

        public VerticalWire_ROIItem Main;
        public VerticalWire_ROIItem_ViewModel(int ROIIdx, int refNum)
        {
            this.Main = new VerticalWire_ROIItem();
            this.ItemIndex = ROIIdx;
            this.ROIHeader = "ROI # " + this.ItemIndex.ToString();
            this.Main.DataContext = this;

            this.ArrangeMethod = new ObservableCollection<string>();
            this.ArrangeMethod.Add("Top -> Bottom");
            this.ArrangeMethod.Add("Bottom -> Top");
            this.ArrangeMethod.Add("Left -> Right");
            this.ArrangeMethod.Add("Right -> Left");

            this.ChannelList = new ObservableCollection<string>();
            this.ChannelList.Add("Red");
            this.ChannelList.Add("Green");
            this.ChannelList.Add("Blue");

            this.RefCoordList = new ObservableCollection<string>();
            this.RefCoordNum = refNum;
            this.SelectedRefCoord = 0;
        }

        public void ArrangeDetectPoint()
        {
            switch(eArrangeType)
            {
                case ArrangeType.Top2Bottom:
                    WirePointList.Sort(delegate (Point x, Point y)
                    {
                        return x.Y.CompareTo(y.Y);
                    });
                    break;
                case ArrangeType.Bottom2Top:
                    WirePointList.Sort(delegate (Point x, Point y)
                    {
                        return y.Y.CompareTo(x.Y);
                    });
                    break;
                case ArrangeType.Left2Right:
                    WirePointList.Sort(delegate (Point x, Point y)
                    {
                        return x.X.CompareTo(y.X);
                    });
                    break;
                case ArrangeType.Right2Left:
                    WirePointList.Sort(delegate (Point x, Point y)
                    {
                        return y.X.CompareTo(x.X);
                    });
                    break;
            }
            DetectNum = WirePointList.Count;
        }

        #region [Enum]
        public enum ArrangeType
        {
            Top2Bottom,
            Bottom2Top,
            Left2Right,
            Right2Left,
        }

        #endregion
    }
}
