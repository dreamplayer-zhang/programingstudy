using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RootTools_Vision
{
    public class VerticalWire_RefCoordItem_ViewModel : ObservableObject
    {
        public ArrangeType eArrangeType;

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

        private int refX = 0;
        public int RefX
        {
            get => this.refX;
            set
            {
                SetProperty<int>(ref this.refX, value);
            }
        }

        private int refY = 0;
        public int RefY
        {
            get => this.refY;
            set
            {
                SetProperty<int>(ref this.refY, value);
            }
        }

        private int refW = 0;
        public int RefW
        {
            get => this.refW;
            set
            {
                SetProperty<int>(ref this.refW, value);
            }
        }

        private int refH = 0;
        public int RefH
        {
            get => this.refH;
            set
            {
                SetProperty<int>(ref this.refH, value);
            }
        }

        private string refHeader;
        public string RefHeader
        {
            get => this.refHeader;
            set
            {
                SetProperty<string>(ref this.refHeader, value);
            }
        }

        private int selectedArrageMethod;
        public int SelectedArrageMethod
        {
            get => this.selectedArrageMethod;
            set
            {
                SetProperty<int>(ref this.selectedArrageMethod, value);
                eArrangeType = (ArrangeType)selectedArrageMethod;

                switch(eArrangeType)
                {
                    case ArrangeType.LT:
                        RefCoord = new CPoint(RefX, RefY);
                        break;
                    case ArrangeType.LB:
                        RefCoord = new CPoint(RefX, RefY + RefH);
                        break;
                    case ArrangeType.RT:
                        RefCoord = new CPoint(RefX + RefW, RefY);
                        break;
                    case ArrangeType.RB:
                        RefCoord = new CPoint(RefX + RefW, RefY + RefH);
                        break;
                    case ArrangeType.Center:
                        RefCoord = new CPoint(RefX + RefW / 2, RefY + RefH / 2);
                        break;
                }
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

        private CPoint refCoord;
        public CPoint RefCoord
        {
            get => this.refCoord;
            set
            {
                SetProperty<CPoint>(ref this.refCoord, value);
            }
        }
        #endregion

        public VerticalWire_RefCoordItem Main;
        public VerticalWire_RefCoordItem_ViewModel(int RefIdx)
        {
            Main = new VerticalWire_RefCoordItem();
            this.ItemIndex = RefIdx;
            this.RefHeader = "Ref Coord # " + this.ItemIndex.ToString();
            Main.DataContext = this;

            this.SelectedArrageMethod = 0;

            this.ArrangeMethod = new ObservableCollection<string>();
            this.ArrangeMethod.Add("Left Top");
            this.ArrangeMethod.Add("Left Bottom");
            this.ArrangeMethod.Add("Right Top");
            this.ArrangeMethod.Add("Right Bottom");
            this.ArrangeMethod.Add("Center");
        }

        #region [Enum]
        public enum ArrangeType
        {
            LT,
            LB,
            RT,
            RB,
            Center,
        }     
        #endregion
    }
}
