﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System;
using RootTools;
using Root_CAMELLIA.Data;

namespace Root_CAMELLIA
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                NormalizeButton.Visibility = Visibility.Collapsed;
            }

            InitTimer();

            Init();
        }

        private void Init()
        {
            recipe = new Dlg_RecipeManger();
            DataManager = new DataManager(this);
            m_RecipeManagerViewModel = new Dlg_RecipeManager_ViewModel(this, recipe);
            recipe.DataContext = m_RecipeManagerViewModel;
        }

        #region Window
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
        }
        #endregion


        #region ViewModel
        public Dlg_RecipeManager_ViewModel m_RecipeManagerViewModel;
        #endregion

        #region Dlg
        private Dlg_RecipeManger recipe;
        #endregion

        #region Getter Setter
        public DataManager DataManager { get; set; }
        #endregion

        #region Title Bar
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
            recipe.Close();
            this.Close();
        }
        #endregion

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromMilliseconds(100);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start();
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            tbTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            recipe.Close();
            this.Close();
        }


        bool test = false;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //recipe = new Dlg_RecipeManger();
            //m_RecipeManagerViewModel = new Dlg_RecipeManager_ViewModel(recipe);
            //recipe.DataContext = m_RecipeManagerViewModel;

           

            if (!test)
            {

                test = true;
                recipe.Visibility = Visibility.Visible;
                
            }
            else if (test)
            {
                recipe.Visibility = Visibility.Hidden;
                test = false;
            }
            //recipe.Visibility = Visibility.Hidden;
            //recipe.Close();



        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Dlg_Engineer engineer = new Dlg_Engineer();
            engineer.ShowDialog();
        }
    }
}
