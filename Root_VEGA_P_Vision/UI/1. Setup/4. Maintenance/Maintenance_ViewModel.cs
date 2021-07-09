using System.Windows.Controls;
using System.Windows.Input;
using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.UI;
using RootTools.ToolBoxs;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class Maintenance_ViewModel : ObservableObject
    {
        public MaintenancePanel Main;
        UserControl subPanel;
        public UserControl pSubPanel
        {
            get => subPanel;
            set => SetProperty(ref subPanel, value);
        }
        public VEGA_P_Vision_Handler_UI HandlerUI;
        VEGA_P_Vision_Engineer_UI EngineerUI;
        Setup_ViewModel m_Setup;
        Home_ViewModel home;
        ToolBox_UI ToolBoxUI;
        ViewerTest ToolViewerTest;
        ViewerTest_ViewModel ToolViewerTest_VM;
        PodInfoRecipe_ViewModel podInfoRecipe;
        public Maintenance_ViewModel(Home_ViewModel home)
        {
            this.home = home;
            m_Setup = home.m_Setup;

            HandlerUI = new VEGA_P_Vision_Handler_UI();
            EngineerUI = new VEGA_P_Vision_Engineer_UI();
            ToolBoxUI= new ToolBox_UI();

            ToolViewerTest_VM = new ViewerTest_ViewModel();
            ToolViewerTest_VM.Init(GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().ClassMemoryTool());
            ToolViewerTest = new ViewerTest();
            ToolViewerTest.DataContext = ToolViewerTest_VM;

            EngineerUI.Init(GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>());
            HandlerUI.Init(GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().m_handler);
            ToolBoxUI.Init(GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().ClassToolBox());

            podInfoRecipe = new PodInfoRecipe_ViewModel();
            Init();
            pSubPanel = EngineerUI;
            pSubPanel.DataContext = EngineerUI.DataContext;
        }
        public void Init()
        {
            Main = new MaintenancePanel();
            Main.DataContext = this;
        }

        #region [RelayCommand]
        public ICommand btnHandler
        {
            get => new RelayCommand(() => {
                pSubPanel = EngineerUI;
                pSubPanel.DataContext = EngineerUI.DataContext;
            });
        }
        public ICommand btnBack
        {
            get => new RelayCommand(() => m_Setup.SetHome());
        }
        //public ICommand btnToolBox
        //{
        //    //get => new RelayCommand(() => SetPage(ToolBoxUI));
        //}
        //public ICommand btnViewerTest
        //{
        //    get => new RelayCommand(() => SetPage(ToolViewerTest));
        //}
        public ICommand btnPodInfo
        {
            get => new RelayCommand(() => {
                pSubPanel = podInfoRecipe.Main;
                pSubPanel.DataContext = podInfoRecipe;
            });
        }
        #endregion
    }
}
