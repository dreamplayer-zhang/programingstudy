using Root_EFEM.Module;
using System.Windows;

namespace Root_AOP01_Inspection.Module
{
    /// <summary>
    /// OHTs_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class OHTs_UI : Window
    {
        public OHTs_UI()
        {
            InitializeComponent();
        }

        public void Init(AOP01_Handler handler)
        {
            manualOHTA.Init((Loadport_Cymechs)handler.m_aLoadport[0]);
            manualOHTB.Init((Loadport_Cymechs)handler.m_aLoadport[1]);
        }

    }
}
