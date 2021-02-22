using Root_WIND2;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class Frontside_ViewModel : ObservableObject
    {

        public FrontSide_Panel      Main;
        public FrontsideSummary     Summary;
        public FrontSideMap         Map;
        public FrontSideOrigin      Origin;
        public FrontSidePosition   Position;
        public FrontSideMask         ROI;
        public FrontSideSpec        Spec;
        public FrontsideInspection        InspTestPage;

        private FrontsideSummary_ViewModel m_Summary_VM;
        public FrontsideSummary_ViewModel p_Summary_VM
        {
            get
            {
                return m_Summary_VM;
            }
            set
            {
                SetProperty(ref m_Summary_VM, value);
            }
        }

        private FrontsideOrigin_ViewModel m_Origin_VM;
        public FrontsideOrigin_ViewModel p_Origin_VM
        {
            get
            {
                return m_Origin_VM;
            }
            set
            {
                SetProperty(ref m_Origin_VM, value);
            }
        }
        private FrontsidePosition_ViewModel m_Position_VM;
        public FrontsidePosition_ViewModel p_Position_VM
        {
            get
            {
                return m_Position_VM;
            }
            set
            {
                SetProperty(ref m_Position_VM, value);
            }
        }
        private FrontsideMask_ViewModel m_ROI_VM;
        public FrontsideMask_ViewModel p_ROI_VM
        {
            get
            {
                return m_ROI_VM;
            }
            set
            {
                SetProperty(ref m_ROI_VM, value);
            }
        }
        private FrontsideSpec_ViewModel m_Spec_VM;
        public FrontsideSpec_ViewModel p_Spec_VM
        {
            get
            {
                return m_Spec_VM;
            }
            set
            {
                SetProperty(ref m_Spec_VM, value);
            }
        }
        private FrontsideMap_ViewModel m_Map_VM;
        public FrontsideMap_ViewModel p_Map_VM
        {
            get
            {
                return m_Map_VM;
            }
            set
            {
                SetProperty(ref m_Map_VM, value);
            }
        }

        private FronsideInspection_ViewModel m_InspTest_VM;
        public FronsideInspection_ViewModel p_InspTest_VM
        {
            get
            {
                return m_InspTest_VM;
            }
            set
            {
                SetProperty(ref m_InspTest_VM, value);
            }
        }



        Setup_ViewModel m_Setup;
        RecipeBase m_Recipe = null;
        public Frontside_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;

            p_Summary_VM = new FrontsideSummary_ViewModel();
            p_Summary_VM.init(setup);
            
            p_Origin_VM = new FrontsideOrigin_ViewModel();
            p_Origin_VM.SetOrigin += P_Origin_VM_SetOrigin;
            p_Origin_VM.init(setup);

            p_Position_VM = new FrontsidePosition_ViewModel();
            p_Position_VM.init(setup);

            p_ROI_VM = new FrontsideMask_ViewModel();
            p_ROI_VM.Init(setup);

            p_Spec_VM = new FrontsideSpec_ViewModel();
            p_Spec_VM.init(this);

            p_Origin_VM.MapControl_VM.SetMasterDie += P_Origin_VM_SetMasterDie;

            p_InspTest_VM = new FronsideInspection_ViewModel();
            p_InspTest_VM.init(setup);

            Init();

            // 구조가 ...

            m_Map_VM = new FrontsideMap_ViewModel();
            m_Map_VM.Init(setup, Map, m_Recipe);

            p_Summary_VM.ConnectInspItemDataGrid(p_Spec_VM);


        }

        private void P_Origin_VM_SetOrigin(object e)
        {
            p_ROI_VM.SetOrigin(e);
        }
        private void P_Origin_VM_SetMasterDie(object e)
        {
            p_Origin_VM.SetMasterDie(e);
        }
        public void UpdateUI()
        {
            // 추후에 관리 통합할 수 있도록 변경
            p_Map_VM.Load(); // Map
            m_Origin_VM.Load(); // Origin
            p_Position_VM.Load(); // Position
            p_Spec_VM.Load();
            p_Summary_VM.Load();
            p_InspTest_VM.Load();
            p_ROI_VM.Load();
        }

        public void Init()
        {
            Main = new FrontSide_Panel();
            Summary = new FrontsideSummary();
            Map = new FrontSideMap();
            Origin = new FrontSideOrigin();
            Position = new FrontSidePosition();
            ROI = new FrontSideMask();
            Spec = new FrontSideSpec();
            InspTestPage = new FrontsideInspection();

            SetPage(Map);
            SetPage(Origin);
            SetPage(Position);
            SetPage(ROI);
            SetPage(Spec);
            SetPage(Summary);
            SetPage(InspTestPage);

        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public ICommand btnFrontSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Summary);
                    p_Summary_VM.SetPage();
                });
            }
        }

        public ICommand btnFrontNewRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                    dlg.InitialDirectory = Constants.Path.RecipeFrontRootPath;
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

                        RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
                        recipe.Clear();

                        recipe.Name = sFileNameNoExt;
                        recipe.RecipePath = sFullPath;
                        recipe.RecipeFolderPath = sRecipeFolderPath;

                        recipe.Save(sFullPath);
                    }
                });
            }
        }

        public ICommand btnFrontSaveRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
                    if(recipe.RecipePath != "")
                    {
                        recipe.Save(recipe.RecipePath);
                    }
                    else
                    {
                        System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog();
                        dlg.InitialDirectory = Constants.Path.RecipeFrontRootPath;
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

                            recipe.Name = sFileNameNoExt;
                            recipe.RecipePath = sFullPath;
                            recipe.RecipeFolderPath = sRecipeFolderPath;

                            recipe.Save(sFullPath);
                        }
                    }
                });
            }
        }

        public ICommand btnFrontLoadRecipe
        {
            get
            {
                return new RelayCommand(() =>
                {
                    System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                    dlg.InitialDirectory = Constants.Path.RecipeFrontRootPath;
                    dlg.Title = "Load Recipe";
                    dlg.Filter = "ATI files (*.rcp)|*.rcp|All files (*.*)|*.*";
                    if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string sFolderPath = Path.GetDirectoryName(dlg.FileName); // 디렉토리명
                        string sFileNameNoExt = Path.GetFileNameWithoutExtension(dlg.FileName); // Only 파일이름
                        string sFileName = Path.GetFileName(dlg.FileName); // 파일이름 + 확장자
                        string sFullPath = Path.Combine(sFolderPath, sFileName); // 레시피 이름으 된 폴더안의 rcp 파일 경로

                        DirectoryInfo dir = new DirectoryInfo(sFolderPath);
                        if (!dir.Exists)
                            dir.Create();

                        RecipeFront recipe = GlobalObjects.Instance.Get<RecipeFront>();
                        recipe.Read(sFullPath);

                        UpdateUI();
                    }
                });
            }
        }

        public ICommand btnFrontMap
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Map);
                });
            }
        }
        public ICommand btnFrontOrigin
        {
            get
            {
                return new RelayCommand(() =>
                { 
                    p_Origin_VM.SetPage();
                    SetPage(Origin);
                    
                });
            }
        }
        public ICommand btnFrontPosition
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Position);
                });
            }
        }
        public ICommand btnFrontMask
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(ROI);
                });
            }
        }
        public ICommand btnFrontSpec
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Spec);
                });
            }
        }
        public ICommand btnFrontInspTest
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(InspTestPage);
                    m_InspTest_VM.SetPage(InspTestPage);
                });
            }
        }
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.SetRecipeWizard();
                });
            }
        }
    }
    public enum ModifyType
    {
        None,
        LineStart,
        LineEnd,
        ScrollAll,
        Left,
        Right,
        Top,
        Bottom,
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom,
    }
}
