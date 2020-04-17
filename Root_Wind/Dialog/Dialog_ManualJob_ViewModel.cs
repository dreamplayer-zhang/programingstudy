using Root_Wind;
using RootTools;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;

namespace Root_Wind
{
    class Dialog_ManualJob_ViewModel :ObservableObject, IDialogRequestClose
    {
        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        InfoCarrier m_InfoCarrier;
        public InfoCarrier p_InfoCarrier
        {
            get
            {
                return m_InfoCarrier;
            }
            set
            {
                SetProperty(ref m_InfoCarrier, value);
            }
        }
        InfoWafer m_SelectedInfoWafer;
        public InfoWafer p_SelectedInfoWafer
        {
            get
            {
                return m_SelectedInfoWafer;
            }
            set
            {
                SetProperty(ref m_SelectedInfoWafer, value);
            }
        }
        Wind_Process m_Process;
        public Wind_Process p_Process
        {
            get
            {
                return m_Process;
            }
            set
            {
                SetProperty(ref m_Process, value);
            }
        }

        string m_sRecipePath = @"C:\Recipe\"; // 딴데서 가져와야데
        public string p_sRecipePath
        {
            get
            {
                return m_sRecipePath;
            }
            set
            {
                SetProperty(ref m_sRecipePath, value);
            }
        }
        string m_sSelectedRecipe = "";
        public string p_sSelectedRecipe
        {
            get
            {
                return m_sSelectedRecipe;
            }
            set
            {
                SetProperty(ref m_sSelectedRecipe, value);
            }
        }
        ObservableCollection<string> m_sRecipeName = new ObservableCollection<string>();
        public ObservableCollection<string> p_sRecipeName
        {
            get
            {
                return m_sRecipeName;
            }
            set
            {
                SetProperty(ref m_sRecipeName, value);
            }
        }

        public Dialog_ManualJob_ViewModel(InfoCarrier carrier, Wind_Process process)
        {
            p_InfoCarrier = carrier;
            p_Process = process;
            SetRecipeList();
        }
        

        void SetRecipeList()
        {
            p_sRecipeName.Clear();
            string[] files = Directory.GetFiles(p_sRecipePath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                p_sRecipeName.Add(file.Replace(p_sRecipePath,""));
            }
            RaisePropertyChanged("p_sRecipeName");
        }

        public void OnOkButton()
        {  
            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }

        public void OnCancelButton()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }

        void AddWaferSeqeuence()
        {
            if (p_SelectedInfoWafer != null)
            {
                p_Process.AddInfoWafer(p_SelectedInfoWafer);
                p_Process.ReCalcSequence();
            }
        }

        void SeqeuneceClear()
        {
            p_Process.ClearInfoWafer();
        }

        void SetRecipeInWafer()
        {
            if (p_SelectedInfoWafer != null)
            {
                p_SelectedInfoWafer.RecipeOpen(p_sRecipePath + m_sSelectedRecipe);
            }
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

        public RelayCommand WaferAddCommand
        {
            get
            {
                return new RelayCommand(AddWaferSeqeuence);
            }
        }

        public RelayCommand SeqeuneceClearCommand
        {
            get
            {
                return new RelayCommand(SeqeuneceClear);
            }
        }

        public RelayCommand SetRecipeCommand
        {
            get
            {
                return new RelayCommand(SetRecipeInWafer);
            }
        }
    }
}

namespace Dialog_ManualJobConverter
{
    public class ShortRecipeNameConverter : IValueConverter
    {
        string sPath = @"C:\Recipe\";

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string sValue = value.ToString();
            return sValue.Replace(sPath, "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string sValue = value.ToString();
            return sPath + sValue;
        }
        #endregion
    }

    public class ListBoxontentsHeightConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            ListBox listbox = values[0] as ListBox;
            double height = (listbox.ActualHeight) / (listbox.Items.Count);
            //Subtract 1, otherwise we could overflow to two rows.
            if (listbox.Items.Count == 2)
                return (height <= 1) ? 0 : (height - 2);
            else
                return (height <= 1) ? 0 : (height - 5);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    public class InfoWaferReverseConverter : IValueConverter
    {
        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<InfoWafer> aValue = (ObservableCollection<InfoWafer>)value;
            ObservableCollection<InfoWafer> result = new ObservableCollection<InfoWafer>();

            for (int i = aValue.Count - 1; i >= 0; i--)
            {
                result.Add(aValue[i]);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<InfoWafer> aValue = (ObservableCollection<InfoWafer>)value;
            ObservableCollection<InfoWafer> result = new ObservableCollection<InfoWafer>();

            for (int i = aValue.Count - 1; i >= 0; i--)
            {
                result.Add(aValue[i]);
            }

            return result;
        }
        #endregion
    }
}
