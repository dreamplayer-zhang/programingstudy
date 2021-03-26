using RootTools;
using RootTools.Comm;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace Root_CAMELLIA.Module
{
	public class Module_FFU : ModuleBase
	{
		#region ToolBox
		public Modbus m_modbus;
		public override void GetTools(bool bInit)
		{
			p_sInfo = m_toolBox.GetComm(ref m_modbus, this, "Modbus");
		}
		#endregion

		#region Unit
		public class Unit : NotifyProperty
		{
			#region Fan
			public class Fan : NotifyProperty
			{
				public RPoint m_mmLimit = new RPoint();
				public int m_nSet = 0;
				int _nRPM = 0;
				public int p_nRPM
				{
					get { return _nRPM; }
					set
					{
						if (_nRPM == value) return;
						if (m_nSet == 0)
						{
							m_alidSetted_RPMLow.Run(m_mmLimit.X > _nRPM, "FFU RPM Lower than Low Limit.");
							m_alidSetted_RPMHigh.Run(m_mmLimit.Y < _nRPM, "FFU RPM Higher than High Limit.");
						}
						else if (m_nSet == 1)
						{
							m_alidSetted_PreLow.Run(m_mmLimit.X > _nRPM, "FFU Pressure Lower than Low Limit.");
							m_alidSetted_PreHigh.Run(m_mmLimit.Y < _nRPM, "FFU Pressure Higher than High Limit.");
						}

						_nRPM = value;
						OnPropertyChanged();
					}
				}

				double _fPressure = 0;
				public double p_fPressure
				{
					get { return _fPressure; }
					set
					{
						if (_fPressure == value) return;
						_fPressure = value;
						OnPropertyChanged();
					}
				}

				#region ALID
				ALID m_alidFan;
				ALID m_alidRPMHigh;
				ALID m_alidRPMLow;
				ALID m_alidPressureSensor;
				ALID m_alidPressureHigh;
				ALID m_alidPressureLow;

				ALID m_alidSetted_RPMHigh;
				ALID m_alidSetted_RPMLow;
				ALID m_alidSetted_PreHigh;
				ALID m_alidSetted_PreLow;
				public void InitALID(Module_FFU FFU)
				{
					GAF GAF = FFU.m_gaf;
					m_alidFan = GAF.GetALID(FFU, m_id + " : Fan Error", "Fan Run Error");
					m_alidRPMHigh = GAF.GetALID(FFU, m_id + " : RPM High", "Fan RPM too High");
					m_alidRPMLow = GAF.GetALID(FFU, m_id + " : RPM Low", "Fan RPM too Low");
					m_alidPressureSensor = GAF.GetALID(FFU, m_id + " : Pressure Sensor", "Pressure Check Sensor Error");
					m_alidPressureHigh = GAF.GetALID(FFU, m_id + " : Pressure High", "Pressure too High");
					m_alidPressureLow = GAF.GetALID(FFU, m_id + " : Pressure Low", "Pressure too Low");
					m_alidSetted_RPMHigh = GAF.GetALID(FFU, m_id + " : RPM High", "RPM Higher than Limit");
					m_alidSetted_RPMLow = GAF.GetALID(FFU, m_id + " : RPM Low", "RPM Lower than Limit");
					m_alidSetted_PreHigh = GAF.GetALID(FFU, m_id + " : Pressure High", "Pressure Higher than Limit");
					m_alidSetted_PreLow = GAF.GetALID(FFU, m_id + " : Pressure Low", "Pressure Lower than Limit");
				}
				#endregion

				#region State
				int _nState = 0;
				public int p_nState
				{
					get { return _nState; }
					set
					{
						if (_nState == value) return;
						_nState = value;
						OnPropertyChanged();
						p_bFanError = (value & 0x0001) != 0;
						p_bPressureSensorError = (value & 0x0002) != 0;
						p_bRPMHigh = (value & 0x0004) != 0;
						p_bRPMLow = (value & 0x0008) != 0;
						p_bPressureHigh = (value & 0x0010) != 0;
						p_bPressureLow = (value & 0x0020) != 0;
						p_bTimeover = (value & 0x0040) != 0;
						p_bCommunicationError = (value & 0x0080) != 0;
						p_bRun = (value & 0x0200) != 0;
					}
				}

				bool _bFanError = false;
				public bool p_bFanError
				{
					get { return _bFanError; }
					set
					{
						if (_bFanError == value) return;
						_bFanError = value;
						OnPropertyChanged();
						m_alidFan.p_bSet = value;
					}
				}

				bool _bPressureSensorError = false;
				public bool p_bPressureSensorError
				{
					get { return _bPressureSensorError; }
					set
					{
						if (_bPressureSensorError == value) return;
						_bPressureSensorError = value;
						OnPropertyChanged();
						m_alidPressureSensor.p_bSet = value;
					}
				}

				bool _bRPMHigh = false;
				public bool p_bRPMHigh
				{
					get { return _bRPMHigh; }
					set
					{
						if (_bRPMHigh == value) return;
						_bRPMHigh = value;
						OnPropertyChanged();
						m_alidRPMHigh.p_bSet = value;
					}
				}

				bool _bRPMLow = false;
				public bool p_bRPMLow
				{
					get { return _bRPMLow; }
					set
					{
						if (_bRPMLow == value) return;
						_bRPMLow = value;
						OnPropertyChanged();
						m_alidRPMLow.p_bSet = value;
					}
				}

				bool _bPressureHigh = false;
				public bool p_bPressureHigh
				{
					get { return _bPressureHigh; }
					set
					{
						if (_bPressureHigh == value) return;
						_bPressureHigh = value;
						OnPropertyChanged();
						m_alidPressureHigh.p_bSet = value;
					}
				}

				bool _bPressureLow = false;
				public bool p_bPressureLow
				{
					get { return _bPressureLow; }
					set
					{
						if (_bPressureLow == value) return;
						_bPressureLow = value;
						OnPropertyChanged();
						m_alidPressureLow.p_bSet = value;
					}
				}

				bool _bTimeover = false;
				public bool p_bTimeover
				{
					get { return _bTimeover; }
					set
					{
						if (_bTimeover == value) return;
						_bTimeover = value;
						OnPropertyChanged();
					}
				}

				bool _bCommunicationError = false;
				public bool p_bCommunicationError
				{
					get { return _bCommunicationError; }
					set
					{
						if (_bCommunicationError == value) return;
						_bCommunicationError = value;
						OnPropertyChanged();
					}
				}

				bool _bRun = false;
				public bool p_bRun
				{
					get { return _bRun; }
					set
					{
						if (_bRun == value) return;
						_bRun = value;
						OnPropertyChanged();
					}
				}
				#endregion


				public bool p_IsUpdate { get; set; } = false;

				public void RunTree(Tree tree)
				{
					p_sFan = tree.Set(p_sFan, p_sFan, "Fan ID", "Fan ID");
					m_nSet = tree.Set(m_nSet, m_nSet, "Set", "Fan Set Value (RPM) or Pressure (0.1pa)");
					m_mmLimit = tree.Set(m_mmLimit, m_mmLimit, "Limit", "FFU Lower & Upper Limit");

					p_IsUpdate = true;
				}

				public string m_id;
				string _sFan = "";
				public string p_sFan 
				{
                    get { return _sFan; }
                    set
                    {
						if (_sFan == value) return;
						_sFan = value;
						OnPropertyChanged();
                    }
				}
				public Fan(Module_FFU FFU, string id)
				{
					m_id = id;
					InitALID(FFU);
				}
			}
			#endregion
			public List<Fan> m_aFan = new List<Fan>();
			public List<Fan> p_aFan
			{
				get { return m_aFan; }
				set
				{
					if (m_aFan == value) return;
					m_aFan = value;
					OnPropertyChanged();
				}
			}
			public List<Temp> m_aTemp = new List<Temp>();
			public List<Temp> p_aTemp
			{
				get { return m_aTemp; }
				set
				{
					if (m_aTemp == value) return;
					m_aTemp = value;
					OnPropertyChanged();
				}
			}
			public List<Humidity> m_aHumidity = new List<Humidity>();
			public List<Humidity> p_aHumidity
            {
                get { return m_aHumidity; }
                set
                {
					if (m_aHumidity == value) return;
					m_aHumidity = value;
					OnPropertyChanged();
                }
            }

			List<int> m_aFanState = new List<int>();
			List<int> m_aFanRPM = new List<int>();
			List<int> m_aFanPressure = new List<int>();
			List<int> m_aFanReset = new List<int>();
			List<int> m_aFanRPMSet = new List<int>();

			List<int> m_aTempValue = new List<int>();
			List<int> m_aHumidityValue = new List<int>();

			public void InitFan()
			{
				while (m_aFan.Count < m_lFan) m_aFan.Add(new Fan(m_FFU, m_id + ".Fan" + m_aFan.Count.ToString("00")));
				InitListFan(m_aFanState);
				InitListFan(m_aFanRPM);
				InitListFan(m_aFanPressure);
				InitListFan(m_aFanReset);
				InitListFan(m_aFanRPMSet);

                while (m_aTemp.Count < m_lTemp) m_aTemp.Add(new Temp(m_FFU, m_id + ".Temp" + m_aTemp.Count.ToString("00")));
                InitListTemp(m_aTempValue);

                while (m_aHumidity.Count < m_lHumidity) m_aHumidity.Add(new Humidity(m_FFU, m_id + ".Humidity" + m_aHumidity.Count.ToString("00")));
                InitListHumidity(m_aHumidityValue);
            }

			void InitListFan(List<int> aList)
			{
				while (aList.Count > m_lFan) aList.RemoveAt(aList.Count - 1);
				while (aList.Count < m_lFan) aList.Add(0);
			}
			void InitListTemp(List<int> aList)
            {
				int nTotal = 32 * m_lTemp;
				aList.Clear();
				for (int i = 0; i < nTotal; i++) aList.Add(0);
            }

			void InitListHumidity(List<int> aList)
            {
				int nTotal = 32 * m_lTemp;
				aList.Clear();
				for (int i = 0; i < nTotal; i++) aList.Add(0);
			}

			int m_lFan = 2;
			int m_lTemp = 1;
			int m_lHumidity = 1;
			public void RunTreeUnit(Tree tree)
			{
				p_sUnit = tree.Set(p_sUnit, p_sUnit, "Unit ID", "Unit ID");
				m_idUnit = (byte)tree.Set((int)m_idUnit, (int)m_idUnit, "Unit Address", "Unit Address");
				m_lFan = tree.Set(m_lFan, m_lFan, "Fan Count", "Fan Count");
				m_lTemp = tree.Set(m_lTemp, m_lTemp, "Temp Count", "Temp Count");
				m_lHumidity = tree.Set(m_lHumidity, m_lHumidity, "Humidity Count", "Humidity Count");
				InitFan();
				for (int n = 0; n < m_lFan; n++) m_aFan[n].RunTree(tree.GetTree(m_aFan[n].m_id));
				for (int n = 0; n < m_lTemp; n++) m_aTemp[n].RunTree(tree.GetTree(m_aTemp[n].m_id));
				for (int n = 0; n < m_lHumidity; n++) m_aHumidity[n].RunTree(tree.GetTree(m_aHumidity[n].m_id));
			}
            

            #region Temp
            public class Temp : NotifyProperty
            {
				public RPoint m_mmLimit { get; set; } = new RPoint();
				int _nTemp;
				public int p_nTemp
				{
					get { return _nTemp; }
					set
					{
						if (_nTemp == value) return;
						m_alidTempLow.Run(m_mmLimit.X > _nTemp, "FFU Temp Lower than Low Limit.");
						m_alidTempHigh.Run(m_mmLimit.Y < _nTemp, "FFU Temp Higher than High Limit.");
						_nTemp = value;
						OnPropertyChanged();
					}
				}
				#region ALID
				ALID m_alidTempHigh;
				ALID m_alidTempLow;
				public void InitALID(Module_FFU FFU)
				{
					GAF GAF = FFU.m_gaf;
					m_alidTempHigh = GAF.GetALID(FFU, m_id + " : Temp High", "Temp too High");
					m_alidTempLow = GAF.GetALID(FFU, m_id + " : Temp Low", "Temp RPM too Low");
				}
				#endregion
				string _sTemp = "";
				public string p_sTemp 
				{
                    get { return _sTemp; }
                    set
                    {
						if (_sTemp == value) return;
						_sTemp = value;
						OnPropertyChanged();
                    }
				}
				public void RunTree(Tree tree)
				{
					p_sTemp = tree.Set(p_sTemp, p_sTemp, "Temp ID", "Temp ID");
					m_mmLimit = tree.Set(m_mmLimit, m_mmLimit, "Limit", "Temp Lower & Upper Limit");
				}
				public string m_id;
				public Temp(Module_FFU FFU, string id)
				{
					m_id = id;
					InitALID(FFU);
				}
			}

			#endregion

			#region Humidity
			public class Humidity : NotifyProperty
			{
				public RPoint m_mmLimit { get; set;} = new RPoint();
				int _nHumidity;
				public int p_nHumidity
				{
					get { return _nHumidity; }
					set
					{
						if (_nHumidity == value) return;
						m_alidHumidityLow.Run(m_mmLimit.X > _nHumidity, "FFU Humidity Lower than Low Limit.");
						m_alidHumidityHigh.Run(m_mmLimit.Y < _nHumidity, "FFU Humidity Higher than High Limit.");
						_nHumidity = value;
						OnPropertyChanged();
					}
				}
				#region ALID
				ALID m_alidHumidityHigh;
				ALID m_alidHumidityLow;
				public void InitALID(Module_FFU FFU)
				{
					GAF GAF = FFU.m_gaf;
					m_alidHumidityHigh = GAF.GetALID(FFU, m_id + " : Humidity High", "Humidity too High");
					m_alidHumidityLow = GAF.GetALID(FFU, m_id + " : Humidity Low", "Humidity RPM too Low");
				}
				#endregion
				string _sHumidity = "";
				public string p_sHumidity 
				{
                    get { return _sHumidity; }
                    set
                    {
						if (_sHumidity == value) return;
						_sHumidity = value;
						OnPropertyChanged();
                    }
				}
				public void RunTree(Tree tree)
				{
					p_sHumidity = tree.Set(p_sHumidity, p_sHumidity, "Humidity ID", "Temp ID");
					m_mmLimit = tree.Set(m_mmLimit, m_mmLimit, "Limit", "Humidity Lower & Upper Limit");
                }
				public string m_id;
				public Humidity(Module_FFU FFU, string id)
				{
					m_id = id;
					InitALID(FFU);
				}
			}

			#endregion

			#region Run Thread
			public void RunThreadFan()
			{
				try
				{
					Thread.Sleep(10);
					m_FFU.m_modbus.ReadHoldingRegister(m_idUnit, 64, m_aFanState);
					for (int n = 0; n < m_lFan; n++) m_aFan[n].p_nState = m_aFanState[n];

					Thread.Sleep(10);
					m_FFU.m_modbus.ReadHoldingRegister(m_idUnit, 0, m_aFanRPM);
					for (int n = 0; n < m_lFan; n++) m_aFan[n].p_nRPM = m_aFan[n].p_bRun ? m_aFanRPM[n] : 0;

					Thread.Sleep(10);
					m_FFU.m_modbus.ReadHoldingRegister(m_idUnit, 128, m_aFanPressure);
					for (int n = 0; n < m_lFan; n++) m_aFan[n].p_fPressure = m_aFan[n].p_bRun ? m_aFanPressure[n] / 10.0 : 0;

					for (int n = 0; n < m_lHumidity; n++)
					{
						Thread.Sleep(10);
						int nHumidity = 0;
						m_FFU.m_modbus.ReadHoldingRegister(m_idUnit, 193 + (32 * n), ref nHumidity);
						m_aHumidity[n].p_nHumidity = nHumidity;
					}

					for (int n = 0; n < m_lTemp; n++)
                    {
						Thread.Sleep(10);
						int nTemp = 0;
						m_FFU.m_modbus.ReadHoldingRegister(m_idUnit, 194 + (32 * n), ref nTemp);
						m_aTemp[n].p_nTemp = nTemp;
					}

                    if (m_FFU.m_bResetFan)
                    {
                        Thread.Sleep(10);
                        for (int n = 0; n < m_lFan; n++) m_aFanReset[n] = 1;
                        m_FFU.m_modbus.WriteHoldingRegister(m_idUnit, 96, m_aFanReset);
                    }

                    if (IsRPMSet())
					{
						Thread.Sleep(10);
						for (int n = 0; n < m_lFan; n++) m_aFanRPMSet[n] = m_aFan[n].m_nSet;
						m_FFU.m_modbus.WriteHoldingRegister(m_idUnit, 32, m_aFanRPMSet);
					}
				}
				catch { }
			}

			bool IsRPMSet()
			{
				for (int n = 0; n < m_lFan; n++)
				{
					if (m_aFan[n].m_nSet != m_aFanRPMSet[n]) return true;
				}
				return false;
			}
			#endregion

			Module_FFU m_FFU;
			byte m_idUnit = 0;
			public string m_id;
			public string p_sUnit { get; set; }
			public Unit(Module_FFU FFU, int nID)
			{
				m_FFU = FFU;
				m_id = "Unit" + nID.ToString();
				p_sUnit = m_id;
			}
		}
		List<Unit> _aUnit = new List<Unit>();
		public List<Unit> p_aUnit
		{
			get { return _aUnit; }
			set
			{
				if (_aUnit == value) return;
				_aUnit = value;
				OnPropertyChanged();
			}
		}
		public int m_nUnit = 1;
		void RunTreeUnit(Tree tree)
		{
			lock (m_csLock)
			{
				m_nUnit = tree.Set(m_nUnit, m_nUnit, "Count", "Unit Count");
				while (p_aUnit.Count < m_nUnit)
				{
					Unit unit = new Unit(this, p_aUnit.Count);
					unit.InitFan();
					p_aUnit.Add(unit);
				}
				for (int n = 0; n < m_nUnit; n++) p_aUnit[n].RunTreeUnit(tree.GetTree(p_aUnit[n].m_id, false));
			}
		}
		#endregion

		#region Thread
		bool m_bThreadFan = false;
		Thread m_threadFan;
		void InitThreadFan()
		{
			if (m_bThreadFan) return;
			m_threadFan = new Thread(new ThreadStart(RunThreadFan));
			m_threadFan.Start();
		}
		static readonly object m_csLock = new object();
		void RunThreadFan()
		{
			m_bThreadFan = true;
			Thread.Sleep(10000);
			while (m_bThreadFan)
			{
				Thread.Sleep(10);

				if (!m_modbus.m_client.Connected)
				{
					Thread.Sleep(1000);
					p_sInfo = m_modbus.Connect();
				}
				else
				{
					lock (m_csLock)
					{
						foreach (Unit unit in p_aUnit) unit.RunThreadFan();
						m_bResetFan = false;
					}
				}
			}
		}
		#endregion

		#region Functions
		public bool m_bResetFan = false;
		public override void Reset()
		{
			base.Reset();
			m_bResetFan = true;
		}
		#endregion

		#region Tree
		public override void RunTree(Tree tree)
		{
			base.RunTree(tree);
			RunTreeUnit(tree.GetTree("Unit", false));
		}
		#endregion

		public Module_FFU(string id, IEngineer engineer)
		{
			p_id = id;
			InitBase(id, engineer);
			InitThreadFan();
		}

		public override void ThreadStop()
		{
			if (m_bThreadFan)
			{
				m_bThreadFan = false;
				m_threadFan.Join();
			}
			base.ThreadStop();
		}
	}
}
