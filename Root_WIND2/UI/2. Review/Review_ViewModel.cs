using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Root_WIND2
{
    class Review_ViewModel : ObservableObject
    {
        MainWindow m_MainWindow;

        public Review_ViewModel()
        {
            init();
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
                    m_MainWindow.MainPanel.Children.Add(m_MainWindow.m_ModeUI);
                });
            }
        }
    }
}
