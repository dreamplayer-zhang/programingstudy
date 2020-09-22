using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RootTools;
using RootTools_Vision;

namespace Root_WIND2
{
    class Alignment_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;

        #region ViewModel
        private Origin_ViewModel m_Origin_VM;
        public Origin_ViewModel p_Origin_VM
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

        private Position_ViewModel m_Position_VM;
        public Position_ViewModel p_Position_VM
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
        #endregion

        #region UI
        public AlignmentPanel Main;
        public AlignmentSummaryPage Summary;
        public AlignmentOriginPage Origin;
        public AlignmentSetupPage Setup;
        public AlignmentPositionPage Position;
        public AlignmentMapPage Map;
        #endregion

        public Recipe m_Recipe;
    
        public Alignment_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Recipe = m_Setup.m_MainWindow.m_RecipeMGR.GetRecipe();


            init();
            p_Origin_VM = new Origin_ViewModel();
            p_Origin_VM.init(setup, m_Recipe);
            p_Position_VM = new Position_ViewModel();
            p_Position_VM.init(setup, m_Recipe);

        }

        public void init()
        {        
            Main = new AlignmentPanel();
            Summary = new AlignmentSummaryPage();
            Setup = new AlignmentSetupPage();
            Origin = new AlignmentOriginPage();
            p_Origin_VM = new Origin_ViewModel();
            Position = new AlignmentPositionPage();
            Map = new AlignmentMapPage();
            SetPage(Summary);

        }

        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnAlignSummary
        {
            get
            {
                return new RelayCommand(m_Setup.SetFrontAlignment);
            }
        }
        public ICommand btnAlignSetup
        {
            get
            {
                return new RelayCommand(m_Setup.SetFrontAlignSetup);
            }
        }
        public ICommand btnAlignOrigin
        {
            get
            {
                return new RelayCommand(m_Setup.SetFrontAlignOrigin);
            }
        }
        public ICommand btnAlignMap
        {
            get
            {
                return new RelayCommand(m_Setup.SetFrontAlignMap);
            }
        }
        public ICommand btnAlignPosition
        {
            get
            {
                return new RelayCommand(m_Setup.SetFrontAlignPosition);
            }
        }

        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(m_Setup.SetWizardFrontSide);
            }
        }
    }
}
