﻿using System;
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
using RootTools_Vision;

namespace Root_WIND2
{
    /// <summary>
    /// InspTestPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InspTestPage : UserControl
    {
        public InspTestPage()
        {
            InitializeComponent();
        }


        public void SetUIContext()
        {
            //this.mapViewer.DataContext = vmMapView;
        }

        private void RootViewer_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}