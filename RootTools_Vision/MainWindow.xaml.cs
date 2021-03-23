using RootTools;
using RootTools.Memory;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using RootTools.Database;

namespace RootTools_Vision  
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {


        //ImageData imageData;
        //InspectionManager inspectionManager;
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainWindow_ViewModel();
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
