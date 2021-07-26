using Root_VEGA_D_IPU.Engineer;
using RootTools_Vision;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Root_VEGA_D_IPU
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        MainWindow_ViewModel m_mainWindow_ViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Window Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(@"C:\Recipe\VEGA_D_IPU")) Directory.CreateDirectory(@"C:\Recipe\VEGA_D_IPU");
            Init();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ThreadStop();
        }

        VEGA_D_IPU_Engineer m_engineer = new VEGA_D_IPU_Engineer();
        void Init()
        {
            RegisterGlobalObjects();

            m_mainWindow_ViewModel = new MainWindow_ViewModel(this);
            this.DataContext = m_mainWindow_ViewModel;

            m_engineer.Init("VEGA_D_IPU");
            engineerUI.Init(m_engineer);
        }

        void RegisterGlobalObjects()
        {
            GlobalObjects.Instance.RegisterNamed<string>("RecipeFile", "");

            GlobalObjects.Instance.RegisterNamed<bool>("PerformInspection", true);
            GlobalObjects.Instance.RegisterNamed<bool>("D2DInspection", true);
            GlobalObjects.Instance.RegisterNamed<bool>("SurfaceInspection", true);
            GlobalObjects.Instance.RegisterNamed<bool>("CustomOption1", true);
            GlobalObjects.Instance.RegisterNamed<bool>("CustomOption2", true);
        }

        void ThreadStop()
        {
            m_engineer.ThreadStop();
        }
        #endregion
    }
}
