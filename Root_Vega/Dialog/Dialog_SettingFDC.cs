using RootTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;
using System.Globalization;


namespace Root_Vega
{
    public class TK4S_UnitToIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int indexValue = (int)value;

            return indexValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int indexValue = (int)value;
            FDC_Unit result;
            result = (FDC_Unit)Enum.ToObject(typeof(FDC_Unit), indexValue);

            return result;
        }
    }


    class Dialog_SettingFDC_ViewModel : ObservableObject, IDialogRequestClose
    {
        #region XamlProperty
        private int m_InputNumber;
        public int p_InputNumber
        {
            get { return m_InputNumber; }
            set {
				if(value>=0)
					SetProperty(ref m_InputNumber, value); }
        }
        private String m_InputName;
        public String p_InputName
        {
            get { return m_InputName; }
            set { SetProperty(ref m_InputName, value); }
        }
        private FDC_Unit m_InputUnit;
        public FDC_Unit p_InputUnit
        {
            get { return m_InputUnit; }
            set { SetProperty(ref m_InputUnit, value); }
        }
        private ObservableCollection<String> _UnitCollection;
        public ObservableCollection<String> p_UnitCollection
        {
            get { return _UnitCollection; }
            set { SetProperty(ref _UnitCollection, value); }
        }

		private double m_LowerValue;
		public double p_LowerValue
		{
			get { return m_LowerValue; }
			set { SetProperty(ref m_LowerValue, value); }
		}

		private double m_UpperValue;
		public double p_UpperValue
		{
			get { return m_UpperValue; }
			set { SetProperty(ref m_UpperValue, value); }
		}

		private double m_test;
		public double p_test
		{
			get { return m_test; }
			set { SetProperty(ref m_test, value); }
		}
      

        ObservableCollection<FDC_Control> item;

        #endregion

        CVM_Manager CVM_manager;
        bool newbi;
        FDC_ControlViewModel selectedViewModel;
        public Dialog_SettingFDC_ViewModel()
        {
            p_InputNumber = 0;
            p_InputName = "";
            p_InputUnit = FDC_Unit.None;
        }

        //ADD버튼 클릭시 다이얼로그 생성.
		public Dialog_SettingFDC_ViewModel(Setting_FDCViewModel _owner)
        {
            newbi = true;
            p_UnitCollection = new ObservableCollection<string>();
            foreach (FDC_Unit EachEnum in Enum.GetValues(typeof(FDC_Unit)))
            {
                Type type = EachEnum.GetType();
                MemberInfo[] memInfo = type.GetMember(EachEnum.ToString());
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                p_UnitCollection.Add(((DescriptionAttribute)attrs[0]).Description);
            }

            item = _owner.p_Item;
            CVM_manager = _owner.m_CVM_manager;
            p_InputNumber = 0;
            p_InputName = "";
            p_InputUnit = FDC_Unit.None;
			p_LowerValue = 0;
			p_UpperValue = 100;
        }

        //tk4s더블클릭시 다이얼로그 생성.
		public Dialog_SettingFDC_ViewModel(FDC_ControlViewModel TK4SCntrl, CVM_Manager _manager)
        {
            newbi = false;
            p_UnitCollection = new ObservableCollection<string>();

            foreach (FDC_Unit EachEnum in Enum.GetValues(typeof(FDC_Unit)))
            {
                Type type = EachEnum.GetType();
                MemberInfo[] memInfo = type.GetMember(EachEnum.ToString());
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                p_UnitCollection.Add(((DescriptionAttribute)attrs[0]).Description);
            }

         
            selectedViewModel = TK4SCntrl;
            p_InputNumber = selectedViewModel.p_SlaveNumber;
            p_InputName = selectedViewModel.p_TmprtrName;
            p_InputUnit = selectedViewModel.p_EnumSelectedUnit;
            CVM_manager = _manager;
			p_LowerValue = selectedViewModel.p_LowerValue;
			p_UpperValue = selectedViewModel.p_UpperValue;
			p_test = selectedViewModel.p_CurrentValue;
		}

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;


		public ICommand Minus_InputNumber
		{
			get
			{
				return new RelayCommand(Minus_InputNumberFunction);
			}
		}
		public void Minus_InputNumberFunction()
		{
			p_InputNumber--;
		}
		public ICommand Plus_InputNumber
		{
			get
			{
				return new RelayCommand(Plus_InputNumberFunction);
			}
		}
		public void Plus_InputNumberFunction()
		{
			p_InputNumber++;
		}
        public ICommand OkCommand
        {
            get
            {
                return new RelayCommand(OkCommandFunction);
            }
        }
        public void OkCommandFunction()
        {
            if (newbi == true)
            {
                CVM_manager.Add_CVM();
                selectedViewModel = CVM_manager.GetLastCVM();

				FDC_Control Cxaml1 = new FDC_Control(selectedViewModel);
                item.Add(Cxaml1);
            }
            selectedViewModel.p_SlaveNumber = p_InputNumber;
            selectedViewModel.p_TmprtrName = p_InputName;
            selectedViewModel.p_EnumSelectedUnit = p_InputUnit;
			selectedViewModel.p_LowerValue = p_LowerValue;
			selectedViewModel.p_UpperValue = p_UpperValue;
			 selectedViewModel.p_CurrentValue = p_test ;


            CVM_manager.ModifyRegistry();

            CloseRequested(this, new DialogCloseRequestedEventArgs(true));
        }
        public ICommand CancelCommand
        {
            get
            {
                return new RelayCommand(CancelCommandFunction);
            }
        }
        public void CancelCommandFunction()
        {
            CloseRequested(this, new DialogCloseRequestedEventArgs(false));
        }
    }
}
