using RootTools.Trees;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RootTools.Comm
{
    /// <summary>
    /// Modbus_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Modbus_UI : UserControl
    {
        public Modbus_UI()
        {
            InitializeComponent();
        }

        Modbus m_modbus; 
        public void Init(Modbus modbus)
        {
            m_modbus = modbus;
            DataContext = modbus;
            commLogUI.Init(modbus.m_commLog);
            dataGridData.ItemsSource = modbus.m_aViewData;
            comboType.ItemsSource = Enum.GetValues(typeof(Modbus.eType));
            comboType.SelectedIndex = 0;
            treeRootUI.Init(modbus.m_treeRoot);
            modbus.RunTree(Tree.eMode.Init);
            InitTimer(); 
        }

		private void checkBoxConnect_Checked(object sender, RoutedEventArgs e)
		{
            m_modbus.p_sInfo = m_modbus.Connect();
            Thread.Sleep(50);
            if (m_modbus.p_bConnect == false) checkBoxConnect.IsChecked = false;
        }

        private void checkBoxConnect_Unchecked(object sender, RoutedEventArgs e)
		{
            m_modbus.m_client.Disconnect();
        }

        private void checkBoxRead_Click(object sender, RoutedEventArgs e)
        {
            m_swTimer.Start(); 
        }

        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(1);
            m_timer.Tick += M_timer_Tick;
            m_timer.Start(); 
        }

        StopWatch m_swTimer = new StopWatch(); 
        private void M_timer_Tick(object sender, EventArgs e)
        {
            if (checkBoxRead.IsChecked == false) return;
            m_modbus.ReadViewData(m_modbus.p_nViewUnit); 
            if (m_swTimer.ElapsedMilliseconds > 300000) checkBoxRead.IsChecked = false;
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            Modbus.eType eType = (Modbus.eType)comboType.SelectedItem;
            m_modbus.AddViewData(eType); 
        }
    }
}
