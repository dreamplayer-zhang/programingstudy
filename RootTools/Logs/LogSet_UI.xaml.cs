using System.Windows.Controls;

namespace RootTools.Logs
{
    /// <summary>
    /// LogSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LogSet_UI : UserControl
    {
        public LogSet_UI()
        {
            InitializeComponent();
        }

        LogSet m_logSet; 
        public void Init(LogSet logSet)
        {
            m_logSet = logSet;
            DataContext = logSet; 
        }
    }
}
