using Root_WIND2;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
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
        public FrontSideAlignment   Position;
        public FrontSideROI         ROI;
        public FrontSideSpec        Spec;

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
        private FrontsideAlignment_ViewModel m_Alignment_VM;
        public FrontsideAlignment_ViewModel p_Alignment_VM
        {
            get
            {
                return m_Alignment_VM;
            }
            set
            {
                SetProperty(ref m_Alignment_VM, value);
            }
        }
        private FrontsideROI_ViewModel m_ROI_VM;
        public FrontsideROI_ViewModel p_ROI_VM
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



        Setup_ViewModel m_Setup;
        Recipe m_Recipe;
        public Frontside_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Recipe = setup.Recipe;

            p_Origin_VM = new FrontsideOrigin_ViewModel();
            p_Origin_VM.SetOrigin += P_Origin_VM_SetOrigin;
            p_Origin_VM.init(setup, m_Recipe);

            p_Alignment_VM = new FrontsideAlignment_ViewModel();
            p_Alignment_VM.init(setup, m_Recipe);

            p_ROI_VM = new FrontsideROI_ViewModel();
            p_ROI_VM.Init(setup,m_Recipe);

            p_Spec_VM = new FrontsideSpec_ViewModel();
            p_Spec_VM.init(setup, m_Recipe);
            
            Init();

            m_Map_VM = new FrontsideMap_ViewModel();
            m_Map_VM.Init(setup, Map, m_Recipe);
        }

        private void P_Origin_VM_SetOrigin(object e)
        {
            p_ROI_VM.SetOrigin(e);
        }

        public void UI_Redraw()
        {
            p_Map_VM.LoadMapData(); // Map
            m_Origin_VM.LoadOriginData(); // Origin
            p_Alignment_VM.LoadPositonMark(); // Position
        }

        public void Init()
        {
            Main = new FrontSide_Panel();
            Summary = new FrontsideSummary();
            Map = new FrontSideMap();
            Origin = new FrontSideOrigin();
            Position = new FrontSideAlignment();
            ROI = new FrontSideROI();
            Spec = new FrontSideSpec();

            SetPage(Map);
            SetPage(Origin);
            SetPage(Position);
            SetPage(ROI);
            SetPage(Spec);
            SetPage(Summary);

        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnFrontSummary
        {
            get
            {
                return new RelayCommand(() =>
                {
                    SetPage(Summary);
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
                    m_Setup.SetFrontInspTest();
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
