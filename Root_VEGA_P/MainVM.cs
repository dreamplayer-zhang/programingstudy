using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Root_VEGA_P
{

    public class MainVM:ObservableObject
    {
        public MainWindow mainWindow;
        Home_ViewModel homeVM;
        public Home_ViewModel HomeVM
        {
            get => homeVM;
            set => SetProperty(ref homeVM, value);
        }
        UserControl subPanel;
        public UserControl MainSubPanel
        {
            get => subPanel;
            set => SetProperty(ref subPanel, value);
        }
        public MainVM(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            homeVM = new Home_ViewModel();
            MainSubPanel = homeVM.Main;
            MainSubPanel.DataContext = homeVM;
        }
    }
}
