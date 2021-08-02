using RootTools.Trees;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace RootTools.Control.Ajin
{
    /// <summary>
    /// UserControl1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AjinAxis_UI : UserControl
    {
        public AjinAxis_UI()
        {
            InitializeComponent();
        }

        AjinAxis m_axis;
        public void Init(AjinAxis axis)
        {
            this.DataContext = axis;
            m_axis = axis;
            comboSpeedJog.SelectedIndex = (int)Axis.eSpeed.Jog; 
            comboSpeedShift.SelectedIndex = (int)Axis.eSpeed.Move;
            comboSpeedMove.SelectedIndex = (int)Axis.eSpeed.Move;
            comboSpeedRepeat.SelectedIndex = (int)Axis.eSpeed.Move;
            comboMovePos.SelectedIndex = (int)Axis.ePosition.Position_0;
            comboRepeatPos0.SelectedIndex = (int)Axis.ePosition.Position_0;
            comboRepeatPos1.SelectedIndex = (int)Axis.ePosition.Position_1;
            textBoxShift.Text = "0"; 
            treeRootUI.Init(axis.m_treeRoot);
            treeRootSettingUI.Init(axis.m_treeRootSetting);
            treeRootInterlockUI.Init(axis.m_treeRootInterlock);
            axis.RunTree(Tree.eMode.Init);
            axis.RunTreeSetting(Tree.eMode.Init);
            axis.RunTreeInterlock(Tree.eMode.Init);
            StartTimer(); 
        }

        #region Timer
        DispatcherTimer m_timer = new DispatcherTimer(); 
        void StartTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        private void M_timer_Tick(object sender, EventArgs e)
        {
            Timer_Status();
            Timer_Repeat();
        }
        #endregion

        #region Status
        void Timer_Status()
        {
            Timer_Status(buttonServoOn, m_axis.p_bServoOn);
            Timer_Status(buttonAlarm, m_axis.p_sensorAlarm);
            Timer_Status(buttonInPos, m_axis.p_sensorInPos);
            Timer_Status(buttonLimitM, m_axis.p_sensorMinusLimit);
            Timer_Status(buttonHome, m_axis.p_sensorHome);
            Timer_Status(buttonLimitP, m_axis.p_sensorPlusLimit);
        }

        void Timer_Status(Button button, bool bOn)
        {
            button.Background = bOn ? Brushes.Yellow : Brushes.LightGray;
            button.Foreground = bOn ? Brushes.Black : Brushes.DarkGray; 
        }

        private void buttonServoOn_Click(object sender, RoutedEventArgs e)
        {
            m_axis.ServoOn(!m_axis.p_bServoOn);
        }

        private void buttonAlarm_Click(object sender, RoutedEventArgs e)
        {
            m_axis.ResetAlarm(); 
        }
        #endregion

        #region Home
        private void buttonRunHome_Click(object sender, RoutedEventArgs e)
        {
            m_axis.StartHome(); 
        }
        #endregion

        #region Jog
        private void buttonJogM3_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { EQ.p_bStop = false; Jog(-1); }
        private void buttonJogM2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { EQ.p_bStop = false; Jog(-0.31); }
        private void buttonJogM1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { EQ.p_bStop = false; Jog(-0.1); }
        private void buttonJogP1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { EQ.p_bStop = false; Jog(0.1); }
        private void buttonJogP2_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { EQ.p_bStop = false; Jog(0.31); }
        private void buttonJogP3_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { EQ.p_bStop = false; Jog(1); }

        private void buttonJog_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_axis.p_eState != Axis.eState.Jog) return;
            m_axis.StopAxis(true);
            m_bRepeat = false;
        }

        void Jog(double fScale)
        {
            if (m_bRepeat) return;
            //if (m_axis.p_eState != Axis.eState.Ready) return;
            m_axis.Jog(fScale, comboSpeedJog.Text); 
        }
        #endregion

        #region Shift
        private void buttonShiftM_Click(object sender, RoutedEventArgs e) { Shift(-1); }
        private void buttonShiftP_Click(object sender, RoutedEventArgs e) { Shift(1); }

        void Shift(int nDir)
        {
            if (m_bRepeat) return;
            if (m_axis.p_eState != Axis.eState.Ready) return;
            try
            {
                double dPos = nDir * Convert.ToInt32(textBoxShift.Text);
                m_axis.StartShift(dPos, comboSpeedShift.Text); 
            }
            catch (Exception) { }
        }
        #endregion

        #region Move
        private void buttonMove_Click(object sender, RoutedEventArgs e)
        {
            if (m_bRepeat) return;
            if (m_axis.p_eState != Axis.eState.Ready) return;
            m_axis.StartMove(comboMovePos.Text, 0, comboSpeedMove.Text); 
        }

        private void buttonMoveStop_Click(object sender, RoutedEventArgs e)
        {
            if (m_axis.p_eState != Axis.eState.Move) return;
            m_axis.StopAxis(true);
            m_bRepeat = false; 
        }

        private void buttonSetPos_Click(object sender, RoutedEventArgs e)
        {
            m_axis.SetPositionValue(comboMovePos.Text);
            m_axis.RunTree(Tree.eMode.Init);
        }
        #endregion

        #region Repeat
        private void buttonRepeat_Click(object sender, RoutedEventArgs e)
        {
            m_bRepeat = true; 
        }

        private void buttonRepeatStop_Click(object sender, RoutedEventArgs e)
        {
            m_bRepeat = false; 
        }

        bool m_bRepeat = false;
        bool m_bDstRepeat = false; 
        void Timer_Repeat()
        {
            if (m_bRepeat == false) return;
            if (m_axis.p_eState != Axis.eState.Ready) return;
            string sDst = m_bDstRepeat ? comboRepeatPos1.Text : comboRepeatPos0.Text;
            m_axis.StartMove(sDst, 0, comboSpeedRepeat.Text);
            m_bDstRepeat = !m_bDstRepeat; 
        }
        #endregion

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            m_bRepeat = false;
            m_axis.StopAxis(); 
        }

        private void checkBoxTrigger_Checked(object sender, RoutedEventArgs e)
        {
            m_axis.SetTrigger(Convert.ToInt32(m_axis.GetPosValue(comboRepeatPos0.Text)), Convert.ToInt32(m_axis.GetPosValue(comboRepeatPos1.Text)), Convert.ToDouble(tbInterval.Text), Convert.ToDouble(tbUpTime.Text), true);
            //m_axis.RunTrigger(true);
        }

        private void checkBoxTrigger_Unchecked(object sender, RoutedEventArgs e)
        {
            m_axis.RunTrigger(false);
        }
    }
}
