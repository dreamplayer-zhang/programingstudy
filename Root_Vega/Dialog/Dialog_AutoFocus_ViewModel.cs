using Root_Vega.Module;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Vega
{
    class Dialog_AutoFocus_ViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
        SideVision m_Vision;
        SideVision.Run_AutoFocus m_RunAutoFocus;

        public Dialog_AutoFocus_ViewModel(SideVision vision, SideVision.Run_AutoFocus af)
        {
            m_Vision = vision;
            m_RunAutoFocus = af;
        }

        public void OnOkButton()
        {
            m_Vision.StartRun(m_RunAutoFocus);
        }

        public void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }

        public RelayCommand OkCommand
        {
            get
            {
                return new RelayCommand(OnOkButton);
            }
        }
        public RelayCommand CancelCommand
        {
            get
            {
                return new RelayCommand(OnCancelButton);
            }
        }
    }
}
