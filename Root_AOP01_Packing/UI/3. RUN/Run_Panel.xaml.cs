using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Root_AOP01_Packing
{
    /// <summary>
    /// Run_Panel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Run_Panel : UserControl
    {
        public Run_Panel()
        {
            InitializeComponent();
            DispatcherTimer m_timerALID = new DispatcherTimer();
            m_timerALID.Interval = TimeSpan.FromMilliseconds(1000);
            m_timerALID.Tick += M_timerALID_Tick;
            m_timerALID.Start();
        }
        private void M_timerALID_Tick(object sender, EventArgs e)
        {
            Run_ViewModel vm = this.DataContext as Run_ViewModel;
            #region AlarmBtn_Blink_Option
            if (vm.m_Engineer.ClassGAF().m_listALID.p_alarmListBlink)
                btnAlarm.Tag = true;
            else
                btnAlarm.Tag = false;
            #endregion
        }
        private void DoorButton_Click(object sender, RoutedEventArgs e)
        {
            //if ((sender as ToggleButton).IsChecked == true)
            //    (sender as ToggleButton).Content = "Door Lock";
            //else
            //    (sender as ToggleButton).Content = "Door Unlock";
        }
        private void OnlineButton_Click(object sender, RoutedEventArgs e)
        {
            //if ((sender as ToggleButton).IsChecked == true)
            //    (sender as ToggleButton).Content = "ONLINE";
            //else
            //    (sender as ToggleButton).Content = "OFFLINE";
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag.ToString() == "PAUSE")
            {
                (sender as Button).Content = "RESUME";
                (sender as Button).Tag = "RESUME";
            }
            else
            {
                (sender as Button).Content = "PAUSE";
                (sender as Button).Tag = "PAUSE";
            }
        }

        private void PodButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag.ToString() == "START")
            {
                (sender as Button).Content = "STOP";
                (sender as Button).Tag = "STOP";
            }
            else
            {
                (sender as Button).Content = "POD START";
                (sender as Button).Tag = "START";
            }
        }
        private void CaseButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag.ToString() == "START")
            {
                (sender as Button).Content = "STOP";
                (sender as Button).Tag = "STOP";
            }
            else
            {
                (sender as Button).Content = "CASE START";
                (sender as Button).Tag = "START";
            }
        }
    }
}
