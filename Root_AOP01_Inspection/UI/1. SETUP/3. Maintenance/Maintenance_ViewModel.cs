using Root_AOP01_Inspection.UI._1._SETUP._3._Maintenance;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_AOP01_Inspection
{
    public class Maintenance_ViewModel : ObservableObject
    {
        public Maintenance_Panel Maintenance = new Maintenance_Panel();

        Setup_ViewModel m_Setup;
        AOP01_Engineer_UI Engineer_UI;
        ViewerTest ToolViewerTest;
        ViewerTest_ViewModel ToolViewerTest_VM;
        public Maintenance_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            Engineer_UI = new AOP01_Engineer_UI();
            Engineer_UI.Init(m_Setup.m_MainWindow.m_engineer);
            ToolViewerTest_VM = new ViewerTest_ViewModel();
            ToolViewerTest_VM.Init(setup.m_MainWindow.m_engineer.ClassMemoryTool());
            ToolViewerTest = new ViewerTest();
            ToolViewerTest.DataContext = ToolViewerTest_VM;
        }
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
        public void SetPage(UserControl page)
        {
            Maintenance.SubPanel.Children.Clear();
            Maintenance.SubPanel.Children.Add(page);
        }
    }
}
