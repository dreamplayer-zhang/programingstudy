using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Reflection;
using System.IO.Ports;
using System.Diagnostics;
using System.Windows.Threading;
using System.Linq.Expressions;
using RootTools;
using RootTools.Trees;
using Root_Vega.Module;
using System.Windows.Data;
using System.Globalization;

namespace Root_Vega
{
    public static class Member
    {
        public static string GetName<T>(Expression<Func<T>> memberExpression)
        {
            MemberExpression expressionBody = (MemberExpression)memberExpression.Body;
            return expressionBody.Member.Name;
        }
    }

    public enum FDC_Unit
	{
		[Description("")]
		None,
		[Description("kPa")]
		Kilopascal,
		[Description("MPa")]
		Megapascal,
		[Description("°C")]
		Celsius,
		[Description("V")]
		Volt
	}

	public class SetValue : IMultiValueConverter  
	{
		public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
		{
		
				if (value[0] == DependencyProperty.UnsetValue)
					return 0;

				double Length = (double)value[0] - (double)value[1];
				double result;
				result = (double)value[0] + Length * 0.1;

				return result;
		
		}
		public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}

    //TK4S유저컨트롤을 모아둔 클래스.
    public class CVM_Manager : ObservableObject
    {
		//@private FDC
		private List<FDC_ControlViewModel> m_CVM;
		private int[] m_SlaveList;
        public readonly IDialogService m_DialogService;
		DispatcherTimer m_timer = new DispatcherTimer();

		public List<FDC_ControlViewModel> m_CVMDeleteList = new List<FDC_ControlViewModel>();
		public bool bDeleteState = false;

        public CVM_Manager(IDialogService DialogService)
        {
            m_DialogService = DialogService;

			m_timer.Interval = TimeSpan.FromMilliseconds(500);
			m_timer.Tick += new EventHandler(timer_Tick);
			m_timer.Start();
		}

        public void Add_CVM()
        {
			m_CVM.Add(new FDC_ControlViewModel(this));
        }

        public int[] SlaveList()
        {
            m_SlaveList = new int[m_CVM.Count];
            for (int i = 0; i < m_CVM.Count; i++)
            {
                m_SlaveList[i] = m_CVM[i].p_SlaveNumber;
            }
            return m_SlaveList;
        }

		public FDC_ControlViewModel GetLastCVM()
        {
			return m_CVM.Last<FDC_ControlViewModel>();
        }

		public FDC_ControlViewModel GetCVM(int num)
        {
            return m_CVM[num];
        }

        public int GetCount()
        {
            return m_CVM.Count;
        }

		public int Remove(FDC_ControlViewModel del)
        {
            for (int i = 0; i < m_CVM.Count; i++)
                if (m_CVM[i] == del)
                {
                    m_CVM.RemoveAt(i);
                    ModifyRegistry();
                    return i;
                }
            return -1;
        }

        public void SetTemperature(int _slaveNum, int _temp)
        {
            for (int i = 0; i < m_SlaveList.Length; i++)
            {
                if (_slaveNum == m_SlaveList[i])
                {
					m_CVM[i].p_CurrentValue = _temp;
					m_CVM[i].p_CurrentString = m_CVM[i].p_CurrentValue + " " +  m_CVM[i].p_SelectedUnit;
                    //break;
                }
            }
        }

		public Root_Vega.Module.FDC m_FDC;
        Registry m_reg;



