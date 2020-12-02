using RootTools.Trees;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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
            treeRootUI.Init(modbus.m_treeRoot);
            modbus.RunTree(Tree.eMode.Init); 
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
	}
}
