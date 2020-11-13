using System.Windows.Controls;

namespace Root_AxisMapping.MainUI
{
    /// <summary>
    /// Result_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Result_UI : UserControl
    {
        public Result_UI()
        {
            InitializeComponent();
        }

        Result m_result; 
        public void Init(Result result)
        {
            m_result = result;
            DataContext = result; 
        }
    }
}
