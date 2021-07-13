using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Root_VEGA_D
{
    public class RecipeManager_VM : ObservableObject
    {
        public RecipeManager_VM()
        {
            InitViewModel();
        }

        #region property
        RecipeWizard_VM m_recipeWizard_ViewModel;
        public RecipeWizard_VM p_recipeWizard_ViewModel
        {
            get => m_recipeWizard_ViewModel;
            set
            {
                SetProperty(ref m_recipeWizard_ViewModel, value);
            }
        }

        DateTime m_selectedStartDate;
        public DateTime p_selectedStartDate
        {
            get => m_selectedStartDate;
            set
            {
                SetProperty(ref m_selectedStartDate, value);
            }
        }

        DateTime m_selectedEndDate;
        public DateTime p_selectedEndData
        {
            get => m_selectedEndDate;
            set
            {
                SetProperty(ref m_selectedEndDate, value);
            }
        }

        bool m_isCheckStartDate;
        public bool p_isCheckStartDate
        {
            get => m_isCheckStartDate;
            set
            {
                SetProperty(ref m_isCheckStartDate, value);
            }
        }
        bool m_isCheckEndDate;
        public bool p_isCheckEndDate
        {
            get => m_isCheckEndDate;
            set
            {
                SetProperty(ref m_isCheckEndDate, value);
            }
        }
        bool m_isCheckMaskID;
        public bool p_isCheckMaskID
        {
            get => m_isCheckMaskID;
            set
            {
                SetProperty(ref m_isCheckMaskID, value);
            }
        }
        bool m_isCheckRecipeName;
        public bool p_isCheckRecipeName
        {
            get => m_isCheckRecipeName;
            set
            {
                SetProperty(ref m_isCheckRecipeName, value);
            }
        }

        BitmapSource m_bitmapAlignKeySrc;
        public BitmapSource p_bitmapAlignKeySrc
        {
            get => m_bitmapAlignKeySrc;
            set => SetProperty(ref m_bitmapAlignKeySrc, value);
        }
        #endregion

        #region Function
        void InitViewModel()
        {
            p_recipeWizard_ViewModel = new RecipeWizard_VM();
        }

        void SearchRecipe()
        {

        }

        void NewRecipe()
        {

        }

        void SaveRecipe()
        {

        }

        void DeleteRecipe()
        {

        }
        #endregion
        #region Command
        public ICommand CmdSearchRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SearchRecipe();
                });
            }
        }
        public ICommand CmdNewRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    NewRecipe();
                });
            }
        }
        public ICommand CmdSaveRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SaveRecipe();  
                });
            }
        }
        public ICommand CmdDeleteRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    DeleteRecipe();
                });
            }
        }
        #endregion
    }
}
