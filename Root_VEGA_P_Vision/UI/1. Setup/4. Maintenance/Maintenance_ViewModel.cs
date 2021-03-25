using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Root_VEGA_P_Vision.Engineer;
using Root_VEGA_P_Vision.UI;
using RootTools;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class Maintenance_ViewModel : ObservableObject
    {
        public MaintenancePanel Main;
        Setup_ViewModel m_Setup;
        ToolBox_UI ToolBoxUI;
        ViewerTest ToolViewerTest;
        ViewerTest_ViewModel ToolViewerTest_VM;

        public Maintenance_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            ToolBoxUI= new ToolBox_UI();
            ToolViewerTest_VM = new ViewerTest_ViewModel();
            ToolViewerTest_VM.Init(GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().ClassMemoryTool());
            ToolViewerTest = new ViewerTest();
            ToolViewerTest.DataContext = ToolViewerTest_VM;
            ToolBoxUI.Init(GlobalObjects.Instance.Get<VEGA_P_Vision_Engineer>().ClassToolBox());

            init();
        }
        public void init()
        {
            Main = new MaintenancePanel();
        }

        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetHome();
                });
            }
        }

        public ICommand btnToolBox
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(ToolBoxUI);
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


    }
}
