using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_AOP01_Packing
{
    public class Dlg_RunStepViewModel : ObservableObject, IDialogRequestClose
    {
        MainWindow m_MainWindow;
        public AOP01_Engineer m_Engineer;

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public Dlg_RunStepViewModel(MainWindow main, AOP01_Engineer engineer)
        {
            m_MainWindow = main;
            m_Engineer = engineer;
            Init();
        }

        private void Init()
        {
        }

        public ICommand cmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                });
            }
        }
    }
}
