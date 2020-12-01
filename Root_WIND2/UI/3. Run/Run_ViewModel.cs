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

        public Run_ViewModel()
        {
            init();
        }
        public void init()
        {

        }
        public ICommand btnMode
        {
            get
            {
                return new RelayCommand(() =>
                {
                    UIManager.Instance.ChangUIMode();
                });
            }
        }
    }
}
