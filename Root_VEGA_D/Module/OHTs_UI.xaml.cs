using Root_EFEM.Module;
using Root_VEGA_D.Engineer;
using System.Windows;

namespace Root_VEGA_D.Module
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

        public void Init(VEGA_D_Handler handler)
        {
            manualOHTA.Init((Loadport_Cymechs)handler.m_aLoadport[0]);
            manualOHTB.Init((Loadport_Cymechs)handler.m_aLoadport[1]);
        }

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
            e.Cancel = true;
            this.Hide();
		}
	}
}
