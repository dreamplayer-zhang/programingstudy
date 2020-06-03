using RootTools.Inspects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools.RADS
{
	public class RADSConnectControl : ObservableObject
	{
		#region EventHandler
		/// <summary>
		/// 이벤트 핸들러
		/// </summary>
		public delegate void EventHandler();
		public EventHandler SearchComplete;
		#endregion

		UdpClient listen = null;

		private UdpClient dataSocket;
		//Socket listen = null;
		RADS m_CurrentController;
		public RADS p_CurrentController
		{
			get
			{
				return m_CurrentController;
			}
			set
			{
				SetProperty(ref m_CurrentController, value);
			}
		}
		int seq_number;
		IEngineer m_engineer;
		public RADSConnectControl(IEngineer engineer)
		{
			m_engineer = engineer;
			p_CurrentController = null;

			if (listen == null)
			{
				listen = new UdpClient(RADSControlInfo.ADSCP_PORT);

			}
			else
			{
				Console.WriteLine("Alerady Listening");
				return;
			}
			if (dataSocket == null)
			{

				dataSocket = new UdpClient(RADSControlInfo.ADSDP_PORT);
				dataSocket.BeginReceive(OnReceive, dataSocket);
			}
			else
			{
				Console.WriteLine("Alerady Data Listening");
				return;
			}
			GetDeviceInfo();

			//StartListening();
		}
		byte[] buffer = new byte[1024];

        public void StartListening()
        {

           

        }
        /// <summary>
        /// 물리적으로 연결된 모든 네트워크에서 Piezo Controller를 탐색
        /// </summary>
        /// <param name="dest_ip">Broadcast IP. 기본값은 Piezo Broadcast IP인 100.11.255.255. 그 외 전역 탐색의 경우 255.255.255.255 사용 권장</param>
        public void GetDeviceInfo(string dest_ip = "255.255.255.255")
		{
			//100.11.0.12 Default IP Dest
			//Broadcast Address : 100.11.255.255
			//Normal Broadcast Address : 255.255.255.255
		
			string message = RADSControlInfo.ADSCP_TYPE_REQ + RADSControlInfo.ADSCP_OPCODE_PING;

			if (seq_number < 65534)
			{
				seq_number += 1;
			}
			else
			{
				seq_number = 0;
			}
			message += "0000";
			message += seq_number.ToString("X4");

			byte[] bytes = ConvertToByteArr(message);
			listen.EnableBroadcast = true;
			listen.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Parse(dest_ip), RADSControlInfo.ADSCP_PORT));

			IPEndPoint from = new IPEndPoint(0, 0);
			while (true)
			{

				var echoBuffer = listen.Receive(ref from);

				var ADSCP_Type = echoBuffer[0].ToString("X2") + echoBuffer[1].ToString("X2");
				var ADSCP_Opcode = echoBuffer[2].ToString("X2") + echoBuffer[3].ToString("X2");
				var ADSCP_Length = echoBuffer[4].ToString("X2") + echoBuffer[5].ToString("X2");
				int ADSCP_seqNumber = (int)uint.Parse(echoBuffer[6].ToString("X2") + echoBuffer[7].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
				int ADSCP_address = (int)uint.Parse(echoBuffer[8].ToString("X2") + echoBuffer[9].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
				int ADSCP_value = (int)uint.Parse(echoBuffer[10].ToString("X2") + echoBuffer[11].ToString("X2"), System.Globalization.NumberStyles.HexNumber);

				Console.WriteLine("Response IP Address : {0}", from);
				Console.WriteLine("Echo Data Received : {0} {1} {2} {3} {4} {5}",
					ADSCP_Type, ADSCP_Opcode, ADSCP_Length, ADSCP_seqNumber, ADSCP_address, ADSCP_value);

				if (ADSCP_Type == "0000" && ADSCP_Opcode == RADSControlInfo.ADSCP_OPCODE_PONG) //Controller Discovery Pong!
				{
					byte[] controller_ip = new byte[4];
					byte[] controller_mac = new byte[6];
					byte[] controller_name = new byte[10];

					for (int i = 0; i < 10; i++)
					{
						controller_name[i] = echoBuffer[i + 8];
						if (i < 6) { controller_mac[i] = echoBuffer[i + 18]; }
						if (i < 4) { controller_ip[i] = echoBuffer[i + 24]; }
					}
					var MAC = controller_mac[0].ToString("X2") + ":" + controller_mac[1].ToString("X2") + ":" + controller_mac[2].ToString("X2") + ":" + (controller_mac[3]).ToString("X2") + ":" + (controller_mac[4]).ToString("X2") + ":" + (controller_mac[5]).ToString("X2");

					RADS controller = new RADS(m_engineer);
					controller.ADSCP_Type = ADSCP_Type;
					controller.ADSCP_Opcode = ADSCP_Opcode;
					controller.ADSCP_Length = ADSCP_Length;
					controller.RawName = controller_name;
					controller.p_MAC = MAC;
					controller.p_IP = new IPAddress(controller_ip);

					listen.Close();
					listen = null;
					//	GC.Collect();//TODO : 나중에 원인 찾아서 수정해야 함


					p_CurrentController = controller;

					Console.WriteLine("Find Controller. Controller Info : ");
					Console.WriteLine(p_CurrentController.GetInformation());

					Console.WriteLine("Start Read Registry");

					ReadPacket(0);
					ReadPacket(1);
					ReadPacket(2);
					ReadPacket(3);
					ReadPacket(4);
					ReadPacket(5);
					//6,7번은 어디로갔을깡...
					ReadPacket(8);
					ReadPacket(9);
					Console.WriteLine("Read Registry Finish");


					if (SearchComplete != null)
					{
						SearchComplete();
					}
					break;
				}
			}
		}
		private void ChangeRegestry(int ADSCP_address, int ADSCP_value)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return;
			}
			switch (ADSCP_address)
			{
				case 0:
					if ((32768 & ADSCP_value) != 0)
					{
						p_CurrentController.p_ADS_run = 1;
						Console.WriteLine("Data : ADS_run - On");
					}
					else
					{
						p_CurrentController.p_ADS_run = 0;
						Console.WriteLine("Data : ADS_run - Off");
					}

					if ((16384 & ADSCP_value) != 0)
					{
						p_CurrentController.p_ADS_view = 1;
					}
					else
					{
						p_CurrentController.p_ADS_view = 0;
					}
					Console.WriteLine("Data - ADS_view:{0}\t", p_CurrentController.p_ADS_view);

					if ((128 & ADSCP_value) != 0)
					{
						p_CurrentController.p_filterType = RADS.eFilterType.Unknown;
					}
					else
					{
						p_CurrentController.p_filterType = RADS.eFilterType.Mean;
					}
					Console.WriteLine("Data - filterType:{0}\t", p_CurrentController.p_filterType);

					if ((64 & ADSCP_value) != 0)
					{
						p_CurrentController.p_yFilter_on = true;
					}
					else
					{
						p_CurrentController.p_yFilter_on = false;
					}
					Console.WriteLine("Data - yFilter_on:{0}\t", p_CurrentController.p_yFilter_on);

					if ((32 & ADSCP_value) != 0)
					{
						p_CurrentController.p_hFilter_on = true;
					}
					else
					{
						p_CurrentController.p_hFilter_on = false;
					}
					Console.WriteLine("Data - hFilter_on:{0}\t", p_CurrentController.p_hFilter_on);

					if ((16 & ADSCP_value) != 0)
					{
						p_CurrentController.p_wFilter_on = true;
					}
					else
					{
						p_CurrentController.p_wFilter_on = false;
					}
					Console.WriteLine("Data - wFilter_on:{0}\t", p_CurrentController.p_wFilter_on);

					break;
				case 1:
					p_CurrentController.p_ySize = ADSCP_value;
					Console.WriteLine("Data - ySize:{0}\t", p_CurrentController.p_ySize);
					break;
				case 2:
					p_CurrentController.p_nThreshold = ADSCP_value;
					Console.WriteLine("Data - nThreshold:{0}\t", p_CurrentController.p_nThreshold);
					break;
				case 3:
					p_CurrentController.p_hFilterMin = ADSCP_value;
					Console.WriteLine("Data - hFilterMin:{0}\t", p_CurrentController.p_hFilterMin);
					break;
				case 4:
					p_CurrentController.p_hFilterMax = ADSCP_value;
					Console.WriteLine("Data - hFilterMax:{0}\t", p_CurrentController.p_hFilterMax);
					break;
				case 5:
					p_CurrentController.p_wFilterSize = ADSCP_value;
					Console.WriteLine("Data - wFilterSize:{0}\t", p_CurrentController.p_wFilterSize);
					break;
				case 8:
					p_CurrentController.p_limit = ADSCP_value;
					Console.WriteLine("Data - limit:{0}\t", p_CurrentController.p_limit);
					break;
				case 9:
					if (ADSCP_value > 128)
					{   // down
						p_CurrentController.p_offset_value = ADSCP_value - 128;
						p_CurrentController.p_offset_updown = 1;
					}
					else if (ADSCP_value < 128)
					{   // up
						p_CurrentController.p_offset_value = 128 - ADSCP_value;
						p_CurrentController.p_offset_updown = 0;
					}
					else
					{
						p_CurrentController.p_offset_value = 0;
						p_CurrentController.p_offset_updown = 0;
					}
					Console.WriteLine("Data - offset_value:{0}\toffset_updown:{1}", p_CurrentController.p_offset_value, p_CurrentController.p_offset_updown);
					break;
			}
		}

		private void ReadPacket(int address)
		{
			// send read packet to controller 
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return;
			}
			string out_packet = RADSControlInfo.ADSCP_TYPE_REQ;
			out_packet += RADSControlInfo.ADSCP_OPCODE_READ_REQ;
			int length = 4;
			out_packet += length.ToString("X4");
			if (seq_number < 65534)
			{
				seq_number += 1;
			}
			else
			{
				seq_number = 0;
			}
			out_packet += seq_number.ToString("X4");
			out_packet += address.ToString("X4");
			byte[] bytes = ConvertToByteArr(out_packet);
			dataSocket.Send(bytes, bytes.Length, new IPEndPoint(p_CurrentController.p_IP, RADSControlInfo.ADSCP_PORT));
		}

		private void OnReceive(IAsyncResult ar)
		{
			UdpClient u = (UdpClient)ar.AsyncState;
			IPEndPoint ep=new IPEndPoint(0,0);
			var results = u.EndReceive(ar, ref ep);

			var ADSCP_Type = results[0].ToString("X2") + results[1].ToString("X2");
			var ADSCP_Opcode = results[2].ToString("X2") + results[3].ToString("X2");
			var ADSCP_Length = results[4].ToString("X2") + results[5].ToString("X2");
			int ADSCP_seqNumber = (int)uint.Parse(results[6].ToString("X2") + results[7].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
			int ADSCP_address = (int)uint.Parse(results[8].ToString("X2") + results[9].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
			int ADSCP_value = (int)uint.Parse(results[10].ToString("X2") + results[11].ToString("X2"), System.Globalization.NumberStyles.HexNumber);

			Console.WriteLine("Echo Data Received : {0} {1} {2} {3} {4} {5}",
				ADSCP_Type, ADSCP_Opcode, ADSCP_Length, ADSCP_seqNumber, ADSCP_address, ADSCP_value);

			if (ADSCP_Type == "3202" && ADSCP_Opcode == RADSControlInfo.ADSCP_OPCODE_READ_RSP)
			{
				//Read response!
				Console.WriteLine("Read Packet Response");
				p_CurrentController.p_reg[ADSCP_address] = ADSCP_value;
				ChangeRegestry(ADSCP_address, ADSCP_value);
				Console.WriteLine();
				
			}

			
			if (ADSCP_Type == "3202" && ADSCP_Opcode == RADSControlInfo.ADSCP_OPCODE_SEND_DATA)
			{
				int ADSCP_ADS_up = (int)uint.Parse(results[8].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
				int ADSCP_ADS_down = (int)uint.Parse(results[9].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
				int ADSCP_ADS_data = (int)uint.Parse(results[10].ToString("X2") + results[11].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
				if (p_CurrentController != null)
				{
					p_CurrentController.p_ADS_up = ADSCP_ADS_up;
					p_CurrentController.p_ADS_down = ADSCP_ADS_down;
					p_CurrentController.p_ADS_data = ADSCP_ADS_data;
				}
				
			}
			u.BeginReceive(OnReceive, u);
		}
		/// <summary>
		/// Piezo Reset
		/// </summary>
		/// <returns></returns>
		public bool SetResetControllerPacket()
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			bool result = false;

			string message = RADSControlInfo.ADSCP_TYPE_REQ;
			message += RADSControlInfo.ADSCP_OPCODE_RESET_REQ;
			int length = 0;
			message += length.ToString("X4");
			if (seq_number < 65534)
			{
				seq_number += 1;
			}
			else
			{
				seq_number = 0;
			}
			message += seq_number.ToString("X4");

			byte[] bytes = ConvertToByteArr(message);
			UdpClient sender = new UdpClient(RADSControlInfo.ADSCP_PORT);

			sender.Send(bytes, bytes.Length, new IPEndPoint(p_CurrentController.p_IP, RADSControlInfo.ADSCP_PORT));

			IPEndPoint remoteEP = new IPEndPoint(0, 0);

			var results = sender.Receive(ref remoteEP);

			var ADSCP_Type = results[0].ToString("X2") + results[1].ToString("X2");
			var ADSCP_Opcode = results[2].ToString("X2") + results[3].ToString("X2");
			var ADSCP_Length = results[4].ToString("X2") + results[5].ToString("X2");
			int ADSCP_seqNumber = (int)uint.Parse(results[6].ToString("X2") + results[7].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
			int ADSCP_address = (int)uint.Parse(results[8].ToString("X2") + results[9].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
			int ADSCP_value = (int)uint.Parse(results[10].ToString("X2") + results[11].ToString("X2"), System.Globalization.NumberStyles.HexNumber);

			Console.WriteLine("Response IP Address : {0}", remoteEP);
			Console.WriteLine("Echo Data Received : {0} {1} {2} {3} {4} {5}",
				ADSCP_Type, ADSCP_Opcode, ADSCP_Length, ADSCP_seqNumber, ADSCP_address, ADSCP_value);

			if (ADSCP_Type == "3202" && ADSCP_Opcode == RADSControlInfo.ADSCP_OPCODE_RESET_RSP)
			{
				//set base response!
				Console.WriteLine("Reset Complete!");
				result = true;
			}

			sender.Close();
			GC.Collect();//TODO : 나중에 원인 찾아서 수정해야 함
			return result;
		}

		void SetReg()
		{
			SetYSize(p_CurrentController.p_ySize);
			SetThreshold(p_CurrentController.p_nThreshold);
			SetHFilterMin(p_CurrentController.p_hFilterMin);
			SetHFilterMax(p_CurrentController.p_hFilterMax);
			SetWFilterSize(p_CurrentController.p_wFilterSize);
			SetADSLimit(p_CurrentController.p_limit);
			bool bDir = true;
			if (p_CurrentController.p_offset_updown < 0) bDir = false;
			SetADSOffset(bDir, Math.Abs(p_CurrentController.p_offset_value));
		}
		public bool Start()
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}

			bool result = false;
			p_CurrentController.p_ADS_run = 1;
			SetReg();
			result = SetFilter(p_CurrentController.p_filterType, p_CurrentController.p_yFilter_on, p_CurrentController.p_hFilter_on, p_CurrentController.p_wFilter_on);

			return result;
		}
		public bool Stop()
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}

			bool result = false;
			p_CurrentController.p_ADS_run = 0;
			SetReg();
			result = SetFilter(p_CurrentController.p_filterType, p_CurrentController.p_yFilter_on, p_CurrentController.p_hFilter_on, p_CurrentController.p_wFilter_on);

			return result;
		}
		/// <summary>
		/// View 옵션을 변경한다
		/// </summary>
		/// <param name="ViewStatus">View 상태로 전환할때 true</param>
		/// <returns></returns>
		public bool ChangeViewStatus(bool ViewStatus)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			p_CurrentController.p_ADS_view = ViewStatus ? 1 : 0;

			SetReg();
			return SetFilter(p_CurrentController.p_filterType, p_CurrentController.p_yFilter_on, p_CurrentController.p_hFilter_on, p_CurrentController.p_wFilter_on);
		}
		public void SetADSOffset(int value)
		{
			if (value > 0)
			{
				SetADSOffset(true, value);
			}
			else
			{
				SetADSOffset(false, value);
			}
		}
		public bool SetADSOffset(bool isUpStatus, int value)
		{
			int resultValue = 0;
			if (isUpStatus)
			{
				resultValue = 128 + value;
			}
			else
			{
				resultValue = 128 - value;
			}
			return WritePacket(9, resultValue);
		}
		public bool SetYSize(int value)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			p_CurrentController.p_ySize = value;
			return WritePacket(1, value);
		}
		public bool SetThreshold(int value)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			p_CurrentController.p_nThreshold = value;
			return WritePacket(2, value);
		}
		public bool SetHFilterMin(int value)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			p_CurrentController.p_hFilterMin = value;
			return WritePacket(3, value);
		}
		public bool SetHFilterMax(int value)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			p_CurrentController.p_hFilterMax = value;
			return WritePacket(4, value);
		}
		public bool SetWFilterSize(int value)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			p_CurrentController.p_wFilterSize = value;
			return WritePacket(5, value);
		}
		public bool SetADSLimit(int value)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			p_CurrentController.p_limit = value;
			return WritePacket(8, value);
		}
		public bool SetBaseLine()
		{
			return SetBaseLinePacket();
		}
		private bool SetBaseLinePacket()
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			bool result = false;

			string message = RADSControlInfo.ADSCP_TYPE_REQ;
			message += RADSControlInfo.ADSCP_OPCODE_SET_BASE_REQ;
			int length = 0;
			message += length.ToString("X4");
			if (seq_number < 65534)
			{
				seq_number += 1;
			}
			else
			{
				seq_number = 0;
			}
			message += seq_number.ToString("X4");

			byte[] bytes = ConvertToByteArr(message);
			UdpClient sender = new UdpClient(RADSControlInfo.ADSCP_PORT);

			sender.Send(bytes, bytes.Length, new IPEndPoint(p_CurrentController.p_IP, RADSControlInfo.ADSCP_PORT));

			IPEndPoint remoteEP = new IPEndPoint(0, 0);

			var results = sender.Receive(ref remoteEP);

			var ADSCP_Type = results[0].ToString("X2") + results[1].ToString("X2");
			var ADSCP_Opcode = results[2].ToString("X2") + results[3].ToString("X2");
			var ADSCP_Length = results[4].ToString("X2") + results[5].ToString("X2");
			int ADSCP_seqNumber = (int)uint.Parse(results[6].ToString("X2") + results[7].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
			int ADSCP_address = (int)uint.Parse(results[8].ToString("X2") + results[9].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
			int ADSCP_value = (int)uint.Parse(results[10].ToString("X2") + results[11].ToString("X2"), System.Globalization.NumberStyles.HexNumber);

			Console.WriteLine("Response IP Address : {0}", remoteEP);
			Console.WriteLine("Echo Data Received : {0} {1} {2} {3} {4} {5}",
				ADSCP_Type, ADSCP_Opcode, ADSCP_Length, ADSCP_seqNumber, ADSCP_address, ADSCP_value);

			if (ADSCP_Type == "3202" && ADSCP_Opcode == RADSControlInfo.ADSCP_OPCODE_SET_BASE_RSP)
			{
				//set base response!
				Console.WriteLine("Set BaseLine Function Complete!");
				result = true;
			}

			sender.Close();
			GC.Collect();//TODO : 나중에 원인 찾아서 수정해야 함
			return result;
		}

		private bool SetFilter(int filter_type, int yfilter_on, int hfilter_on, int wfilter_on)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			int ads_config = 0;
			bool result = false;

			if (filter_type == 1) { ads_config += 128; }
			if (yfilter_on == 1) { ads_config += 64; }
			if (hfilter_on == 1) { ads_config += 32; }
			if (wfilter_on == 1) { ads_config += 16; }
			if (p_CurrentController.p_ADS_view == 1) { ads_config += 16384; }
			if (p_CurrentController.p_ADS_run == 1) { ads_config += 32768; }
			do
			{
				result = WritePacket(0, ads_config);
			}
			while (ads_config != p_CurrentController.p_reg[0]);

			Console.WriteLine("Set Filter Complete");
			return result;
		}

		private bool SetFilter(RADS.eFilterType filter_type, bool yfilter_on, bool hfilter_on, bool wfilter_on)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			int ads_config = 0;
			bool result = false;

			if (filter_type == RADS.eFilterType.Mean) { ads_config += 128; }
			if (yfilter_on == true) { ads_config += 64; }
			if (hfilter_on == true) { ads_config += 32; }
			if (wfilter_on == true) { ads_config += 16; }
			if (p_CurrentController.p_ADS_view == 1) { ads_config += 16384; }
			if (p_CurrentController.p_ADS_run == 1) { ads_config += 32768; }
			do
			{
				result = WritePacket(0, ads_config);
			}
			while (ads_config != p_CurrentController.p_reg[0]);

			Console.WriteLine("Set Filter Complete");
			return result;
		}

		private bool WritePacket(int address, int value)
		{
			if (p_CurrentController == null)
			{
				Console.WriteLine("Current Controller is null. GetDeviceInfo() First");
				return false;
			}
			// send write packet to conroller
			bool result = false;
			string out_packet = RADSControlInfo.ADSCP_TYPE_REQ;
			out_packet += RADSControlInfo.ADSCP_OPCODE_WRITE_REQ;
			int length = 4;
			out_packet += length.ToString("X4");
			if (seq_number < 65534)
			{
				seq_number += 1;
			}
			else
			{
				seq_number = 0;
			}
			out_packet += seq_number.ToString("X4");
			out_packet += address.ToString("X4");
			out_packet += value.ToString("X4");
			//SEND
			byte[] bytes = ConvertToByteArr(out_packet);
			
			UdpClient sender = new UdpClient(RADSControlInfo.ADSCP_PORT);

			sender.Send(bytes, bytes.Length, new IPEndPoint(p_CurrentController.p_IP, RADSControlInfo.ADSCP_PORT));

			IPEndPoint remoteEP = new IPEndPoint(0, 0);

			var results = sender.Receive(ref remoteEP);

			var ADSCP_Type = results[0].ToString("X2") + results[1].ToString("X2");
			var ADSCP_Opcode = results[2].ToString("X2") + results[3].ToString("X2");
			var ADSCP_Length = results[4].ToString("X2") + results[5].ToString("X2");
			int ADSCP_seqNumber = (int)uint.Parse(results[6].ToString("X2") + results[7].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
			int ADSCP_address = (int)uint.Parse(results[8].ToString("X2") + results[9].ToString("X2"), System.Globalization.NumberStyles.HexNumber);
			int ADSCP_value = (int)uint.Parse(results[10].ToString("X2") + results[11].ToString("X2"), System.Globalization.NumberStyles.HexNumber);


			Console.WriteLine("Echo Data Received : {0} {1} {2} {3} {4} {5}",
				ADSCP_Type, ADSCP_Opcode, ADSCP_Length, ADSCP_seqNumber, ADSCP_address, ADSCP_value);

			if (ADSCP_Type == "3202" && ADSCP_Opcode == RADSControlInfo.ADSCP_OPCODE_WRITE_RSP)
			{
				//Write response!
				Console.WriteLine("Write Packet Response");

				p_CurrentController.p_reg[ADSCP_address] = ADSCP_value;
				ChangeRegestry(ADSCP_address, ADSCP_value);
				Console.WriteLine();
				result = true;
			}
			sender.Close();
			GC.Collect();//TODO : 나중에 원인 찾아서 수정해야 함

			return result;
		}

		byte[] ConvertToByteArr(string out_packet)
		{
			// sendPacket
			int buflen = out_packet.Length;

			string hex2byte;
			byte[] charData = new byte[buflen];
			int int2byte;
			int count = 0;
			for (int i = 0; i < buflen; i = i + 2)
			{
				hex2byte = out_packet.Substring(i, 2);
				int2byte = int.Parse(hex2byte, System.Globalization.NumberStyles.AllowHexSpecifier);
				charData[count] = (byte)Convert.ToChar(int2byte);
				count = count + 1;
			}
			return charData;
		}
		private void PrintIpAddressAndSubmask()
		{
			NetworkInterface[] netInter = NetworkInterface.GetAllNetworkInterfaces();
			foreach (var itInter in netInter)
			{ // 인터넷 혹은 망에 연결된 것만 남김
				if (itInter.OperationalStatus != OperationalStatus.Up) continue;
				// 유선(Ethernet)과 무선(Wireless80211)만 남김 
				if (itInter.NetworkInterfaceType != NetworkInterfaceType.Ethernet && itInter.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
					continue;
				var unicast = itInter.GetIPProperties().UnicastAddresses;
				foreach (var itUnicast in unicast)
				{
					// Ip v6는 걸러냄 
					if (itUnicast.IPv4Mask.ToString() == "0.0.0.0") continue;

					Console.WriteLine("\tIp Address : {0}", itUnicast.Address);
					Console.WriteLine("\tSubnet Mask : {0}\n", itUnicast.IPv4Mask);
				}
			}
		}
	}
}
