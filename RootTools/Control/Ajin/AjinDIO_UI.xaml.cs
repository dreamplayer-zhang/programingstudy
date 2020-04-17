using System.Windows.Controls;

namespace RootTools.Control.Ajin
{
    /// <summary>
    /// AjinDIO_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class AjinDIO_UI : UserControl
    {
        public AjinDIO_UI()
        {
            InitializeComponent();
        }

        AjinDIO m_ajinDIO; 
        public void Init(AjinDIO ajinDIO)
        {
            m_ajinDIO = ajinDIO;
            this.DataContext = ajinDIO;
            listInputUI.Init(ajinDIO.p_listDI);
            listOutputUI.Init(ajinDIO.p_listDO); 
        }
    }
}
