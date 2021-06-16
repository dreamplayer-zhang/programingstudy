using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using RootTools;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class Setup_ViewModel : ObservableObject, IDisposable
    {
        public UserControl p_CurrentPanel
        {
            get
            {
                return m_CurrentPanel;
            }
            set
            {
                SetProperty(ref m_CurrentPanel, value);
            }
        }
        private UserControl m_CurrentPanel;

        public ObservableCollection<UIElement> p_NaviButtons
        {
            get
            {
                return m_NaviButtons;
            }
            set
            {
                SetProperty(ref m_NaviButtons, value);
            }
        }
        private ObservableCollection<UIElement> m_NaviButtons = new ObservableCollection<UIElement>();

        private Home_ViewModel homeVM;

        public Setup Main;

        RecipeCoverFront recipeCoverFront;
        RecipeCoverBack recipeCoverBack;
        RecipePlateFront recipePlateFront;
        RecipePlateBack recipePlateBack;

        public Setup_ViewModel()
        {
            Main = new Setup();
            init();
        }

        public void init()
        {
            recipeCoverFront = GlobalObjects.Instance.Get<RecipeCoverFront>();
            recipeCoverBack  = GlobalObjects.Instance.Get<RecipeCoverBack>();
            recipePlateFront = GlobalObjects.Instance.Get<RecipePlateFront>();
            recipePlateBack  = GlobalObjects.Instance.Get<RecipePlateBack>();

            InitAllPanel();
            InitAllNaviBtn();
            SetHome();
        }

        private void InitAllPanel()
        {
            homeVM = new Home_ViewModel(this);

        }
        private void InitAllNaviBtn()
        {
            m_btnNaviInspection = new NaviBtn("Inspection");
            m_btnNaviInspection.Btn.Click += Navi_InspectionClick;

            m_btnNaviRecipeWizard = new NaviBtn("Recipe Wizard");
            m_btnNaviRecipeWizard.Btn.Click += Navi_RecipeWizardClick;

            m_btnNaviRecipeMask = new NaviBtn("Recipe Mask");
            m_btnNaviRecipeMask.Btn.Click += Navi_RecipeMaskClick; ;

            m_btnNaviMaintenance = new NaviBtn("Maintenance");
            m_btnNaviMaintenance.Btn.Click += Navi_MaintClick;

            m_btnNaviPodInfo = new NaviBtn("PodInfo");
            m_btnNaviPodInfo.Btn.Click += Navi_PodInfoClick;
        }

        #region Navi Buttons

        // SetupHome Navi Buttons
        public NaviBtn m_btnNaviInspection;
        public NaviBtn m_btnNaviRecipeWizard;
        public NaviBtn m_btnNaviPodInfo;
        public NaviBtn m_btnNaviRecipeMask;
        public NaviBtn m_btnNaviMaintenance;
        public NaviBtn m_btnNaviGEM;

        // 
        #endregion

        #region NaviBtn Event

        #region Main
        void Navi_InspectionClick(object sender, RoutedEventArgs e)
        {
            SetInspection();
        }
        void Navi_RecipeWizardClick(object sender, RoutedEventArgs e)
        {
            SetRecipeWizard();
        }
        void Navi_RecipeMaskClick(object sender, RoutedEventArgs e)
        {
            SetRecipeMask();
        }
        void Navi_MaintClick(object sender, RoutedEventArgs e)
        {
            SetMaintenance();
        }
        void Navi_PodInfoClick(object sender, RoutedEventArgs e)
        {
            SetPodInfo();
        }
        #endregion

        #region Command
        public ICommand btnMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetHome();
                    UIManager.Instance.ChangeUIMode();
                });
            }
        }
        public ICommand btnNaviSetupHome
        {
            get
            {
                return new RelayCommand(SetHome);
            }
        }
        public ICommand btnNewRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CreateRecipe();
                });
            }

        }

        public ICommand btnSaveRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SaveRecipe();
                });
            }
        }

        void SaveRecipe()
        {
            if (!recipeCoverFront.RecipePath.Equals(""))
            {
                recipeCoverFront.Save(recipeCoverFront.RecipePath);
                return;
            }

            if (!recipeCoverBack.RecipePath.Equals(""))
            {
                recipeCoverBack.Save(recipeCoverBack.RecipePath);
                return;
            }

            if (!recipePlateFront.RecipePath.Equals(""))
            {
                recipePlateFront.Save(recipePlateFront.RecipePath);
                return;
            }

            if (!recipePlateBack.RecipePath.Equals(""))
            {
                recipePlateBack.Save(recipePlateBack.RecipePath);
                return;
            }


            //System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            //dlg.InitialDirectory = App.RecipeRootPath;
            //dlg.Title = "Save Recipe";
            //dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
            //if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //    string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
            //    string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자

            //    DirectoryInfo dir = new DirectoryInfo(sFolderPath);
            //    if (!dir.Exists)
            //        dir.Create();

            //    recipeCoverFront.Save(Path.Combine(sFolderPath, "RecipeCoverFront_" + sFileName));
            //    recipeCoverBack.Save(Path.Combine(sFolderPath, "RecipeCoverBack_" + sFileName));
            //    recipePlateFront.Save(Path.Combine(sFolderPath, "RecipePlateFront_" + sFileName));
            //    recipePlateBack.Save(Path.Combine(sFolderPath, "RecipePlateBack_" + sFileName));
            //}
            CreateRecipe();
        }
        void LoadRecipe()
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.InitialDirectory = App.RecipeRootPath;
            dlg.Title = "Load Recipe";
            dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자

                DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                if (!dir.Exists)
                    dir.Create();

                recipeCoverFront.Read(Path.Combine(sFolderPath,"RecipeCoverFront_"+sFileName));
                recipeCoverBack.Read(Path.Combine(sFolderPath, "RecipeCoverBack_" + sFileName));
                recipePlateFront.Read(Path.Combine(sFolderPath, "RecipePlateFront_" + sFileName));
                recipePlateBack.Read(Path.Combine(sFolderPath, "RecipePlateBack_" + sFileName));
            }
        }
        
        public ICommand btnLoadRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    LoadRecipe();
                });
            }
        }

        void CreateRecipe()
        {
            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            dlg.InitialDirectory = Constants.RootPath.RecipeEBRRootPath;
            dlg.Title = "Save Recipe";
            dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명
                string sFullPath = Path.Combine(sRecipeFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                DirectoryInfo dir = new DirectoryInfo(sRecipeFolderPath);
                if (!dir.Exists)
                    dir.Create();

                recipeCoverFront.Clear();
                recipeCoverBack.Clear();
                recipePlateFront.Clear();
                recipePlateBack.Clear();

                recipeCoverFront.Save(Path.Combine(sFolderPath, "RecipeCoverFront_" + sFileName));
                recipeCoverBack.Save(Path.Combine(sFolderPath, "RecipeCoverBack_" + sFileName));
                recipePlateFront.Save(Path.Combine(sFolderPath, "RecipePlateFront_" + sFileName));
                recipePlateBack.Save(Path.Combine(sFolderPath, "RecipePlateBack_" + sFileName));
            }
        }

        #endregion

        #region Panel Change Method

        #region Main
        public void SetHome()
        {
            p_NaviButtons.Clear();
            p_CurrentPanel = homeVM.Main;
        }
        public void SetInspection()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviInspection);
        }
        public void SetRecipeWizard()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
        }
        public void SetRecipeMask() 
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviRecipeWizard);
            p_NaviButtons.Add(m_btnNaviRecipeMask);
        }
        public void SetPodInfo()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviPodInfo);
        }

        public void SetMaintenance()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviMaintenance);
        }
        public void SetGEM()
        {
            p_NaviButtons.Clear();
            p_NaviButtons.Add(m_btnNaviGEM);
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion


        #endregion
        #endregion

    }



}
