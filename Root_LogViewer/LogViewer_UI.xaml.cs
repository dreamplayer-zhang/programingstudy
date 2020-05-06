using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;

namespace Root_LogViewer
{
    /// <summary>
    /// LogViewer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogViewer_UI : UserControl
    {
        public LogViewer_UI()
        {
            InitializeComponent();
        }

        LogViewer m_logViewer; 
        public void Init(LogViewer logViewer)
        {
            m_logViewer = logViewer;
            DataContext = logViewer; 
        }

        private void buttonOpen_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Log Files (*.csv)|*.csv";
            dlg.InitialDirectory = "c:\\Log"; 
            if (dlg.ShowDialog() == false)
            {
                m_logViewer.p_sInfo = "Log File Open Cancel !!"; 
                return;
            }
            string sTitle = dlg.SafeFileName;
            string sFile = dlg.FileName;
            string sPath = sFile.Substring(0, sFile.Length - sTitle.Length - 1);
            DirectoryInfo dir = new DirectoryInfo(sPath);
            if (dir.Exists == false)
            {
                m_logViewer.p_sInfo = "Can't Find Log Folder : " + sPath;
                return;
            }
            tabLogGroup.Items.Clear(); 
            //m_logViewer.OpenLogFiles(sPath);
            if (m_logViewer.p_sInfo != "OK") return; 

        }
    }
}
