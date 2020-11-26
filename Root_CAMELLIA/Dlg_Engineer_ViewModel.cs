using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA
{
    class Dlg_Engineer_ViewModel : ObservableObject, IDialogRequestClose
    {
        public Dlg_Engineer_ViewModel(MainWindow main)
        {
            CAMELLIA_Engineer engineer = new CAMELLIA_Engineer();
        }
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }
}
