﻿using RootTools;
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

namespace Root_CAMELLIA
{
    /// <summary>
    /// Dlg_Engineer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Dlg_Engineer : Window, IDialog
    {
        public Dlg_Engineer()
        {
            InitializeComponent();
            //CAMELLIA_Engineer engineer = new CAMELLIA_Engineer();
            //Init(App.m_engineer.m_handler);
            Init(App.m_engineer);
        }

        public void Init(CAMELLIA_Engineer engineer)
        {
            EngineerUI.Init(engineer);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void PMButton_Click(object sender, RoutedEventArgs e)
        {
            Dlg_PM dlg_PM = new Dlg_PM();
            dlg_PM.ShowDialog();
        }
    }
}
