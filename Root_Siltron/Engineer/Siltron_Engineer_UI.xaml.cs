using RootTools;
using System.Windows.Controls;

namespace Root_Siltron
{
    /// <summary>
    /// Siltron_Engineer_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Siltron_Engineer_UI : UserControl
    {
        public Siltron_Engineer_UI()
        {
            InitializeComponent();
        }

        Siltron_Engineer m_engineer = null;
        public void Init(Siltron_Engineer engineer)
        {
            m_engineer = engineer;
            logViewUI.Init(LogView._logView);
            toolBoxUI.Init(engineer.ClassToolBox());
            handlerUI.Init(engineer.m_handler);
        }
    }
}
