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

namespace RootTools
{
    /// <summary>
    /// ImageViewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        public ImageViewer()
        {
            InitializeComponent();
        }
        //private void Viewer_KeyDown(object sender, KeyEventArgs e)
        //{
        //    ImageViewer_ViewModel vm = (ImageViewer_ViewModel)this.DataContext;
        //    vm.KeyEvent = e;
        //}
    }
}
