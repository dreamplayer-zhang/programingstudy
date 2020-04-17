using RootTools;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Root_Wind
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public EQ.eState p_eState
        {
            get
            {
                return EQ.p_eState;
            }
        }

        #region Window Event
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }
        #endregion

        Wind_Engineer m_engineer = new Wind_Engineer();
    
        void Init()
        {
            this.DataContext = this;

            IDialogService dialogService = new DialogService(this);
            DialogInit(dialogService);
            m_engineer.Init("Wind");
            Maint.engineerUI.Init(m_engineer);

            _1_Mainview_ViewModel mvm = new _1_Mainview_ViewModel(m_engineer, dialogService);
            MainPage.DataContext = mvm;

            _2_3_OriginViewModel ovm = new _2_3_OriginViewModel(m_engineer, dialogService);
            _recipe._Origin.DataContext = ovm;
            //_recipe._recipeOrigin.DataContext = rovm;

            _2_4_SurfaceViewModel svm = new _2_4_SurfaceViewModel(m_engineer, dialogService);
            _recipe._Surface.DataContext = svm;

            _4_Viewer_ViewModel vvm = new _4_Viewer_ViewModel(m_engineer, dialogService);
            MainViewer.DataContext = vvm;
        }

        void DialogInit(IDialogService dialogService)
        {
            dialogService.Register<Dialog_ImageOpenViewModel, Dialog_ImageOpen>();
            dialogService.Register<Dialog_Scan_ViewModel, Dialog_Scan>();
            dialogService.Register<Dialog_ManualJob_ViewModel, Dialog_ManualJob>();
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            TabControl ff = (TabControl)sender;
            TabItem ti = (TabItem) ff.SelectedItem;
            tbViewName.Text = ti.Header.ToString();
            

        }
    }

    
}
