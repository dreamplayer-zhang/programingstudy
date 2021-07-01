using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_VEGA_D
{
    class MainWindow_ViewModel : ObservableObject
    {

        MainWindow m_MainWindow;

        #region Dialog
        public DialogService m_dialogService;
        #endregion

        #region ViewModel

        //RecipeWizard_VM m_recipeWizard_ViewModel;
        //public RecipeWizard_VM p_recipeWizard_ViewModel
        //{
        //    get
        //    {
        //        return m_recipeWizard_ViewModel;
        //    }
        //    set
        //    {
        //        SetProperty(ref m_recipeWizard_ViewModel, value);
        //    }
        //}
        RecipeManager_VM m_recipeManager_ViewModel;
        public RecipeManager_VM p_recipeManager_ViewModel
        {
            get
            {
                return m_recipeManager_ViewModel;
            }
            set
            {
                SetProperty(ref m_recipeManager_ViewModel, value);
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
            p_recipeManager_ViewModel = new RecipeManager_VM();
            //p_recipeWizard_ViewModel = new RecipeWizard_VM();
        }
        private void DialogInit(MainWindow main)
        {
            m_dialogService = new DialogService(main);
        }


        #region Command
        public ICommand CmdReview
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MessageBox.Show("Review");
                });
            }
        }

        #endregion
    }
}
