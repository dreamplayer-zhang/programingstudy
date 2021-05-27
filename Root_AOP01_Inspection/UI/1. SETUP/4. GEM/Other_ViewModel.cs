using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Root_AOP01_Inspection
{
    class Other_ViewModel
    {        
        Setup_ViewModel m_Setup;
        AOP01_Engineer m_Engineer;
        public Other_Panel Other = new Other_Panel();

        public Other_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            m_Engineer = GlobalObjects.Instance.Get<AOP01_Engineer>();
            Other.Init(m_Engineer, m_Engineer.m_handler.m_aLoadport[0]);
        }

       

        public ICommand btnBack
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_Setup.Set_GEMPanel();
                });
            }
        }
    }
}