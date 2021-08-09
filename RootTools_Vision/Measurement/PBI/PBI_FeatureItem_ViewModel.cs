using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class PBI_FeatureItem_ViewModel : ObservableObject
    {
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

        #endregion

        public PBI_FeatureItem Main;
        public PBI_FeatureItem_ViewModel(int RefIdx)
        {
            Main = new PBI_FeatureItem();
            this.ItemIndex = RefIdx;
            this.RefHeader = "Feature # " + this.ItemIndex.ToString();
            Main.DataContext = this;
        }
    }
}
