using System.Windows.Controls;

namespace RootTools.Logs
{
    /// <summary>
    /// LogSet_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Log_Set_UI : UserControl
    {
        public Log_Set_UI()
        {
            InitializeComponent();
        }

        Log_Set m_logSet; 
        public void Init(Log_Set logSet)
        {
            m_logSet = logSet;
            DataContext = logSet; 
        }
    }
}
