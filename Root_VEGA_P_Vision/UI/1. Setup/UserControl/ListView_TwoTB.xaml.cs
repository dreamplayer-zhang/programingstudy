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
    /// ListView_TwoTB.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ListView_TwoTB : UserControl
    {
        public ListView_TwoTB()
        {
            InitializeComponent();
        }
        public ListView_TwoTB(string item1,string item2)
        {
            InitializeComponent();

            sItem1 = item1;
            sItem2 = item2;            
        }
        string item1, item2;
        public string sItem1
        {
            get => item1;
            set
            {
                item1 = value;
                Item1.Text = value;
            }
        }
        public string sItem2
        {
            get => item2;
            set
            {
                item2 = value;
                Item2.Text = value;
            }
        }
    }
}
