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
        RecipeBase[] recipes;

        public Setup_ViewModel()
        {
            Main = new Setup();
            init();
        }

        public void init()
        {
            recipes = new RecipeBase[4];

            recipes[0] = recipeCoverFront = GlobalObjects.Instance.Get<RecipeCoverFront>();
            recipes[1] = recipeCoverBack = GlobalObjects.Instance.Get<RecipeCoverBack>();
            recipes[2] = recipePlateFront = GlobalObjects.Instance.Get<RecipePlateFront>();
            recipes[3] = recipePlateBack = GlobalObjects.Instance.Get<RecipePlateBack>();

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
                    CreateRecipe(false);
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
            foreach (RecipeBase recipe in recipes)
            {
                if (!recipe.RecipePath.Equals(""))
                {
                    //기존에 Recipe가 열려있을 때
                    recipe.Save(recipe.RecipePath);
                    MessageBox.Show("Recipe Saved!");
                    return;
                }
            }

            CreateRecipe(true);
        }
        void LoadAllRecipe()
        {
            /*
             RootPath -> Recipe -> 부분별 Recipe Reading하고 PodInfo UI Update
             */

            PodIDInfo podIDInfo = new PodIDInfo();
            podIDInfo.ReadReg();
            DirectoryInfo dir = new DirectoryInfo(App.RecipeRootPath + podIDInfo.DualPodID); //현재 파드 정보 Path

            List<RecipeCoverFront> liCF = new List<RecipeCoverFront>();
            List<RecipeCoverBack> liCB = new List<RecipeCoverBack>();
            List<RecipePlateFront> liPF = new List<RecipePlateFront>();
            List<RecipePlateBack> liPB = new List<RecipePlateBack>();

            
            foreach (DirectoryInfo dirInfo in dir.GetDirectories()) // Recipe Lists
            {
                DirectoryInfo[] infos = dirInfo.GetDirectories();

                if (infos.Length != 4)
                    return;

                RecipeCoverFront cf = new RecipeCoverFront();
                RecipeCoverBack cb = new RecipeCoverBack();
                RecipePlateFront pf = new RecipePlateFront();
                RecipePlateBack pb = new RecipePlateBack();

                if(cb.Read(infos[0].FullName+"\\"+infos[0].Name+"_"+dirInfo.Name+".rcp"))
                    liCB.Add(cb);

                if(cf.Read(infos[1].FullName + "\\" + infos[1].Name + "_" + dirInfo.Name + ".rcp"))
                    liCF.Add(cf);

                if(pb.Read(infos[2].FullName + "\\" + infos[2].Name + "_" + dirInfo.Name + ".rcp"))
                    liPB.Add(pb);

                if(pf.Read(infos[3].FullName + "\\" + infos[3].Name + "_" + dirInfo.Name + ".rcp"))
                    liPF.Add(pf);
            }

            VegaPEventManager.OnLoadedAllRecipe(this, new LoadAllRecipeEventArgs(liCF, liCB, liPF, liPB));
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

                //부분별 Recipe Open할 때
                if (sFileName.Contains("Recipe"))
                    LoadPartsRecipe(sFileName, sFolderPath);
                else
                {
                    for(int i=0;i<4;i++)
                    {
                        if(recipes[i].Read(Path.Combine(sFolderPath+"\\"+App.RecipeNames[i], App.RecipeNames[i] + "_" + sFileName)))
                            VegaPEventManager.OnRecipeUpdated(this, new RecipeEventArgs(recipes[i]));
                    }

                    MessageBox.Show("Recipe Loaded!");
                }
            }
        }

        void LoadPartsRecipe(string sFileName,string sFolderPath)
        {
            bool saveRes = false;
            int selectedIdx = -1;

            for (int i = 0; i < 4; i++)
                if (sFileName.Contains(App.RecipeNames[i]))
                {
                    saveRes = recipes[i].Read(Path.Combine(sFolderPath, sFileName));
                    selectedIdx = i;
                    break;
                }

            if (saveRes)
            {
                MessageBox.Show("Recipe Loaded!");
                VegaPEventManager.OnRecipeUpdated(this, new RecipeEventArgs(recipes[selectedIdx]));
            }
        }
        public ICommand btnSaveAsRecipe
        {
            get => new RelayCommand(() => CreateRecipe(false));
        }
        public ICommand btnLoadRecipe
        {
            get =>new RelayCommand(()=>LoadRecipe());
        }
        public ICommand btnLoadAllRecipe
        {
            get => new RelayCommand(()=>LoadAllRecipe());
        }
        void CreateRecipe(bool IsCreate)
        {
            System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
            PodIDInfo podIDInfo = new PodIDInfo();
            podIDInfo.ReadReg();

            dlg.InitialDirectory = App.RecipeRootPath+podIDInfo.DualPodID;
            dlg.Title = "Save Recipe";
            dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                string sRecipeFolderPath = Path.Combine(sFolderPath, sFileNameNoExt); // 디렉토리명

                string[] RecipeFilePaths = { sRecipeFolderPath + @"\RecipeCoverFront", sRecipeFolderPath + @"\RecipeCoverBack",
                sRecipeFolderPath + @"\RecipePlateFront", sRecipeFolderPath + @"\RecipePlateBack"};

                foreach (string path in RecipeFilePaths)
                {
                    DirectoryInfo dir = new DirectoryInfo(path);
                    if (!dir.Exists)
                        dir.Create();
                }

                if(!IsCreate)
                {
                    if (!recipeCoverFront.RecipePath.Equals(""))
                        recipeCoverFront.Clear();
                    if (!recipeCoverBack.RecipePath.Equals(""))
                        recipeCoverBack.Clear();
                    if (!recipePlateFront.RecipePath.Equals(""))
                        recipePlateFront.Clear();
                    if (!recipePlateBack.RecipePath.Equals(""))
                        recipePlateBack.Clear();
                }

                using (StreamWriter writer = new StreamWriter(sRecipeFolderPath+"\\"+sFileName, true))
                {
                    //가라템;;
                    string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    writer.WriteLine(time + " - SaveRecipe()");
                }

                bool saveRes = false;
                saveRes = recipeCoverFront.Save(Path.Combine(RecipeFilePaths[0], "RecipeCoverFront_" + sFileName));
                saveRes = recipeCoverBack.Save(Path.Combine(RecipeFilePaths[1], "RecipeCoverBack_" + sFileName));
                saveRes = recipePlateFront.Save(Path.Combine(RecipeFilePaths[2], "RecipePlateFront_" + sFileName));
                saveRes = recipePlateBack.Save(Path.Combine(RecipeFilePaths[3], "RecipePlateBack_" + sFileName));

                if (saveRes)
                {
                    MessageBox.Show("Recipe Created!");
                }
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
