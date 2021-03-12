using System.Windows.Controls;
using System.Windows.Input;

namespace Root_AOP01_Inspection
{
    public class RecipeWizard_ViewModel : ObservableObject
    {
        public RecipeWizard_Panel RecipeWizard = new RecipeWizard_Panel();

        public RecipeSummary_Page RecipeSummary = new RecipeSummary_Page();
        public RecipeSpec_Page RecipeSpec = new RecipeSpec_Page();

        public Recipe45D_Panel Recipe45D = new Recipe45D_Panel();
        public RecipeFrontside_Panel RecipeFrontside = new RecipeFrontside_Panel();
        public RecipeEdge_Panel RecipeEdge = new RecipeEdge_Panel();
        public RecipeLADS_Panel RecipeLADS = new RecipeLADS_Panel();

        Setup_ViewModel m_Setup;
        public RecipeWizard_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
        }

        #region Property
        bool m_bUseEdgeBroken = false;
        public bool p_bUseEdgeBroken
        {
            get { return m_bUseEdgeBroken; }
            set { SetProperty(ref m_bUseEdgeBroken, value); }
        }

        double m_dEdgeBrokenSpec_mm = 0.0;
        public double p_dEdgeBrokenSpec_mm
        {
            get { return m_dEdgeBrokenSpec_mm; }
            set { SetProperty(ref m_dEdgeBrokenSpec_mm, value); }
        }

        bool m_bUsePatternArrayShiftAndRotation = false;
        public bool p_bUsePatternArrayShiftAndRotation
        {
            get { return m_bUsePatternArrayShiftAndRotation; }
            set { SetProperty(ref m_bUsePatternArrayShiftAndRotation, value); }
        }

        double m_dPatternArrayShiftSpec_mm = 0.0;
        public double p_dPatternArrayShiftSpec_mm
        {
            get { return m_dPatternArrayShiftSpec_mm; }
            set { SetProperty(ref m_dPatternArrayShiftSpec_mm, value); }
        }

        double m_dPatternArrayRotationSpec_degree = 0.0;
        public double p_dPatternArrayRotationSpec_degree
        {
            get { return m_dPatternArrayRotationSpec_degree; }
            set { SetProperty(ref m_dPatternArrayRotationSpec_degree, value); }
        }

        bool m_bUsePatternDiscolor = false;
        public bool p_bUsePatternDiscolor
        {
            get { return m_bUsePatternDiscolor; }
            set { SetProperty(ref m_bUsePatternDiscolor, value); }
        }

        double m_dPatternDiscolorSpec_mm = 0.0;
        public double p_dPatternDiscolorSpec_mm
        {
            get { return m_dPatternDiscolorSpec_mm; }
            set { SetProperty(ref m_dPatternDiscolorSpec_mm, value); }
        }

        bool m_bUseBarcodeScratch = false;
        public bool p_bUseBarcodeScratch
        {
            get { return m_bUseBarcodeScratch; }
            set { SetProperty(ref m_bUseBarcodeScratch, value); }
        }

        double m_dBarcodeScratchSpec_mm = 0.0;
        public double p_dBarcodeScratchSpec_mm
        {
            get { return m_dBarcodeScratchSpec_mm; }
            set { SetProperty(ref m_dBarcodeScratchSpec_mm, value); }
        }

        bool m_bUseAlignKeyExist = false;
        public bool p_bUseAlignKeyExist
        {
            get { return m_bUseAlignKeyExist; }
            set { SetProperty(ref m_bUseAlignKeyExist, value); }
        }

        double m_dAlignKeyExistSpec_um = 0.0;
        public double p_dAlignKeyExistSpec_um
        {
            get { return m_dAlignKeyExistSpec_um; }
            set { SetProperty(ref m_dAlignKeyExistSpec_um, value); }
        }

        bool m_bUsePellicleShiftAndRotation = false;
        public bool p_bUsePellicleShiftAndRotation
        {
            get { return m_bUsePellicleShiftAndRotation; }
            set { SetProperty(ref m_bUsePellicleShiftAndRotation, value); }
        }

        double m_dPellicleShiftSpec_mm = 0.0;
        public double p_dPellicleShiftSpec_mm
        {
            get { return m_dPellicleShiftSpec_mm; }
            set { SetProperty(ref m_dPellicleShiftSpec_mm, value); }
        }

        double m_dPellicleRotationSpec_degree = 0.0;
        public double p_dPellicleRotationSpec_degree
        {
            get { return m_dPellicleRotationSpec_degree; }
            set { SetProperty(ref m_dPellicleRotationSpec_degree, value); }
        }

        bool m_bUsePellicleHaze = false;
        public bool p_bUsePellicleHaze
        {
            get { return m_bUsePellicleHaze; }
            set { SetProperty(ref m_bUsePellicleHaze, value); }
        }

        double m_dPellicleHazeSpec_mm = 0.0;
        public double p_dPellicleHazeSpec_mm
        {
            get { return m_dPellicleHazeSpec_mm; }
            set { SetProperty(ref m_dPellicleHazeSpec_mm, value); }
        }

        bool m_bUsePellicleExpanding = false;
        public bool p_bUsePellicleExpanding
        {
            get { return m_bUsePellicleExpanding; }
            set { SetProperty(ref m_bUsePellicleExpanding, value); }
        }

        double m_dPellicleExpandingSpec_um = 0.0;
        public double p_dPellicleExpandingSpec_um
        {
            get { return m_dPellicleExpandingSpec_um; }
            set { SetProperty(ref m_dPellicleExpandingSpec_um, value); }
        }

        bool m_bUsePellicleFrontside = false;
        public bool p_bUsePellicleFrontside
        {
            get { return m_bUsePellicleFrontside; }
            set { SetProperty(ref m_bUsePellicleFrontside, value); }
        }

        double m_dPellicleFrontsideSpec_um = 0.0;
        public double p_dPellicleFrontsideSpec_um
        {
            get { return m_dPellicleFrontsideSpec_um; }
            set { SetProperty(ref m_dPellicleFrontsideSpec_um, value); }
        }

        #endregion

        public ICommand btnSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    bool check = (bool)RecipeWizard.btnSummary.IsChecked;
                    if (check)
                    {
                        RecipeWizard.btnSpec.IsChecked = false;
                        RecipeWizard.btnSummary.IsChecked = true;
                        m_Setup.Set_RecipeSummary();
                    }
                });
            }
        }
        public ICommand btnRecipeSpec
        {
            get
            {
                return new RelayCommand(() =>
                {
                    bool check = (bool)RecipeWizard.btnSpec.IsChecked;
                    if (check)
                    {
                        RecipeWizard.btnSummary.IsChecked = false;
                        RecipeWizard.btnSpec.IsChecked = true;
                        m_Setup.Set_RecipeSpec();
                    }
                });
            }
        }
        public ICommand btn45D
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_Recipe45DPanel();
                });
            }
        }
        public ICommand btnFrontside
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeFrontsidePanel();
                });
            }
        }
        public ICommand btnEdge
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeEdgePanel();
                });
            }
        }
        public ICommand btnLADS
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_RecipeLADSPanel();
                });
            }
        }
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_HomePanel();
                });
            }
        }
        //public ICommand btnRun
        //{
        //    get
        //    {
        //        return new RelayCommand(() =>
        //        {
        //            m_Setup.m_MainWindow.MainPanel.Children.Clear();
        //            m_Setup.m_MainWindow.MainPanel.Children.Add(m_Setup.m_MainWindow.Run);
        //        });
        //    }
        //}
    }
}
