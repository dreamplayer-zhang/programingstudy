using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_VEGA_P_Vision
{
    /// <summary>
    /// ROIListItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ROIListItem : UserControl,ICloneable
    {
        public ROIListItem()
        {
            InitializeComponent();
        }
        public ROIListItem(ROIListItem roiListItem)
        {
           item1 = roiListItem.item1;
           item2 = roiListItem.item2;
           item3 = roiListItem.item3;

           InitializeComponent();

        }
        public ROIListItem(string item1,string item2, string item3)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;

            InitializeComponent();
        }
        public void SetData(string item1,string item2,string item3)
        {
            this.item1 = item1;
            this.item2 = item2;
            this.item3 = item3;

        }
        string item1;
        public string Item1
        {
            get => item1;
            set => item1 = value;
        }
        string item2;
        public string Item2
        {
            get => item2;
            set => item2 = value;
        }
        string item3;
        public string Item3
        {
            get => item3;
            set => item3 = value;
        }
        public object Clone()
        {
            return new ROIListItem(Item1, Item2, item3);
        }
    }
}
