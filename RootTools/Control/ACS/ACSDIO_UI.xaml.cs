using System.Windows.Controls;

namespace RootTools.Control.ACS
{
    /// <summary>
    /// ACSDIO_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ACSDIO_UI : UserControl
    {
        public ACSDIO_UI()
        {
            InitializeComponent();
        }

        ACSDIO m_ACSDIO;
        public void Init(ACSDIO ACSDIO)
        {
            m_ACSDIO = ACSDIO;
            this.DataContext = ACSDIO;
            listInputUI.Init(ACSDIO.p_listDI);
            listOutputUI.Init(ACSDIO.p_listDO);
        }
    }
}
