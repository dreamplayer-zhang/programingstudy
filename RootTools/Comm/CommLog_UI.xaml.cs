using System.Windows.Controls;
using System.Windows.Input;

namespace RootTools.Comm
{
    /// <summary>
    /// IComm_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CommLog_UI : UserControl
    {
        public CommLog_UI()
        {
            InitializeComponent();
        }

        public CommLog m_commLog; 
        public void Init(CommLog commLog)
        {
            m_commLog = commLog;
            this.DataContext = commLog;
            listViewLog.ItemsSource = commLog.m_aLog; 
        }

        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (m_commLog != null) m_commLog.Send(textBox.Text); 
        }
    }
}
