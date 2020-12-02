using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_CAMELLIA
{
    public class Dlg_Engineer_ViewModel : ObservableObject, IDialogRequestClose
    {
        Camellia_Engineer_UI EngineerUI;

        public Dlg_Engineer_ViewModel(MainWindow_ViewModel main)
        {
            EngineerUI = new Camellia_Engineer_UI();
            EngineerUI.Init(App.m_engineer);
        }
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

    }
}
