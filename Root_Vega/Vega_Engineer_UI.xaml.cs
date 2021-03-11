using RootTools;
using System.Windows.Controls;

namespace Root_Vega
{
    /// <summary>
    /// Vega_Engineer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Vega_Engineer_UI : UserControl
    {
        public Vega_Engineer_UI()
        {
            InitializeComponent();
        }

        Vega_Engineer m_engineer = null;
        public void Init(Vega_Engineer engineer)
        {
            m_engineer = engineer;
            logViewUI.Init(LogView._logView);
            loginUI.Init(engineer.m_login);
            toolBoxUI.Init(engineer.ClassToolBox());
            handlerUI.Init(engineer.m_handler);
        }
    }
}
