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
using System.Windows.Shapes;

namespace RootTools.Camera.BaslerPylon
{
    /// <summary>
    /// GraphWindowTest.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GraphWindowTest : Window
    {
        private bool[] visibleRGB = new bool[3] { true, true, true };

        public bool visibleRed
        {
            get { return visibleRGB[0]; }
            set
            {
                if (value == true) chart.ShowRGB(0);
                else chart.HideRGB(0);

                visibleRGB[0] = value;
            }
        }

        public bool visibleGreen
        {
            get { return visibleRGB[1]; }
            set
            {
                if (value == true) chart.ShowRGB(1);
                else chart.HideRGB(1);

                visibleRGB[1] = value;
            }
        }
        public bool visibleBlue
        {
            get { return visibleRGB[2]; }
            set
            {
                if (value == true) chart.ShowRGB(2);
                else chart.HideRGB(2);

                visibleRGB[2] = value;
            }
        }

        public GraphWindowTest()
        {
            InitializeComponent();

            DataContext = this;
        }
        public void Init()
        {
            chart.StartUpdateGraph();
        }

        public void SetImage(ImageData image)
        {
            chart.SetImage(image);
        }
    }
}
