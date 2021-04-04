using System.Windows;
using System.Windows.Controls;

namespace RootTools.RFIDs
{
    /// <summary>
    /// RFID_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RFID_UI : UserControl
    {
        public RFID_UI()
        {
            InitializeComponent();
        }

        RFID m_RFID; 
        public void Init(RFID RFID)
        {
            m_RFID = RFID;
            DataContext = RFID;
            treeRootUI.Init(RFID.m_treeRoot);
            tabItemComm.Content = RFID.m_RFID.p_ui;
            RFID.RunTree(Trees.Tree.eMode.Init); 
        }

        private void buttonRead_Click(object sender, RoutedEventArgs e)
        {
            string sRFID = ""; 
            m_RFID.Read(out sRFID); 
        }
    }
}
