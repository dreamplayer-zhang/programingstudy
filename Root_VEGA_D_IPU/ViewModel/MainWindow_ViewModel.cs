using Microsoft.Win32;
using Root_VEGA_D_IPU.Module.Recipe;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_VEGA_D_IPU
{
    class MainWindow_ViewModel : ObservableObject
    {
        MainWindow m_mainWindow;

        ADIRecipe m_recipeData;

        public DialogService m_dialogService;

        RootTools.Registry m_reg;

        #region Property
        string m_sRecipeFile = "";
        public string p_sRecipeFile
        {
            get => m_sRecipeFile;
            set => SetProperty(ref m_sRecipeFile, value);
        }

        bool m_bPerformInspection = true;
        public bool p_bPerformInspection
        {
            get => (bool)GlobalObjects.Instance.GetNamed<bool>("PerformInspection");
            set
            {
                m_reg.Write(REGSUBKEYNAME_PERFORMINSPECTION, value);
                GlobalObjects.Instance.SetNamed<bool>("PerformInspection", value);
                RaisePropertyChanged();
            }
        }
        public bool p_bD2DInspection
        {
            get => (bool)GlobalObjects.Instance.GetNamed<bool>("D2DInspection");
            set
            {
                m_reg.Write(REGSUBKEYNAME_D2DINSPECTION, value);
                GlobalObjects.Instance.SetNamed<bool>("D2DInspection", value);
                RaisePropertyChanged();
            }
        }
        public bool p_bSurfaceInspection
        {
            get => (bool)GlobalObjects.Instance.GetNamed<bool>("SurfaceInspection");
            set
            {
                m_reg.Write(REGSUBKEYNAME_SURFACEINSPECTION, value);
                GlobalObjects.Instance.SetNamed<bool>("SurfaceInspection", value);
                RaisePropertyChanged();
            }
        }
        public bool p_bCustomOption1
        {
            get => (bool)GlobalObjects.Instance.GetNamed<bool>("CustomOption1");
            set
            {
                m_reg.Write(REGSUBKEYNAME_CUSTOMOPTION1, value);
                GlobalObjects.Instance.SetNamed<bool>("CustomOption1", value);
                RaisePropertyChanged();
            }
        }
        public bool p_bCustomOption2
        {
            get => (bool)GlobalObjects.Instance.GetNamed<bool>("CustomOption2");
            set
            {
                m_reg.Write(REGSUBKEYNAME_CUSTOMOPTION1, value);
                GlobalObjects.Instance.SetNamed<bool>("CustomOption2", value);
                RaisePropertyChanged();
            }
        }

        #endregion

        public MainWindow_ViewModel(MainWindow mainwindow)
        {
            m_mainWindow = mainwindow;

            DialogInit(m_mainWindow);

            Init();
        }

        private void DialogInit(MainWindow main)
        {
            m_dialogService = new DialogService(main);
        }

        void Init()
        {
            p_sRecipeFile = GlobalObjects.Instance.GetNamed<string>("RecipeFile");

            BrowseRecipeFileBtnCommand = new RelayCommand(BrowseRecipeFile);
            SaveRecipeFileBtnCommand = new RelayCommand(SaveRecipeFile);
            LoadRecipeFileBtnCommand = new RelayCommand(LoadRecipeFile);

            m_reg = new RootTools.Registry("VEGA-D_IPU");

            ReadRegistryData();
        }

         
        const string REGSUBKEYNAME_PERFORMINSPECTION = "PerformInspection";
        const string REGSUBKEYNAME_D2DINSPECTION = "D2DInspection";
        const string REGSUBKEYNAME_SURFACEINSPECTION = "SurfaceInspection";
        const string REGSUBKEYNAME_CUSTOMOPTION1 = "CustomOption1";
        const string REGSUBKEYNAME_CUSTOMOPTION2 = "CustomOption2";

        void ReadRegistryData()
        {
            p_bPerformInspection = m_reg.Read(REGSUBKEYNAME_PERFORMINSPECTION, p_bPerformInspection);
            p_bD2DInspection = m_reg.Read(REGSUBKEYNAME_D2DINSPECTION, p_bD2DInspection);
            p_bSurfaceInspection = m_reg.Read(REGSUBKEYNAME_SURFACEINSPECTION, p_bSurfaceInspection);
            p_bCustomOption1 = m_reg.Read(REGSUBKEYNAME_CUSTOMOPTION1, p_bCustomOption1);
            p_bCustomOption2 = m_reg.Read(REGSUBKEYNAME_CUSTOMOPTION2, p_bCustomOption2);
        }

        public ICommand BrowseRecipeFileBtnCommand { get; set; }
        public ICommand SaveRecipeFileBtnCommand { get; set; }
        public ICommand LoadRecipeFileBtnCommand { get; set; }
        public ICommand RecipeDataEditEndingCommand { get; set; }
        private void BrowseRecipeFile()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Recipe Files (*.csv)|*.csv|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == false) return;

            p_sRecipeFile = dlg.FileName;
        }
        private void SaveRecipeFile()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Recipe Files (*.csv)|*.csv|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == false) return;

            m_recipeData.Save(dlg.FileName);
        }
        private void LoadRecipeFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Recipe Files (*.csv)|*.csv|All Files (*.*)|*.*";
            if (dlg.ShowDialog() == false) return;

            m_recipeData = ADIRecipe.Load(dlg.FileName);
        }
    }
}