		public void Loaded()
		{ 
            m_reg = new Registry("TK4S");
            int CVM_Counter = m_FDC.p_lData;
			m_CVM = new List<FDC_ControlViewModel>();
			for (int index = 0; index < CVM_Counter; index++)
			{
				m_CVM.Add(new FDC_ControlViewModel(this));
				m_CVM[index].p_SlaveNumber = m_reg.Read(Member.GetName(() => m_CVM[index].p_SlaveNumber), (int)0);
				m_CVM[index].p_TmprtrName = m_reg.Read(Member.GetName(() => m_CVM[index].p_TmprtrName), (string)"");
				m_CVM[index].p_EnumSelectedUnit = (FDC_Unit)(int)(m_FDC.m_aData[index].p_eUnit);
				m_CVM[index].p_LowerValue = m_FDC.m_aData[index].m_mmLimit.X;
				m_CVM[index].p_UpperValue = m_FDC.m_aData[index].m_mmLimit.Y;

				//m_CVM[index].p_SlaveNumber = m_reg.Read(Member.GetName(() => m_CVM[index].p_SlaveNumber), (int)0);
				//m_CVM[index].p_TmprtrName = m_reg.Read(Member.GetName(() => m_CVM[index].p_TmprtrName), (string)"");
				//m_CVM[index].p_EnumSelectedUnit = (FDC_Unit)m_reg.Read(Member.GetName(() => m_CVM[index].p_EnumSelectedUnit), (int)0);
				//m_CVM[index].p_LowerValue = m_reg.Read(Member.GetName(() => m_CVM[index].p_LowerValue), (double)0);
				//m_CVM[index].p_UpperValue = m_reg.Read(Member.GetName(() => m_CVM[index].p_UpperValue), (double)0);
			}
                                    
			m_SlaveList = SlaveList();
        }

		private void timer_Tick(object sender, EventArgs e)//check
		{
			int CVM_Counter = m_FDC.p_lData;
			if (m_CVM != null)
			{
				for (int index = 0; index < CVM_Counter; index++)
				{
					m_CVM[index].p_CurrentValue = m_FDC.m_aData[index].p_fValue;
				}
			}
		}

		public void ModifyRegistry()
        {
            m_reg = new Registry("TK4S");
            int CVM_Counter = m_CVM.Count;
			m_FDC.p_lData = CVM_Counter;
            m_reg.Write(Member.GetName(() => CVM_Counter), CVM_Counter);

            for (int index = 0; index < CVM_Counter; index++)
            {
                m_reg = new Registry("TK4S." + index);

				//m_FDC.m_rs232.m_nDataBit = m_IndexDataBits;
				//m_FDC.m_rs232.RunTree(Tree.eMode.Update);
				//m_FDC.m_rs232.RunTree(Tree.eMode.RegWrite);


                m_reg.Write(Member.GetName(() => m_CVM[index].p_SlaveNumber), m_CVM[index].p_SlaveNumber);
                m_reg.Write(Member.GetName(() => m_CVM[index].p_TmprtrName), m_CVM[index].p_TmprtrName);
				m_FDC.m_aData[index].p_eUnit = (FDC.eUnit)(int)(m_CVM[index].p_EnumSelectedUnit);
				m_FDC.m_aData[index].m_mmLimit.X = (int)m_CVM[index].p_LowerValue ;
				m_FDC.m_aData[index].m_mmLimit.Y = (int)m_CVM[index].p_UpperValue;
			//	m_FDC.m_aData.RunTree(Tree.eMode.Update);
            }
				m_FDC.RunTree(Tree.eMode.Init);
				m_FDC.RunTree(Tree.eMode.RegWrite);

        }
    }


    //TK4S유저컨트롤 뷰모델.
    public delegate void DialogDelegateHandler(FDC_ControlViewModel tk, CVM_Manager _manager);
    public delegate void DeleteDelegateHandler(int _i);
    public class FDC_ControlViewModel : ObservableObject
    {
        #region Xaml_Property
        //tk4s슬레이브 번호.
        private int m_SlaveNumber;
        public int p_SlaveNumber
        {
            get { return m_SlaveNumber; }
            set
            {
                SetProperty(ref m_SlaveNumber, value);
            }
        }

        private double m_CurrentValue;
        public double p_CurrentValue
        {
            get { return m_CurrentValue; }
            set
            {
                SetProperty(ref m_CurrentValue, value);
				p_CurrentString = p_CurrentValue + " " + p_SelectedUnit;
            }
        }

		private double m_MinLimitValue=0;
		public double p_MinLimitValue
        {
			get { return m_MinLimitValue; }
			set
			{
				SetProperty(ref m_MinLimitValue, value);
			}
        }

		private double m_MaxLimitValue=0;
			public double p_MaxLimitValue
        {
			get { return m_MaxLimitValue; }
			set
			{
				SetProperty(ref m_MaxLimitValue, value);
			}
        }
		
