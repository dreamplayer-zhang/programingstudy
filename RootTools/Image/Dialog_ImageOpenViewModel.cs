using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RootTools
{
    public class Dialog_ImageOpenViewModel : ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;


        RootViewer_ViewModel m_RootViewer;
        public RootViewer_ViewModel p_RootViewer
        {
            get
            {
                return m_RootViewer;
            }
            set
            {
                SetProperty(ref m_RootViewer, value);
            }
        }

        public Dialog_ImageOpenViewModel(RootViewer_ViewModel rootviewer)
        {
            p_RootViewer = rootviewer;
        }


        public void OnOkButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
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

    public class LineToStartPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int iValue = 0;
            double fValue = 0;
            if (Int32.TryParse(value.ToString(), out iValue))
            {
                return iValue;
            }
            else if (Double.TryParse(value.ToString(), out fValue))
            {
                return (int)fValue;
            }
            else
                return 0;
        }
    }
}
