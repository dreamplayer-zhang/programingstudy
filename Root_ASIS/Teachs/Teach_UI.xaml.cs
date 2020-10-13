using System.Windows.Controls;

namespace Root_ASIS.Teachs
{
    /// <summary>
    /// Teach_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Teach_UI : UserControl
    {
        public Teach_UI()
        {
            InitializeComponent();
        }

        Teach m_teach; 
        public void Init(Teach teach)
        {
            m_teach = teach;
            DataContext = teach;
            memoryViewerUI.Init(teach.m_memoryPool.m_viewer, false); 
        }
    }
}
