using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using System.Windows.Input;
using System.Windows.Data;
using System.IO.Ports;
using System.Globalization;
using System.ComponentModel;
using System.Linq.Expressions;
using RootTools.Trees;


namespace Root_Vega
{
	public enum Baudrate
	{
		[Description("115200")]
		_115200,
		[Description("19200")]
		_19200,
		[Description("38400")]
		_38400,
		[Description("57600")]
		_57600,
		[Description("9600")]
		_9600
	}
	public enum DataBits
	{
		[Description("8")]
		_8,
		[Description("7")]
		_7,
		[Description("6")]
		_6,
	}

	public class BoolToOpacity : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			bool InputBool = (bool)value;
			double result;
			if (InputBool)
				result = 1;
			else
				result = 0.5;

			return result;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class ReverseBoolToOpacity : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			bool InputBool = !(bool)value;
			double result;
			if (InputBool)
				result = 1;
			else
				result = 0.5;

			return result;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class TextConvert : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			bool InputBool = !(bool)value;
			string result;
			if (InputBool)
				result = "Delete";
			else
				result = "Confirm";

			return result;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

	public class ReverseBool : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{

			bool result = (bool)value;

			return !result;
		}
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}


	class Setting_FDCViewModel : ObservableObject
	{
		#region XamlProperty
		private int m_IndexPort;
		public int p_IndexPort
		{
			get { return m_IndexPort; }
			set
			{
				p_bConnectState = false;
				//m_SerialComm.UpdateStop();
				SetProperty(ref m_IndexPort, value);
				m_FDC.m_modbus.m_client.Port = m_IndexPort;
				m_FDC.m_modbus.RunTree(Tree.eMode.Update);
				m_FDC.m_modbus.RunTree(Tree.eMode.RegWrite);
				//m_FDC.m_rs232.RunTree(Tree.eMode.Update);
				//m_FDC.m_rs232.RunTree(Tree.eMode.RegWrite);
			}
		}
		private int m_IndexBaudrate;
		public int p_IndexBaudrate
		{
			get { return m_IndexBaudrate; }
			set
			{
				p_bConnectState = false;
				//m_SerialComm.UpdateStop();
				SetProperty(ref m_IndexBaudrate, value);
				m_FDC.m_modbus.m_client.Baudrate= m_IndexBaudrate;
				m_FDC.m_modbus.RunTree(Tree.eMode.Update);
				m_FDC.m_modbus.RunTree(Tree.eMode.RegWrite);
				//m_FDC.m_rs232.RunTree(Tree.eMode.Update);
				//m_FDC.m_rs232.RunTree(Tree.eMode.RegWrite);
			}
		}
		private int m_IndexParity;
		public int p_IndexParity
		{
			get {
				return m_IndexParity;
			
			}
			set
			{
				p_bConnectState = false;
				SetProperty(ref m_IndexParity, value);
				m_FDC.m_modbus.m_client.Parity = (Parity)m_IndexParity;
				m_FDC.m_modbus.RunTree(Tree.eMode.Update);
				m_FDC.m_modbus.RunTree(Tree.eMode.RegWrite);
				//m_FDC.m_rs232.RunTree(Tree.eMode.Update);
				//m_FDC.m_rs232.RunTree(Tree.eMode.RegWrite);
			}
		}
		//private int m_IndexDataBits;
		//public int p_IndexDataBits
		//{
		//	get {
		//		return m_IndexDataBits; 
		//	}
		//	set
		//	{
		//		p_bConnectState = false;
		//		//m_SerialComm.UpdateStop();
		//		SetProperty(ref m_IndexDataBits, value);
		//		//m_FDC.m_rs232.m_nDataBit = m_IndexDataBits;
		//		//m_FDC.m_rs232.RunTree(Tree.eMode.Update);
		//		//m_FDC.m_rs232.RunTree(Tree.eMode.RegWrite);
		//	}
		//}
		private int m_IndexStopBits;
		public int p_IndexStopBits
		{
			get { return m_IndexStopBits; }
			set
			{
				p_bConnectState = false;
				//m_SerialComm.UpdateStop();
				SetProperty(ref m_IndexStopBits, value);
				m_FDC.m_modbus.RunTree(Tree.eMode.Update);
				m_FDC.m_modbus.RunTree(Tree.eMode.RegWrite);
				//m_FDC.m_rs232.RunTree(Tree.eMode.Update);
				//m_FDC.m_rs232.RunTree(Tree.eMode.RegWrite);
			}
		}

		private bool m_bConnectState = false;
		public bool p_bConnectState
		{
			get { return m_bConnectState; }
			set {
				SetProperty(ref m_bConnectState, value);
			}
		}
		private bool m_bDeleteState = false;
		public bool p_bDeleteState
		{
			get { return m_bDeleteState; }
			set {
				SetProperty(ref m_bDeleteState, value); 
			}
		}
		#endregion

		#region CommboboxItemSorce
		private ObservableCollection<FDC_Control> m_Item;
		public ObservableCollection<FDC_Control> p_Item
		{
			get { return m_Item; }
			set {
				SetProperty(ref m_Item, value);
				}
		}
		private ObservableCollection<string> m_PortCollection = new ObservableCollection<string>();
		public ObservableCollection<string> p_PortCollection
		{
			get { return m_PortCollection; }
			set { 
				SetProperty(ref m_PortCollection, value); 
			}
		}
		private ObservableCollection<string> m_BaudrateCollection = new ObservableCollection<string>();
		public ObservableCollection<string> p_BaudrateCollection
		{
			get { return m_BaudrateCollection; }
			set {
				SetProperty(ref m_BaudrateCollection, value); 
				
			}
		}
		private ObservableCollection<Parity> m_ParityCollection = new ObservableCollection<Parity>();
		public ObservableCollection<Parity> p_ParityCollection
		{
			get { return m_ParityCollection; }
			set {
				SetProperty(ref m_ParityCollection, value); 
			}
		}
		//private ObservableCollection<string> m_DataBitsCollection = new ObservableCollection<string>();
		//public ObservableCollection<string> p_DataBitsCollection
		//{
		//	get { return m_DataBitsCollection; }
		//	set {
		//		SetProperty(ref m_DataBitsCollection, value); 
		//	}
		//}
		private ObservableCollection<StopBits> m_StopBitsCollection = new ObservableCollection<StopBits>();
		public ObservableCollection<StopBits> p_StopBitsCollection
		{
			get { return m_StopBitsCollection; }
			set {
				SetProperty(ref m_StopBitsCollection, value); 
			}
		}
		public Root_Vega.Module.FDC m_FDC;
		#endregion

		Vega_Engineer m_Engineer;
		private readonly IDialogService m_DialogService;
		public CVM_Manager m_CVM_manager;
		Registry m_reg = new Registry("SerialComm");

		public Setting_FDCViewModel(Vega_Engineer engineer, IDialogService DialogService)
		{
			m_Engineer = engineer;
			m_FDC = m_Engineer.m_handler.m_FDC;
			m_FDC.RunTree(Tree.eMode.RegRead);
			m_FDC.RunTree(Tree.eMode.Init);

			//m_Engineer.m_handler.m_FDC.m_aData
			m_DialogService = DialogService;
			m_CVM_manager = new CVM_Manager(m_DialogService);
			
			m_CVM_manager.m_FDC = m_FDC;
			p_Item = new ObservableCollection<FDC_Control>();

			
		}

		#region ICommand
		public ICommand ClickConnectCommand
		{
			get
			{
				return new RelayCommand(ClickConnect_Function);
			}
		}
		public ICommand ClickDisconnectCommand
		{
			get
			{
				return new RelayCommand(ClickDisonnect_Function);
			}
		}
		public ICommand ClickSelectCommand
		{
			get
			{
				return new RelayCommand(ClickSelect_Function);
			}
		}
		public ICommand ClickDeleteCommand
		{
			get
			{
				return new RelayCommand(ClickDelete_Function);
			}
		}
		public ICommand LoadedCommand
		{
			get
			{
				return new RelayCommand(Loaded_Function);
			}
		}
		public ICommand BtnAdd
		{
			get
			{
				return new RelayCommand(BtnAddClick_Function);
			}
		}
		#endregion


		void ClickConnect_Function()
		{
			if (p_PortCollection.Count != 0)
			{
				//m_CVM_manager.m_FDC.m_rs232.p_bConnect = true;// 커넥트 연결
				m_CVM_manager.m_FDC.m_modbus.p_bConnect = true;
				p_bConnectState = true;//커넥트 ui 비활성화
				


				//m_SerialComm.SetPort(p_PortCollection[p_IndexPort],
				//	Convert.ToInt32(p_BaudrateCollection[p_IndexBaudrate]),
				//	p_ParityCollection[p_IndexParity],
				//	Convert.ToInt32(p_DataBitsCollection[p_IndexDataBits]),
				//	p_StopBitsCollection[p_IndexStopBits],
				//   p_HandshakeCollection[p_IndexHandshake]);
				//m_SerialComm.slaveList = m_CVM_manager.SlaveList();
				//m_SerialComm.UpdateStart();

				//m_reg.Write(Member.GetName(() => p_IndexPort), p_IndexPort);
				//m_reg.Write(Member.GetName(() => p_IndexBaudrate), p_IndexBaudrate);
				//m_reg.Write(Member.GetName(() => p_IndexParity), p_IndexParity);
				//m_reg.Write(Member.GetName(() => p_IndexDataBits), p_IndexDataBits);
				//m_reg.Write(Member.GetName(() => p_IndexStopBits), p_IndexStopBits);
				//m_reg.Write(Member.GetName(() => p_IndexHandshake), p_IndexHandshake);
			}
			else
			{
				System.Windows.MessageBox.Show("Please select port");
			}
		}

		void ClickDisonnect_Function()
		{
			p_bConnectState = false;
			m_CVM_manager.m_FDC.m_modbus.p_bConnect = false;
			//m_CVM_manager.m_FDC.m_rs232.p_bConnect = false;
			//m_SerialComm.UpdateStop();
		}
		void ClickSelect_Function()
		{

			p_bDeleteState = !p_bDeleteState;
			m_CVM_manager.bDeleteState = p_bDeleteState;
			if (p_bDeleteState == false)
			{
				foreach (FDC_ControlViewModel _Control in m_CVM_manager.m_CVMDeleteList)
				{
					_Control.p_Selected = System.Windows.Visibility.Hidden;
				}
				m_CVM_manager.m_CVMDeleteList.Clear();
			}
			//m_SerialComm.UpdateStop();
		}
		void ClickDelete_Function()
		{
			p_bDeleteState = false;
			m_CVM_manager.bDeleteState = p_bDeleteState;
			m_FDC.m_aData = new ObservableCollection<Root_Vega.Module.FDC.Data>();
			foreach (FDC_ControlViewModel _Control in m_CVM_manager.m_CVMDeleteList)
				{
				 int _deleteIndex = m_CVM_manager.Remove(_Control);
				 _Control.DeleteDelegate(_deleteIndex);
				}
				m_CVM_manager.m_CVMDeleteList.Clear();

				
			//m_SerialComm.UpdateStop();


		}
		//page가 로드 되면 포트 추가.
		void Loaded_Function()
		{
			string[] ports = SerialPort.GetPortNames();
			p_PortCollection.Clear();

			int m_IndexBaudrate = 0;
			int m_IndexParity = 0;
			//int m_IndexDataBits = 0;
			int m_IndexStopBits = 0;

			foreach (string port in ports)
			{
				p_PortCollection.Add(port);
			}
			p_BaudrateCollection.Clear();
			foreach (Baudrate EachEnum in Enum.GetValues(typeof(Baudrate)))
			{
				Type type = EachEnum.GetType();
				System.Reflection.MemberInfo[] memInfo = type.GetMember(EachEnum.ToString());
				object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
				p_BaudrateCollection.Add(((DescriptionAttribute)attrs[0]).Description);
				if (((DescriptionAttribute)attrs[0]).Description == m_FDC.m_modbus.m_client.Baudrate.ToString())
				{
					p_IndexBaudrate = m_IndexBaudrate;
				}
					m_IndexBaudrate++;
			}
			p_ParityCollection.Clear();
			foreach (Parity EachEnum in Enum.GetValues(typeof(Parity)))
			{
				p_ParityCollection.Add(EachEnum);
				if (EachEnum == m_FDC.m_modbus.m_client.Parity)
				{
					p_IndexParity = m_IndexParity;
				}
				m_IndexParity++;
			}
			//p_DataBitsCollection.Clear();
			//foreach (DataBits EachEnum in Enum.GetValues(typeof(DataBits)))
			//{
			//	Type type = EachEnum.GetType();
			//	System.Reflection.MemberInfo[] memInfo = type.GetMember(EachEnum.ToString());
			//	object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
			//	p_DataBitsCollection.Add(((DescriptionAttribute)attrs[0]).Description);
			//	if (((DescriptionAttribute)attrs[0]).Description == m_FDC.m_m.ToString())
			//	{
			//		p_IndexDataBits = m_IndexDataBits;
			//	}
			//	m_IndexDataBits++;
			//}
			//p_StopBitsCollection.Clear();
			foreach (StopBits EachEnum in Enum.GetValues(typeof(StopBits)))
			{
				p_StopBitsCollection.Add(EachEnum);
				if (EachEnum == m_FDC.m_modbus.m_client.StopBits)
				{
					p_IndexStopBits = m_IndexStopBits;
				}
				m_IndexStopBits++;
			}

			p_IndexPort = m_reg.Read(Member.GetName(() => p_IndexPort), (int)0);
			

			//p_IndexBaudrate = (int)Enum.ToObject(typeof(Baudrate), m_FDC.m_rs232.m_nBaudrate);
			//p_IndexBaudrate = (int)Baudratem_FDC.m_rs232.m_nBaudrate);
			//p_IndexParity = (int)Enum.ToObject(typeof(Parity), m_FDC.m_rs232.m_eParity);
			//p_IndexDataBits= (int)Enum.ToObject(typeof(DataBits), m_FDC.m_rs232.m_nDataBit);
			//p_IndexStopBits= (int)Enum.ToObject(typeof(StopBits), m_FDC.m_rs232.m_eStopbits);
			//p_IndexBaudrate = (int)m_FDC.m_rs232.m_nBaudrate;
			//p_IndexParity = (int)m_FDC.m_rs232.m_eParity;
			//p_IndexDataBits = m_FDC.m_rs232.m_nDataBit;
			//p_IndexStopBits = (int)m_FDC.m_rs232.m_eStopbits;



			//p_IndexPort = m_reg.Read(Member.GetName(() => p_IndexPort), (int)0);
			//p_IndexBaudrate = m_reg.Read(Member.GetName(() => p_IndexBaudrate), (int)0);
			//p_IndexParity = m_reg.Read(Member.GetName(() => p_IndexParity), (int)0);
			//p_IndexDataBits = m_reg.Read(Member.GetName(() => p_IndexDataBits), (int)0);
			//p_IndexStopBits = m_reg.Read(Member.GetName(() => p_IndexStopBits), (int)0);


			m_CVM_manager.Loaded();

			p_Item = new ObservableCollection<FDC_Control>();
			FDC_ControlViewModel viewmodel;
			FDC_Control _Control;
			for (int index = 0; index < m_CVM_manager.GetCount(); index++)
			{
				viewmodel = m_CVM_manager.GetCVM(index);
				_Control = new FDC_Control(viewmodel);

				p_Item.Add(_Control);
				m_CVM_manager.GetCVM(index).SendDelegate += (this.MakeDelegate);
				m_CVM_manager.GetCVM(index).DeleteDelegate += (this.DeleteDelegate);
			}
		}

		void BtnAddClick_Function()
		{
			var viewModel = new Dialog_SettingFDC_ViewModel(this);
			Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
			if (result.HasValue)
			{
				if (result.Value)
				{
					p_bConnectState = false;
					//m_SerialComm.UpdateStop();
					m_CVM_manager.GetLastCVM().SendDelegate += (this.MakeDelegate);
					m_CVM_manager.GetLastCVM().DeleteDelegate += (this.DeleteDelegate);
				}
				else
				{
					// Cancelled
				}
			}
		}

		void DeleteDelegate(int _i)
		{
			p_bConnectState = false;
			//m_SerialComm.UpdateStop();
			p_Item.RemoveAt(_i);
		}

		void MakeDelegate(FDC_ControlViewModel tk, CVM_Manager _manager)
		{
			var viewModel = new Dialog_SettingFDC_ViewModel(tk, _manager);
			Nullable<bool> result = m_DialogService.ShowDialog(viewModel);
			if (result.HasValue)
			{
				if (result.Value)
				{
					p_bConnectState = false;
					//m_SerialComm.UpdateStop();
				}
				else
				{
					// Cancelled
				}
			}
		}
	}
}
