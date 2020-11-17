using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class EBR_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;

        public EBR_Panel Main;
        public EBR_ViewModel(Setup_ViewModel setup)
        {
            init();
            m_Setup = setup;

        }
        public void init()
        {
            Main = new EBR_Panel();
        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnEBRSummary
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
                return new RelayCommand(() =>
                {
                    m_Setup.SetRecipeWizard();
                });
            }
        }
    }
}
