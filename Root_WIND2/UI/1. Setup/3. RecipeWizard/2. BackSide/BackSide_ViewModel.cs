using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
    class BackSide_ViewModel : ObservableObject
    {
        Setup_ViewModel m_Setup;

        public BackSidePanel Main;
        public BackSide_ViewModel(Setup_ViewModel setup)
        {
            init();
            m_Setup = setup;

        }
        public void init()
        {
            Main = new BackSidePanel();
        }
        public void SetPage(UserControl page)
        {
            Main.SubPanel.Children.Clear();
            Main.SubPanel.Children.Add(page);
        }

        public ICommand btnBackSummary
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }
        public ICommand btnBackIllum
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }
        public ICommand btnBackUpload
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }
        public ICommand btnBackMask
        {
            get
            {
                return new RelayCommand(() =>
                {

                });
            }
        }
        public ICommand btnBackSetup
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
