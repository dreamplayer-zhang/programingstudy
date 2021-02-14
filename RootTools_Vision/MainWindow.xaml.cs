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

namespace RootTools_Vision
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        //InspectionManager inspectionManager;
        public MainWindow()
        {
            InitializeComponent();

        }

        bool isServer = false;

        private ClonableWorkFactory client = new ClonableWorkFactory();
        private ServerWorkFactory server = new ServerWorkFactory();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.isServer)
                server.RemoteStart();
            else
                client.RemoteStart();
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (this.isServer)
                server.WriteTest();
            else
                client.WriteTest();
        }

        private void Button_Server(object sender, RoutedEventArgs e)
        {
            this.isServer = true;
        }

        private void Button_Client(object sender, RoutedEventArgs e)
        {
            this.isServer = false;
        }
    }
}
