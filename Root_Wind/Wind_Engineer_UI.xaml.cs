using RootTools;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_Wind
{
    /// <summary>
    /// Wind_Engineer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Wind_Engineer_UI : UserControl
    {
        public Wind_Engineer_UI()
        {
            InitializeComponent();
        }

        Wind_Engineer m_engineer = null;
        public void Init(Wind_Engineer engineer)
        {
            m_engineer = engineer;
            logViewUI.Init(LogView.m_logView);
            loginUI.Init(engineer.m_login);
            toolBoxUI.Init(engineer.ClassToolBox());
            handlerUI.Init(engineer.m_handler);
        }
    }
}
