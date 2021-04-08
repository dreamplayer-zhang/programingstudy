using System;
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
            listViewLog.LayoutUpdated += listViewLog_LayoutUpdated;
        }

        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            if (m_commLog != null) m_commLog.Send(textBox.Text); 
        }

        private void listViewLog_LayoutUpdated(object sender, EventArgs e)
        {
            if (EQ.m_bRun) p_nCount = listViewLog.Items.Count;
        }

        int _nCount = 0;
        int p_nCount
        {
            set
            {
                if (_nCount == value) return;
                _nCount = value;
                if (value > 0) listViewLog.ScrollIntoView(listViewLog.Items[value - 1]);
                //listViewLog.Focus();
            }
        }
    }
}
