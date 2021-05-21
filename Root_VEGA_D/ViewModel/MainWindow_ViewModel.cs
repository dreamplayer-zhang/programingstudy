using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_D
{
    class MainWindow_ViewModel : ObservableObject
    {

        MainWindow m_MainWindow;

        #region Dialog
        public DialogService m_dialogService;
        #endregion

        #region ViewModel
        
        RecipeWizard_VM m_recipeWizard_ViewModel;
        public RecipeWizard_VM p_recipeWizard_ViewModel
        {
            get
            {
                return m_recipeWizard_ViewModel;
            }
            set
            {
                SetProperty(ref m_recipeWizard_ViewModel, value);
            }
        }
        #endregion
        public MainWindow_ViewModel(MainWindow mainwindow)
        {
            m_MainWindow = mainwindow;

            InitViewModel();
            DialogInit(m_MainWindow);
        }

        void InitViewModel()
        {
            p_recipeWizard_ViewModel = new RecipeWizard_VM();
        }
        private void DialogInit(MainWindow main)
        {
            m_dialogService = new DialogService(main);
        }
    }
}
