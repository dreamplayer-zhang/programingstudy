using RootTools.GAFs;
using System.Windows.Controls;

namespace Root_Pine2.Engineer
{
    /// <summary>
    /// Pine2_Main_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pine2_Main_UI : UserControl
    {
        public Pine2_Main_UI()
        {
            InitializeComponent();
        }

        Pine2_Handler m_handler;
        GAF m_gaf; 
        public void Init(Pine2_Engineer engineer)
        {
            m_handler = (Pine2_Handler)engineer.ClassHandler();
            m_gaf = engineer.ClassGAF(); 
            processUI.Init(m_handler);
            alidUI.Init(m_gaf.m_listALID, engineer);
            summaryUI.Init(m_handler.m_summary); 
        }
    }
}
