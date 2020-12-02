using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA
{
    public class Dlg_PM_ViewModel : ObservableObject, IDialogRequestClose
    {
        public Dlg_PM_ViewModel(MainWindow_ViewModel main)
        {
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }
}
