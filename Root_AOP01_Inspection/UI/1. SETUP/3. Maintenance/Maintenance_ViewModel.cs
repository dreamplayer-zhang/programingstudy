using RootTools_Vision;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_AOP01_Inspection
{
    public class Maintenance_ViewModel : ObservableObject
    {
        public Maintenance_Panel Maintenance = new Maintenance_Panel();

        Setup_ViewModel m_Setup;
        AOP01_Engineer_UI Engineer_UI;
        AOP01_ToolBox_UI ToolBox_UI;
        ViewerTest ToolViewerTest;
        ViewerTest_ViewModel ToolViewerTest_VM;

        public Maintenance_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            Engineer_UI = new AOP01_Engineer_UI();
            Engineer_UI.Init(GlobalObjects.Instance.Get<AOP01_Engineer>());
            ToolBox_UI = new AOP01_ToolBox_UI();
            ToolBox_UI.Init(GlobalObjects.Instance.Get<AOP01_Engineer>());
            ToolViewerTest_VM = new ViewerTest_ViewModel();
            ToolViewerTest_VM.Init(GlobalObjects.Instance.Get<AOP01_Engineer>().ClassMemoryTool());
            ToolViewerTest = new ViewerTest();
            ToolViewerTest.DataContext = ToolViewerTest_VM;
        }
        
        public void SetPage(UserControl page)
        {
            Maintenance.SubPanel.Children.Clear();
            Maintenance.SubPanel.Children.Add(page);
        }
        #region RelayCommand
        public ICommand btnSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //Maintenance.EngineerBtn.IsChecked = true;
                });
            }
        }
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_HomePanel();
                });
            }
        }
        public ICommand btnViewerTest
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(ToolViewerTest);
                });

            }
        }

        public ICommand btnEngineerTest
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Engineer_UI);
                });

            }
        }

        public ICommand btnToolBox
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(ToolBox_UI);
                });
            }
        }
        #endregion
    }
}
