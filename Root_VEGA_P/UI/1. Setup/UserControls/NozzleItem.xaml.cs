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

namespace Root_VEGA_P
{
    /// <summary>
    /// NozzleItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NozzleItem : UserControl,ICloneable
    {
        bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set 
            {
                isEnabled = value;
            }
        }
        int num;
        public string Num
        {
            get => num.ToString();
        }
        double pressure;
        public string Pressure
        {
            get => pressure.ToString();
        }
        double sec;
        public string Sec
        {
            get => sec.ToString();
        }

        public NozzleItem(bool IsChecked,int num,double Pressure,double Sec)
        {
            isEnabled = IsChecked;
            this.num = num;
            pressure = Pressure;
            sec = Sec;
            InitializeComponent();
        }
        public object Clone()
        {
            return new NozzleItem(isEnabled,num,pressure,sec);
        }
    }
}
