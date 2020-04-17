using System.Windows.Controls;

namespace RootTools.DMC
{
    /// <summary>
    /// DMCListDIO_UI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DMCListDIO_UI : UserControl
    {
        public DMCListDIO_UI()
        {
            InitializeComponent();
        }

        DMCListDIO m_listDIO;
        public void Init(DMCListDIO listDIO)
        {
            m_listDIO = listDIO;
            this.DataContext = listDIO;
            listView.ItemsSource = listDIO.m_aDIO;
        }
        
    }
}
