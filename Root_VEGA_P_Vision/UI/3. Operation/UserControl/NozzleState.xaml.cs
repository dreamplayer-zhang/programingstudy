using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// NozzleState.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NozzleState : UserControl
    {
        public string ImgSrc { get; set; }
        public ObservableCollection<string> Nozzles { get; set; }
        public string NozzleNum { get; set; }
        public NozzleState()
        {
            InitializeComponent();
        }
        public NozzleState(string ImgSrc, int startNum,int endNum)
        {
            this.ImgSrc = ImgSrc;
            Nozzles = new ObservableCollection<string>();
            for(int i=startNum;i<=endNum;i++)
            {
                NozzleNum = "Nozzle " + i;
                Nozzles.Add(NozzleNum);
            }

            InitializeComponent();
        }
    }
}