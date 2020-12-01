using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Root_WIND2.UI;
using RootTools;
using RootTools.Module;
using RootTools.ToolBoxs;

namespace Root_WIND2
{
    class Maintenance_ViewModel : ObservableObject
    {
        public MaintenancePanel Main;
        Setup_ViewModel m_Setup;
        WIND2_Hander_UI HandlerUI;
        ToolBox_UI ToolBoxUI;
        ViewerTest ToolViewerTest;
        ViewerTest_ViewModel ToolViewerTest_VM;


        public Maintenance_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            HandlerUI = new WIND2_Hander_UI();
            ToolBoxUI= new ToolBox_UI();
            ToolViewerTest_VM = new ViewerTest_ViewModel();
            ToolViewerTest_VM.Init(ProgramManager.Instance.Engineer.ClassMemoryTool());
            ToolViewerTest = new ViewerTest();
            ToolViewerTest.DataContext = ToolViewerTest_VM;

            HandlerUI.Init(ProgramManager.Instance.Engineer.m_handler);
            ToolBoxUI.Init(ProgramManager.Instance.Engineer.ClassToolBox());

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

        public ICommand btnHandler
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(HandlerUI);
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
