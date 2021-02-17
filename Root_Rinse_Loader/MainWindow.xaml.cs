﻿using Root_Rinse_Loader.Engineer;
using Root_Rinse_Loader.Module;
using RootTools;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Root_Rinse_Loader
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitTimer();
        }

        #region Loaded
        RinseL_Engineer m_engineer = new RinseL_Engineer();
        RinseL_Handler m_handler; 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\Rinse_Loader")) Directory.CreateDirectory(@"C:\Recipe\Rinse_Loader");
            m_engineer.Init("Rinse_Loader");
            engineerUI.Init(m_engineer);
            m_handler = (RinseL_Handler)m_engineer.ClassHandler();
            Init(); 
        }

        void Init()
        {
            textBlockState.DataContext = EQ.m_EQ;
            textBolckUnloadState.DataContext = m_handler.m_rinse;
            textBlockRinseState.DataContext = m_handler.m_rinse;
            buttonMode.DataContext = m_handler.m_rinse;
            textBoxWidth.DataContext = m_handler.m_rinse;
            magazineUI.Init(m_handler.m_rinse, m_handler.m_storage);
            stackUI.Init(m_handler.m_storage.m_stack, m_handler.m_loader);
            tabControlStorage.SelectedIndex = (int)m_handler.m_rinse.p_eMode;
            progressUI.Init(m_handler.m_rinse); 
            textBoxRotateSpeed.DataContext = m_handler.m_rinse;
        }
        #endregion

        #region Closing
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_engineer.ThreadStop();
        }
        #endregion

        #region TitleBar
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            NormalizeButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Collapsed;
        }
        private void NormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            MaximizeButton.Visibility = Visibility.Visible;
            NormalizeButton.Visibility = Visibility.Collapsed;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    MaximizeButton.Visibility = Visibility.Visible;
                    NormalizeButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    NormalizeButton.Visibility = Visibility.Visible;
                    MaximizeButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.DragMove();
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(10);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            RinseL rinse = m_handler.m_rinse;
            bool bBlink = rinse.m_bBlink; 

            buttonHome.IsEnabled = EQ.p_eState != EQ.eState.Run;
            buttonStart.IsEnabled = EQ.p_eState == EQ.eState.Ready;
            buttonPause.IsEnabled = EQ.p_eState == EQ.eState.Run;
            buttonReset.IsEnabled = (EQ.p_eState == EQ.eState.Error) || (EQ.p_eState == EQ.eState.Ready);
            buttonPickerSet.IsEnabled = EQ.p_eState == EQ.eState.Ready;

            bool bRun = bBlink && (EQ.p_eState == EQ.eState.Run); 
            buttonStart.Foreground = (bRun && EQ.p_bPickerSet == false) ? Brushes.Red : Brushes.Black;
            buttonPickerSet.Foreground = (bRun && EQ.p_bPickerSet) ? Brushes.Red : Brushes.Black;

            borderState.Background = (EQ.p_eState == EQ.eState.Ready || EQ.p_eState == EQ.eState.Run) ? Brushes.SeaGreen : Brushes.Gold;
            borderUnloadState.Background = (rinse.p_eStateUnloader == EQ.eState.Ready || rinse.p_eStateUnloader == EQ.eState.Run) ? Brushes.SeaGreen : Brushes.Gold;

            gridRed.Background = (bBlink && (EQ.p_eState == EQ.eState.Error)) ? Brushes.Crimson : Brushes.DarkRed;
            gridYellow.Background = (bBlink && (EQ.p_eState == EQ.eState.Run)) ? Brushes.Gold : Brushes.YellowGreen;
            gridGreen.Background = (bBlink && (EQ.p_eState == EQ.eState.Ready)) ? Brushes.SeaGreen : Brushes.DarkGreen;

            textBolckUnloadState.Foreground = rinse.m_tcpip.p_bConnect ? Brushes.Black : Brushes.Gray; 
        }
        #endregion

        #region Control Function
        private void buttonMode_Click(object sender, RoutedEventArgs e)
        {
            m_handler.m_rinse.p_eMode = (RinseL.eRunMode)(1 - (int)m_handler.m_rinse.p_eMode);
            tabControlStorage.SelectedIndex = (int)m_handler.m_rinse.p_eMode; 
        }

        private void buttonHome_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_bStop = false; 
            EQ.p_eState = EQ.eState.Home;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_eState = EQ.eState.Run;
        }

        private void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            EQ.p_eState = EQ.eState.Ready;
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            m_handler.m_rinse.RunBuzzerOff(); 
            EQ.p_eState = EQ.eState.Ready;
        }

        private void buttonPickerSet_Click(object sender, RoutedEventArgs e)
        {
            m_handler.StartPickerSet();
        }

        private void textBoxWidth_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression((TextBox)sender, property);
            if (binding != null) binding.UpdateSource();
        }

        private void textBoxRotateSpeed_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            DependencyProperty property = TextBox.TextProperty;
            BindingExpression binding = BindingOperations.GetBindingExpression((TextBox)sender, property);
            if (binding != null) binding.UpdateSource();
        }
        #endregion
    }
}
