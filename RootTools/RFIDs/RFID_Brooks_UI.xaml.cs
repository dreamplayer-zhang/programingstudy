using System.Windows.Controls;

namespace RootTools.RFIDs
{
    /// <summary>
    /// RFID_Brooks_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RFID_Brooks_UI : UserControl
    {
        public RFID_Brooks_UI()
        {
            InitializeComponent();
        }

        RFID_Brooks m_RFID; 
        public void Init(RFID_Brooks RFID)
        {
            m_RFID = RFID;
            DataContext = RFID;
            rs232UI.Init(RFID.m_rs232); 
        }
    }
}
