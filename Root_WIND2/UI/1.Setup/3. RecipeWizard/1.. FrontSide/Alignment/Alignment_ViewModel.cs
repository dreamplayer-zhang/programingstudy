using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class Alignment_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;

        public AlignmentPanel Main;
        public AlignmentSummaryPage Summary;
        public AlignmentOriginPage Origin;
        public AlignmentSetupPage Setup;
        public AlignmentPositionPage Position;
        public AlignmentMapPage Map;

        
        public Alignment_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            init();
        }
        public void init()
        {
            Main = new AlignmentPanel();
            Summary = new AlignmentSummaryPage();
            Setup = new AlignmentSetupPage();
            Origin = new AlignmentOriginPage();
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
