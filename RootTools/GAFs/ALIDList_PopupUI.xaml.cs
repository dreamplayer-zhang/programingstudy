using System.ComponentModel;
using System.Windows;

namespace RootTools.GAFs
{
    /// <summary>
    /// ALIDList_PopupUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ALIDList_PopupUI : Window
    {
        static public bool m_bShow = false; 
        public ALIDList_PopupUI()
        {
            m_bShow = true; 
            InitializeComponent();
        }

        ALIDList m_listALID;
        public void Init(ALIDList listALID)
        {
            m_listALID = listALID;
            DataContext = listALID;
            listViewALID.ItemsSource = listALID.p_aSetALID; 
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_bShow = false; 
        }

        private void buttonClearALID_Click(object sender, RoutedEventArgs e)
        {
            m_listALID.ClearALID(); 
        }
    }
}
