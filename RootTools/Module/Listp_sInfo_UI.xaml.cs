using System.Windows.Controls;

namespace RootTools.Module
{
    /// <summary>
    /// Listp_sInfo_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Listp_sInfo_UI : UserControl
    {
        public Listp_sInfo_UI()
        {
            InitializeComponent();
        }

        Listp_sInfo m_listp_sInfo;
        public void Init(Listp_sInfo listp_sInfo)
        {
            m_listp_sInfo = listp_sInfo;
            DataContext = listp_sInfo;
        }
    }
}
