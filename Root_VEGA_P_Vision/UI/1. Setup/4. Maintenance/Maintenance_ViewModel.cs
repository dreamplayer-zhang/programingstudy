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
        public VEGA_P_Vision_Handler_UI HandlerUI;
        VEGA_P_Vision_Engineer_UI EngineerUI;
        Setup_ViewModel m_Setup;
        ToolBox_UI ToolBoxUI;
        ViewerTest ToolViewerTest;
        ViewerTest_ViewModel ToolViewerTest_VM;

        public Maintenance_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;

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

            Init();
        }
        public void Init()
        {
            Main = new MaintenancePanel();
        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        #region [RelayCommand]
        public ICommand btnHandler
        {
            get => new RelayCommand(() => SetPage(EngineerUI));
        }
        public ICommand btnBack
        {
            get => new RelayCommand(() => m_Setup.SetHome());
        }
        public ICommand btnToolBox
        {
            get => new RelayCommand(() => SetPage(ToolBoxUI));
        }
        public ICommand btnViewerTest
        {
            get => new RelayCommand(() => SetPage(ToolViewerTest));
        }
        #endregion
    }
}
