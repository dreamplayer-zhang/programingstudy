using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;

namespace RootTools.RADS
{
	public class RADSControl : ObservableObject
	{
		//기존의 CDlgbRealtimeADS의 역할을 이어받는 클래스

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

		Timer timer { get; set; }

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
		public bool IsRun { get; set; }
		public int AdsData { get; set; }
		IEngineer m_engineer;

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

		public RADSControl(IEngineer engineer)
		{
			m_engineer = engineer;

			p_connect = new RADSConnectControl(m_engineer);
			p_connect.SearchComplete += OnSearchComplete;
			
			timer = new Timer(100);
			timer.Elapsed += Timer_Elapsed;
			timer.Start();
		}

		private void OnSearchComplete()
		{
			if (this.SearchComplete != null)
				this.SearchComplete();
		}

		private void Timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (IsReady)
			{
				//Console.WriteLine("Are you ready! I'm lady! 始めよう");

				//nRadsYOffset = ((CBaslerGrabber*)m_pRADSGrabber[0])->GetCameraYOffset();//basler 카메라의 y-offset get

				if (p_connect.p_CurrentController.p_ADS_run == 0)
				{
					IsRun = false;//"Ready";
				}
				else if (p_connect.p_CurrentController.p_ADS_up > 0)
				{
					IsRun = true;
                    AdsData = p_connect.p_CurrentController.p_ADS_data * -1;
                    Console.WriteLine("AdsData : {0}", AdsData);
				}
				else
				{
					IsRun = true;
                    AdsData = p_connect.p_CurrentController.p_ADS_data;
                    Console.WriteLine("AdsData : {0}", AdsData);
				}

			}
			else
			{
				//Console.WriteLine("You're not ready!");
			}
		}

		public void Dispose()
		{
			timer.Stop();
			timer.Dispose();
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
			if (p_connect.Start())
			{
				p_connect.ChangeViewStatus(true);
				if (this.Started != null)
					this.Started();
			}
		}

		public void StopRADS()
		{
			if (p_connect.Stop())
			{
				p_connect.ChangeViewStatus(false);
				if (this.Stopped != null)
					this.Stopped();
			}
		}
	}
}
