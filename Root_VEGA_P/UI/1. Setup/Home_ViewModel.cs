using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_VEGA_P
{
    public class Home_ViewModel:ObservableObject
    {
        public Home_Panel Main;
        UserControl subPanel;
        public Maintenance_Panel MainT;
        PodRecipe_ViewModel PodRecipeVM;
        public UserControl SubPanel
        {
            get => subPanel;
            set => SetProperty(ref subPanel, value);
        }
        public Home_ViewModel()
        {
            Main = new Home_Panel();
            Main.DataContext = this;
            MainT = new Maintenance_Panel();
            PodRecipeVM = new PodRecipe_ViewModel();
            SubPanel = PodRecipeVM.Main;
        }
        public ICommand btnPodRecipe
        {
            get => new RelayCommand(() => {
                SubPanel = PodRecipeVM.Main;
            });
        }
        public ICommand btnMaintenance
        {
            get => new RelayCommand(() => {
                SubPanel = MainT;
            });
        }
        public ICommand btnOperate
        {
            get => new RelayCommand(() => { 
            });
        }
    }
}
