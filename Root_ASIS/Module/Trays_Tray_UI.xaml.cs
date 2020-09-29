using System;
using System.Windows.Controls;

namespace Root_ASIS.Module
{
    /// <summary>
    /// Trays_Tray_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Trays_Tray_UI : UserControl
    {
        public Trays_Tray_UI()
        {
            InitializeComponent();
        }

        Trays.Tray m_tray; 
        public void Init(Trays.Tray tray)
        {
            m_tray = tray;
            DataContext = tray; 
        }
    }
}
