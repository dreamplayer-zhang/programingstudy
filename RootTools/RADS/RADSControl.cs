using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace RootTools.RADS
{
    public class RADSControl : ObservableObject, ITool
	{
		public string p_id { get; set; }
		Log m_log;

		public RADSControl(string id, Log log, bool bUseRADS)
		{
			p_id = id;
			m_log = log;

			p_connect = new RADSConnectControl(this, bUseRADS);
			p_connect.SearchComplete += OnSearchComplete;

			//if (bUseRADS == true)
			//{
			//	p_treeRoot = p_connect.p_CurrentController.p_TreeRoot;

			m_tickTime = DateTime.Now;

			m_timer = new Timer(100);
			m_timer.Elapsed += Timer_Elapsed;
			m_timer.Start();
			//}

			InitRS232();
			m_rs232.p_bConnect = true;
		}

		#region Tree
		TreeRoot m_treeRoot = null;
		public TreeRoot p_treeRoot
		{
			get { return m_treeRoot; }
			set { SetProperty(ref m_treeRoot, value); }
		}
		#endregion

		#region UI
		public UserControl p_ui
		{
			get
			{
				RADSControl_UI ui = new RADSControl_UI();
				ui.Init(this);
				
				if(ui.m_voltPoints != null)
					m_voltPoints = ui.m_voltPoints;

				return (UserControl)ui;
			}
		}

		DataPointCollection m_voltPoints = null;

		#endregion

		#region EventHandler
		/// <summary>
		/// 이벤트 핸들러
		/// </summary>
		public delegate void EventHandler();
		public EventHandler SearchComplete;
		public EventHandler Started;
		public EventHandler Stopped;
		#endregion

		#region ControllerName
		/// <summary>
		/// 선택된 컨트롤러의 이름을 표시. 기본값 N/A
		/// </summary>
		public string ControllerName
		{
			get
			{
				if (p_connect.p_CurrentController == null)
					return "N/A";
				else
					return p_connect.p_CurrentController.Name;
			}
		}
		#endregion

		#region ControllerMacAddress
		/// <summary>
		/// 선택된 컨트롤러의 MAC 주소를 표시. 기본값 00:00:00:00:00:00
		/// </summary>
		public string ControllerMacAddress
		{
			get
			{
				if (p_connect.p_CurrentController == null)
					return "00:00:00:00:00:00";
				else
					return p_connect.p_CurrentController.p_MAC;
			}
		}
		#endregion

		#region ControllerIP
		/// <summary>
		/// 선택된 컨트롤러의 IP주소를 표시. 기본값 0.0.0.0
		/// </summary>
		public IPAddress ControllerIP
		{
			get
			{
				if (p_connect.p_CurrentController == null)
					return IPAddress.Parse("0.0.0.0");
				else
					return p_connect.p_CurrentController.p_IP;
			}
		}
		#endregion

		#region IsReady
		/// <summary>
		/// 컨트롤러와 통신이 완료되었고, 사용할 수 있는 상태인지 확인
		/// </summary>
		bool IsReady
		{
			get
			{
				return (p_connect.p_CurrentController != null && p_connect != null) ? true : false;
			}
		}
		#endregion

		#region RS232
		public RS232byte m_rs232;
		void InitRS232()
		{
			m_rs232 = new RS232byte(p_id, m_log);
			m_rs232.OnReceive += M_rs232_OnReceive;
		}

		private void M_rs232_OnReceive(byte[] aRead, int nRead)
		{
			if (nRead > 0)
			{
				if (aRead[0] == 165)
				{
					byte b1 = aRead[1];
					byte b2 = aRead[2];
					int nVal = b1 << 8 | b2;

					int nVoltage = 150 * nVal / 0x7600;
					p_nVoltage = nVoltage;
				}
			}
		}
		#endregion

		public Timer m_timer { get; set; }

		RADSConnectControl m_connect;
		public RADSConnectControl p_connect
		{
			get
			{
				return m_connect;
			}
			set
			{
				SetProperty(ref m_connect, value);
			}
		}

		bool m_IsRun;
		public bool p_IsRun
		{
			get { return m_IsRun; }
			set { SetProperty(ref m_IsRun, value); }
		}

		int m_nAdsData = 0;
		public int p_nAdsData
        {
            get { return m_nAdsData; }
            set
            {
				SetProperty(ref m_nAdsData, value);
            }
        }

		int m_nVoltage = 0;
		public int p_nVoltage
		{
			get { return m_nVoltage; }
			set
			{
				SetProperty(ref m_nVoltage, value);
			}
		}

		private void OnSearchComplete()
		{
			if (this.SearchComplete != null)
				this.SearchComplete();
		}

		List<int> m_lstVoltageLog = new List<int>();

		DateTime m_tickTime;
		double m_dVoltageSum = 0;
		double m_dElapsedTime = 0;

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (IsReady)
			{
				//Console.WriteLine("Are you ready! I'm lady! 始めよう");

				//nRadsYOffset = ((CBaslerGrabber*)m_pRADSGrabber[0])->GetCameraYOffset();//basler 카메라의 y-offset get

				if (p_connect.p_CurrentController.p_ADS_run == 0)
				{
					p_IsRun = false;//"Ready";
				}
				else if (p_connect.p_CurrentController.p_ADS_up > 0)
				{
					p_IsRun = true;
                    p_nAdsData = p_connect.p_CurrentController.p_ADS_data * -1;
                    //Console.WriteLine("AdsData : {0}", p_nAdsData);
				}
				else
				{
					p_IsRun = true;
                    p_nAdsData = p_connect.p_CurrentController.p_ADS_data;
					//Console.WriteLine("AdsData : {0}", p_nAdsData);
				}
			}
			else
			{
				//Console.WriteLine("You're not ready!");
			}

			if(m_rs232.p_bConnect)
            {
				// 분당 평균 Voltage 로그 작성
				TimeSpan diffTime = e.SignalTime - m_tickTime;
				m_dElapsedTime += diffTime.TotalMilliseconds;
				m_dVoltageSum += p_nVoltage * diffTime.TotalMilliseconds;

				if (m_dElapsedTime > 10 * 1000)
				{
					double dAvr = m_dVoltageSum / m_dElapsedTime;
					m_log.Info(string.Format("Average Voltage: {0}", dAvr.ToString(".00")));

					m_dVoltageSum = 0;
					m_dElapsedTime = 0;
				}

				m_tickTime = e.SignalTime;

				// 100개의 Voltage 데이터가 모일 때마다 별도 파일로 로그 작성
				m_lstVoltageLog.Add(p_nVoltage);
				if(m_lstVoltageLog.Count >= 100)
                {
					WriteVoltageLog();

					m_lstVoltageLog.Clear();
                }
			}
		}

		void WriteVoltageLog()
		{
			DateTime dt = DateTime.Now;
			string sPath = LogView._logView.p_sPath;

			if(!Directory.Exists(sPath + "\\RADS_Voltage"))
				Directory.CreateDirectory(sPath + "\\RADS_Voltage");

			string strFile = sPath + "\\RADS_Voltage" + "\\" + dt.ToShortDateString() + ".txt";
			string strTime = dt.Hour.ToString("00") + '.' + dt.Minute.ToString("00") + '.' + dt.Second.ToString("00") + '.' + dt.Millisecond.ToString("000");

			using (StreamWriter writer = new StreamWriter(strFile, true, Encoding.Default))
            {
				string strValues = string.Join(", ", m_lstVoltageLog.ToArray());
				writer.WriteLine(strTime + "\t" + strValues);
            }
		}

		public void Dispose()
		{
			m_timer.Stop();
			m_timer.Dispose();
		}

		public void ResetController()
		{
			if (p_connect.SetResetControllerPacket())
			{

			}
		}

		public void UpdateDeviceInfo()
		{
			p_connect.GetDeviceInfo("255.255.255.255");
		}

		public void StartRADS()
		{
			m_tickTime = DateTime.Now;

			m_timer.Start();
			p_IsRun = true;

			if (p_connect.Start())
			{
				p_connect.ChangeViewStatus(true);
				if (this.Started != null)
					this.Started();
			}
		}

		public void StopRADS()
		{
			m_dVoltageSum = 0;
			m_dElapsedTime = 0;

			WriteVoltageLog();
			m_lstVoltageLog.Clear();

			m_timer.Stop();
			p_IsRun = false;

			if (p_connect.Stop())
			{
				p_connect.ChangeViewStatus(false);
				if (this.Stopped != null)
					this.Stopped();
			}
		}

		public void ThreadStop()
		{
			
		}
	}
}
