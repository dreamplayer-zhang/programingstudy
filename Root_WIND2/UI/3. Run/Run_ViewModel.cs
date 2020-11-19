using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2
{
    class Run_ViewModel : ObservableObject
    {
        MainWindow m_MainWindow;

        public Run_ViewModel(MainWindow main)
        {
            init(main);
        }
        public void init(MainWindow main = null)
        {
            m_MainWindow = main;
        }
        public ICommand btnMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    m_MainWindow.MainPanel.Children.Clear();
                    m_MainWindow.MainPanel.Children.Add(m_MainWindow.ModeUI);
                });
            }
        }
    }
}
