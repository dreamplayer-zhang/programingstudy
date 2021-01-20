using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class EBR_ViewModel : ObservableObject
    {
        Setup_ViewModel setupVM;
        RecipeBase recipe;

        public EBR_Panel Main;
        public EBRSetup_ViewModel SetupVM;
        public EBRSetupPage SetupPage;

        public EBR_ViewModel(Setup_ViewModel setup)
        {
            this.setupVM = setup;
            Init();
        }

        public void Init()
        {
            Main = new EBR_Panel();
            SetupVM = new EBRSetup_ViewModel();
            SetupVM.Init(setupVM);

            SetupPage = new EBRSetupPage();
            SetupPage.DataContext = SetupVM;
            SetPage(SetupPage);
        }

        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnEBRSetup
        {
            get
            {
                return new RelayCommand(() => SetPage(SetupPage));
            }
        }
        public ICommand btnEBRSnap
        {
            get
            {
                return new RelayCommand(() => SetupVM.Scan());
            }
        }

        public ICommand btnEBRInsp
        {
            get
            {
                return new RelayCommand(() => SetupVM.Inspect());
            }
        }

        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() => setupVM.SetRecipeWizard());
            }
        }

        public void UI_Redraw()
        {
            SetupVM.LoadParameter();
        }
    }
}
