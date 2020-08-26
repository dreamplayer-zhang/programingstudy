using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class General_ViewModel : ObservableObject
    {
        private ObservableCollection<Mask> m_cMask;
        public ObservableCollection<Mask> p_cMask
        {
            get
            {
                return m_cMask;
            }
            set
            {
                SetProperty(ref m_cMask, p_cMask);
            }
        }

        private ObservableCollection<InspectionMethod> m_cInspMethod;
        public ObservableCollection<InspectionMethod> p_cInspMethod
        {
            get
            {

                return m_cInspMethod;
            }
            set
            {
                SetProperty(ref m_cInspMethod, value);
            }
        }

        private ObservableCollection<InspectionItem> m_cInspItem;
        public ObservableCollection<InspectionItem> p_cInspItem
        {
            get
            {
                return m_cInspItem;
            }
            set
            {
                SetProperty(ref m_cInspItem, value);
            }
        }

        private Mask m_selectedMask;
        public Mask p_selectedMask
        {
            get
            {
                return m_selectedMask;
            }
            set
            {
                if (p_selectedInspItem == null)
                    return;
                m_selectedInspItem.p_Mask = value;
                SetProperty(ref m_selectedMask, value);
            }
        }

        private InspectionMethod m_selectedMethod;
        public InspectionMethod p_selectedMethod
        {
            get
            {
                return m_selectedMethod;
            }
            set
            {
                SetProperty(ref m_selectedMethod, value);
            }
        }

        private InspectionItem m_selectedInspItem;
        public InspectionItem p_selectedInspItem
        {
            get
            {
                return m_selectedInspItem;
            }
            set
            {
                SetProperty(ref m_selectedInspItem, value);
            }
        }


        Setup_ViewModel m_Navigation;

        public GeneralPanel Main;
        public GeneralSummaryPage Summary;
        public GeneralMaskPage Mask;
        public GeneralSetupPage Setup;

        public General_ViewModel(Setup_ViewModel navi)
        {
            m_Navigation = navi;
            init();
        }

        public void init()
        {
            m_cMask = new ObservableCollection<Mask>();
            m_cInspMethod = new ObservableCollection<InspectionMethod>();
            m_cInspItem = new ObservableCollection<InspectionItem>();

            Main = new GeneralPanel();
            Summary = new GeneralSummaryPage();
            Mask = new GeneralMaskPage();
            Setup = new GeneralSetupPage();

            SetPage(Summary);

            TestMask();
        }
        void TestMask()
        {
            Mask mask1 = new Mask();
            mask1.p_sName = "Mask1";
            mask1.p_Color = Color.Red;
            mask1.p_dHeight = 200;
            mask1.p_dWidth = 200;
            mask1.p_PtMask = new System.Windows.Point(30, 30);
            Mask mask2 = new Mask();
            mask2.p_sName = "Mask2";
            mask2.p_Color = Color.Blue;
            mask2.p_dHeight = 400;
            mask2.p_dWidth = 400;
            mask2.p_PtMask = new System.Windows.Point(50, 50);
            Mask mask3 = new Mask();
            mask3.p_sName = "Mask3";
            mask3.p_Color = Color.Green;
            mask3.p_dHeight = 800;
            mask3.p_dWidth = 800;
            mask3.p_PtMask = new System.Windows.Point(100, 100);

            p_cMask.Add(mask1);
            p_cMask.Add(mask2);
            p_cMask.Add(mask3);
        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }
        private void InspMethodRefresh()
        {
            ObservableCollection<InspectionMethod> temp = new ObservableCollection<InspectionMethod>();
            foreach (InspectionMethod item in p_cInspMethod)
            {
                temp.Add(item);
            }
            p_cInspMethod.Clear();
            p_cInspMethod = temp;
        }
        private void InspItemRefresh()
        {
            ObservableCollection<InspectionItem> temp = new ObservableCollection<InspectionItem>();
            foreach (InspectionItem item in p_cInspItem)
            {
                temp.Add(item);
            }
            p_cInspItem.Clear();
            p_cInspItem = temp;
        }
        private void Item_changeMode(object e)
        {
            p_selectedMethod = null;
            p_selectedMethod = e as InspectionMethod;
        }

        public ICommand btnGeneralSummary
        {
            get
            {
                return new RelayCommand(m_Navigation.SetFrontGeneral);
            }
        }
        public ICommand btnGeneralMask
        {
            get
            {
                return new RelayCommand(m_Navigation.SetFrontGeneralMask);
            }
        }
        public ICommand btnGeneralSetup
        {
            get
            {
                return new RelayCommand(m_Navigation.SetFrontGeneralSetup);
            }
        }
        public ICommand btnAddInspMethod
        {
            get
            {
                return new RelayCommand(() =>
                {
                    InspectionMethod method = new InspectionMethod();
                    method.p_sName = "Defalut";
                    method.changeMode += Item_changeMode;
                    p_cInspMethod.Add(method);
                    p_selectedMethod = p_cInspMethod.Last();
                });
            }
        }
        public ICommand btnDeleteInspMethod
        {
            get
            {
                return new RelayCommand(() =>
                {
                    p_cInspMethod.Remove(p_selectedMethod);
                    p_selectedMethod = p_cInspMethod.Last();
                    //InspMethodRefresh();
                });
            }
        }
        public ICommand btnAddInspItem
        {
            get
            {
                return new RelayCommand(() =>
                {
                    InspectionItem item = new InspectionItem();
                    item.p_cMask = p_cMask;
                    item.p_cInspMethod = p_cInspMethod;
                    p_cInspItem.Add(item);
                });
            }
        }
        public ICommand btnDeleteInspItem
        {
            get
            {
                return new RelayCommand(() =>
        {
            p_cInspItem.Remove(p_selectedInspItem);
            //InspItemRefresh();
        });
            }
        }
        public ICommand btnBack
        {
            get
            {

                return new RelayCommand(m_Navigation.SetWizardFrontSide);
            }
        }

    }
}
