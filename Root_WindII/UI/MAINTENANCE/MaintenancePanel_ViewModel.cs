using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Root_WindII.Engineer;
using RootTools;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools_Vision;

namespace Root_WindII
{
    public class MaintenancePanel_ViewModel : ObservableObject
    {
        public MaintenancePanel Main;
        public WindII_Engineer_UI EngineerUI;
        public WindII_Handler_UI HandlerUI;
        public WindII_ToolBox_UI ToolBoxUI;

        public MaintenancePanel_ViewModel()
        {
            EngineerUI = new WindII_Engineer_UI();
            HandlerUI = new WindII_Handler_UI();
            ToolBoxUI= new WindII_ToolBox_UI();

            EngineerUI.Init(GlobalObjects.Instance.Get<WindII_Engineer>());
            HandlerUI.Init(GlobalObjects.Instance.Get<WindII_Engineer>().m_handler);
            ToolBoxUI.Init(GlobalObjects.Instance.Get<WindII_Engineer>());

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
        
        public ICommand btnEngineer
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(EngineerUI);
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
    }
}
