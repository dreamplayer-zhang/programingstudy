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

        public Modbus.DataGroup m_dataGroup; 
        public void Init(Modbus.DataGroup dataGroup)
        {
            m_dataGroup = dataGroup;
            DataContext = dataGroup;
            dataGridData.ItemsSource = dataGroup.m_aData; 
        }
    }
}
