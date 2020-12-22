using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2
{
    class Inspection_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;
        public InspectionPanel Main;

        public Inspection_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            init();
        }
        private void init()
        {
            Main = new InspectionPanel();
        }

        public ICommand btnInspStart
        {
            get
            {
                return new RelayCommand(() =>
                {

                    //m_Setup.InspectionVision.Start();
                });
            }
        }
        public ICommand btnInspLoad
        {
            get
            {
                return new RelayCommand(() =>
                {
                });
            }
        }
        public ICommand btnInspSnap
        {
            get
            {
                return new RelayCommand(() =>
                {
                });
            }
        }
        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(m_Setup.SetHome);
            }
        }
    }
}
