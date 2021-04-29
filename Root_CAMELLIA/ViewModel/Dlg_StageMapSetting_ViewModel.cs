using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_CAMELLIA
{
    public class Dlg_StageMapSetting_ViewModel : ObservableObject, IDialogRequestClose
    {
        private MainWindow_ViewModel mainWindow_ViewModel;

        public Dlg_StageMapSetting_ViewModel()
        {
            
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public ICommand CmdClose
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //CloseRequested(this, new DialogCloseRequestedEventArgs(false));
                });
            }
        }

    }
}
