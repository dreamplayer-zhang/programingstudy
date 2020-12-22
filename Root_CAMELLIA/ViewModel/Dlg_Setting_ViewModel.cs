using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class Dlg_Setting_ViewModel : ObservableObject, IDialogRequestClose
    {
        public Dlg_Setting_ViewModel(MainWindow_ViewModel main)
        {
        }
        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CloseRequested(this, new DialogCloseRequestedEventArgs(true));
                });
            }
        }
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }
}