		private double m_LowerValue=0;
		public double p_LowerValue
        {
            get { return m_LowerValue; }
            set
            {
                SetProperty(ref m_LowerValue, value);
				p_MinLimitValue = 1.1 * p_LowerValue - 0.1 * p_UpperValue;
				p_MaxLimitValue = 1.1 * p_UpperValue - 0.1 * p_LowerValue;
            }
        }

		private double m_UpperValue=0;
		public double p_UpperValue
		{
			get { return m_UpperValue; }
			set
			{
				SetProperty(ref m_UpperValue, value);
				p_MinLimitValue = 1.1 * p_LowerValue - 0.1 * p_UpperValue;
				p_MaxLimitValue = 1.1 * p_UpperValue - 0.1 * p_LowerValue;
			}
		}

        //사용자가 입력한 유저컨트롤 이름.

		private string m_XamlName;
		public string p_XamlName
		{
			get { return m_XamlName; }
			set
			{
				SetProperty(ref m_XamlName, value);
			}
        }

		private Visibility m_Selected =Visibility.Hidden;
		public Visibility p_Selected
		{
			get { return m_Selected; }
			set
			{
				SetProperty(ref m_Selected, value);
			}
		}

		private string m_TmprtrName;
		public string p_TmprtrName
		{
			get { return m_TmprtrName; }
			set
			{
				SetProperty(ref m_TmprtrName, value);
				p_XamlName = p_SlaveNumber + ". " + m_TmprtrName;
			}
		}

        private string m_SelectedUnit;
        public string p_SelectedUnit
        {
            get { return m_SelectedUnit; }
            set
            {
                SetProperty(ref m_SelectedUnit, value);
            }
        }

        private FDC_Unit m_EnumSelectedUnit;
        public FDC_Unit p_EnumSelectedUnit
        {
            get { return m_EnumSelectedUnit; }
            set
            {
                Type type = value.GetType();
                MemberInfo[] memInfo = type.GetMember(value.ToString());
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                p_SelectedUnit = ((DescriptionAttribute)attrs[0]).Description;
                m_EnumSelectedUnit = value;
            }
        }

		private Func<double, string> _gaugeFormatter;
		public Func<double, string> GaugeFormatter
		{
			get
			{
				return _gaugeFormatter;
			}
			set
			{
				SetProperty(ref _gaugeFormatter, value);
			}
		}
		private Func<double, string> _AngularGaugeFormatter;
		public Func<double, string> AngularGaugeFormatter
		{
			get
			{
				return _AngularGaugeFormatter;
			}
			set
			{
				SetProperty(ref _AngularGaugeFormatter, value);
			}
		}


		private string m_CurrentString;
		public string p_CurrentString
		{
			get { return m_CurrentString; }
			set { SetProperty(ref m_CurrentString, value); }
		}

        #endregion
        CVM_Manager manager;
		public FDC_ControlViewModel(CVM_Manager _manager)
        {
            manager = _manager;
			p_CurrentString = "";
			p_LowerValue = 0;
			p_UpperValue = 100;

			GaugeFormatter = value =>
			{
				return "";
				//return value.ToString("F2") + "%";
			};
        }

        #region respond
		public ICommand MouseLeftButtonDownCommand
        {
            get
            {
				return new RelayCommand(MouseLeftButtonDownFunction);
            }
        }
		
		private StopWatch m_Click = new StopWatch();
        public DialogDelegateHandler SendDelegate;
		void MouseLeftButtonDownFunction()
        {
			if (manager.bDeleteState == true)
			{
				if (p_Selected == Visibility.Visible)
				{
					p_Selected = Visibility.Hidden;
					manager.m_CVMDeleteList.Remove(this);
				}
				else if (p_Selected == Visibility.Hidden)
				{ 
					p_Selected = Visibility.Visible;
					manager.m_CVMDeleteList.Add(this);
				}
			}

			if (m_Click.ElapsedMilliseconds < 500)
				SendDelegate(this, manager);
			else
				m_Click.Restart();
		}

        public ICommand DeleteBtnClickCommand
        {
            get
            {
                return new RelayCommand(DeleteBtnClickFunction);
            }
        }
        public DeleteDelegateHandler DeleteDelegate;
        void DeleteBtnClickFunction()
        {
            int removeIndex = manager.Remove(this);

            if (removeIndex != -1)
                DeleteDelegate(removeIndex);
        }
        #endregion
    }
}