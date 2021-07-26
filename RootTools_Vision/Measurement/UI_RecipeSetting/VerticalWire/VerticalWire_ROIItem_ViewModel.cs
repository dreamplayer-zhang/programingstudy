using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class VerticalWire_ROIItem_ViewModel : ObservableObject
    {
        public ArrangeType eArrangeType;
        public List<TRect> WireRectList = new List<TRect>();

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
                
                if(WireRectList != null)
                    ArrangeDetectPoint();
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

        private int selectedRefCoord = 0;
        public int SelectedRefCoord
        {
            get => this.selectedRefCoord;
            set
            {
                SetProperty<int>(ref this.selectedRefCoord, value);
            }
        }

        private int refCoordNum = 1;
        public int RefCoordNum
        {
            get => this.refCoordNum;
            set
            {
                SetProperty<int>(ref this.refCoordNum, value);

                this.RefCoordList = new ObservableCollection<string>();
                
                for (int i = 1; i <= value; i++)
                {
                    this.RefCoordList.Add("ROI # " + i.ToString());
                }
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
        public VerticalWire_ROIItem_ViewModel(int ROIIdx)
        {
            Main = new VerticalWire_ROIItem();
            this.ItemIndex = ROIIdx;
            this.ROIHeader = "ROI # " + this.ItemIndex.ToString();
            Main.DataContext = this;

            this.ArrangeMethod = new ObservableCollection<string>();
            this.ArrangeMethod.Add("Top -> Bottom");
            this.ArrangeMethod.Add("Bottom -> Top");
            this.ArrangeMethod.Add("Left -> Right");
            this.ArrangeMethod.Add("Right -> Left");

            this.ChannelList = new ObservableCollection<string>();
            this.ChannelList.Add("Red");
            this.ChannelList.Add("Green");
            this.ChannelList.Add("Blue");
        }

        public void ArrangeDetectPoint()
        {
            switch(eArrangeType)
            {
                case ArrangeType.Top2Bottom:
                    WireRectList.Sort(delegate (TRect x, TRect y)
                    {
                        return x.MemoryRect.Top.CompareTo(y.MemoryRect.Top);
                    });
                    break;
                case ArrangeType.Bottom2Top:
                    WireRectList.Sort(delegate (TRect x, TRect y)
                    {
                        return y.MemoryRect.Top.CompareTo(x.MemoryRect.Top);
                    });
                    break;
                case ArrangeType.Left2Right:
                    WireRectList.Sort(delegate (TRect x, TRect y)
                    {
                        return x.MemoryRect.Left.CompareTo(y.MemoryRect.Left);
                    });
                    break;
                case ArrangeType.Right2Left:
                    WireRectList.Sort(delegate (TRect x, TRect y)
                    {
                        return y.MemoryRect.Left.CompareTo(x.MemoryRect.Left);
                    });
                    break;
            }
            DetectNum = WireRectList.Count;
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
