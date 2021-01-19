using Root_Rinse_Loader.Engineer;
using System.Windows.Controls;

namespace Root_Rinse_Loader.MainUI
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

        RinseL_Engineer m_engineer; 
        public void Init(RinseL_Engineer engineer)
        {
            m_engineer = engineer;
            runUI.Init(engineer); 
        }
    }
}
