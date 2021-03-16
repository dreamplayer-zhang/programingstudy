using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace RootTools.GAFs
{
    /// <summary>
    /// ALIDList_PopupUI.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ALIDList_PopupUI : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        static public bool m_bShow = false; 
        public ALIDList_PopupUI()
        {
            m_bShow = true; 
            InitializeComponent();
			Application.Current.MainWindow.Closed += MainWindow_Closed;
        }

		private void MainWindow_Closed(object sender, System.EventArgs e)
		{
            this.Close();
		}

		ALIDList m_listALID;
        IEngineer m_engineer;
        public void Init(ALIDList listALID, IEngineer engineer)
        {
            m_listALID = listALID;
            m_engineer = engineer; 
            DataContext = listALID;
            listViewALID.ItemsSource = listALID.p_aSetALID;
            if(m_listALID.p_aSetALID.Count != 0)
            {
                ALID alid = (ALID)listViewALID.Items[m_listALID.p_aSetALID.Count - 1];
                SetLableBinding(alid);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            m_bShow = false; 
        }

        private void buttonClearALID_Click(object sender, RoutedEventArgs e)
        {
            m_listALID.ClearALID();
            SetLableBinding(null);
            ALID alid = (ALID)listViewALID.Items[m_listALID.p_aSetALID.Count - 1];
            SetLableBinding(alid);
        }

        private void TextBlock_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ALID alid = (ALID)listViewALID.SelectedItem;
            SetLableBinding(alid); 
        }

        void SetLableBinding(ALID alid)
        {
            lableALID.DataContext = alid;
            lableModule.DataContext = alid;
            lableDesc.DataContext = alid;
            lableMsg.DataContext = alid;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #region Title Bar

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
            NormalizeButton.Visibility = Visibility.Visible;
            MaximizeButton.Visibility = Visibility.Collapsed;
        }
        private void NormalizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
            MaximizeButton.Visibility = Visibility.Visible;
            NormalizeButton.Visibility = Visibility.Collapsed;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                    MaximizeButton.Visibility = Visibility.Visible;
                    NormalizeButton.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;
                    NormalizeButton.Visibility = Visibility.Visible;
                    MaximizeButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                this.DragMove();
            }
        }
        #endregion

        private void buttonBuzzerOff_Click(object sender, RoutedEventArgs e)
        {
            m_engineer.BuzzerOff(); 
        }

        private void buttonRecovery_Click(object sender, RoutedEventArgs e)
        {
            m_engineer.Recovery(); 
        }

        public void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
