using Root_Rinse_Unloader.Engineer;
using System.Windows.Controls;

namespace Root_Rinse_Unloader.MainUI
{
    /// <summary>
    /// Main_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Main_UI : UserControl
    {
        public Main_UI()
        {
            InitializeComponent();
        }

        RinseU_Engineer m_engineer;
        public void Init(RinseU_Engineer engineer)
        {
            m_engineer = engineer;
            runUI.Init(engineer);
        }
    }
}
