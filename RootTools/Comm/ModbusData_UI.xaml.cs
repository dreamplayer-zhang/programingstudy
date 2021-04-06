using System.Windows.Controls;

namespace RootTools.Comm
{
    /// <summary>
    /// ModbusData_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModbusData_UI : UserControl
    {
        public ModbusData_UI()
        {
            InitializeComponent();
        }

        public Modbus m_modbus; 
        public void Init(Modbus modbus)
        {
            m_modbus = modbus;
            DataContext = modbus;
            dataGridData.ItemsSource = modbus.m_aViewData; 
        }
    }
}
