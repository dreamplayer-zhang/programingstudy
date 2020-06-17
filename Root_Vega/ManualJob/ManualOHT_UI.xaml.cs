using System.Windows;

namespace Root_Vega.ManualJob
{
    /// <summary>
    /// ManualOHT_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ManualOHT_UI : Window
    {
        public ManualOHT_UI()
        {
            InitializeComponent();
        }

        public void Init(Vega_Handler handler)
        {
            manualOHTA.Init(handler.m_aLoadport[0]);
            manualOHTB.Init(handler.m_aLoadport[1]);
        }
    }
}
