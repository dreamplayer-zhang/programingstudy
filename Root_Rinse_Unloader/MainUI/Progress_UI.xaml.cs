using Root_Rinse_Unloader.Engineer;
using Root_Rinse_Unloader.Module;
using System.Windows.Controls;

namespace Root_Rinse_Unloader.MainUI
{
    /// <summary>
    /// Progress_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Progress_UI : UserControl
    {
        public Progress_UI()
        {
            InitializeComponent();
        }

        RinseU m_rinse;
        public void Init(RinseU rinse, RinseU_Engineer engineer)
        {
            m_rinse = rinse;
            DataContext = rinse;
            alidUI.Init(engineer.ClassGAF().m_listALID, engineer);
        }
    }
}
