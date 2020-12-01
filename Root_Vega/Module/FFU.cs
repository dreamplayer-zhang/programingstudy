using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Root_Vega.Module
{
    public class FFU : ModuleBase
    {
        #region ToolBox
        public Modbus m_modbus;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_modbus, this, "Modbus"); 
        }
        #endregion

        #region Unit
        public class Unit : NotifyProperty
        {
            #region Fan
            const int c_lFan = 32;
            List<int> m_aFanRPM = new List<int>();
            List<int> p_aFanRPM
			{
                get { return m_aFanRPM; }
                set 
                {
                    if (m_aFanRPM == value) return;
                    m_aFanRPM = value;
                    OnPropertyChanged();
                }
			}
            List<int> m_aFanSet = new List<int>();
            List<int> p_aFanSet
            {
                get { return m_aFanSet; }
                set
                {
                    if (m_aFanSet == value) return;
                    m_aFanSet = value;
                    OnPropertyChanged();
                }
            }
            List<int> m_aFanState = new List<int>();
            List<int> p_aFanState
            {
                get { return m_aFanState; }
                set
                {
                    if (m_aFanState == value) return;
                    m_aFanState = value;
                    OnPropertyChanged();
                }
            }
            List<int> m_aFanReset = new List<int>();
            List<int> p_aFanReset
            {
                get { return m_aFanReset; }
                set
                {
                    if (m_aFanReset == value) return;
                    m_aFanReset = value;
                    OnPropertyChanged();
                }
            }
            List<int> m_aFanPressure = new List<int>();
            List<int> p_aFanPressure
            {
                get { return m_aFanPressure; }
                set
                {
                    if (m_aFanPressure == value) return;
                    m_aFanPressure = value;
                    OnPropertyChanged();
                }
            }

            public class Fan : ModuleBase
            {
                public int p_fFanRPM 
                {
                    get 
                    { 
                        return (m_unit.m_aFanRPM[m_nID]<0) ? 0: m_unit.m_aFanRPM[m_nID]; 
                    } 
                    set
					{
						if (m_unit.m_aFanRPM[m_nID] == value) return;
						m_unit.m_aFanRPM[m_nID] = value;
						OnPropertyChanged();
					}
                }
                public int p_fFanState 
                { 
                    get 
                    {
                        return m_unit.m_aFanState[m_nID]; 
                    }
                    set
                    {
                        if (m_unit.m_aFanState[m_nID] == value) return;
                        m_unit.m_aFanState[m_nID] = value;
                        OnPropertyChanged();
                    }
                }
                public int p_fFanPressure 
                { 
                    get 
                    { 
                        return m_unit.m_aFanPressure[m_nID]; 
                    }
                    set
                    {
                        if (m_unit.m_aFanPressure[m_nID] == value) return;
                        m_unit.m_aFanPressure[m_nID] = value;
                        OnPropertyChanged();
                    }
                }
                public int p_fFanSet
                {
                    get { return m_unit.m_aFanSet[m_nID]; }
                    set
                    {
                        if (m_unit.m_aFanSet[m_nID] == value) return;
                        m_unit.m_aFanSet[m_nID] = value;
                        m_unit.m_bInvalidSet = true; 
                        OnPropertyChanged();
                    }
                }

                public bool p_bRunOK 
                { 
                    get 
                    {
                        if (m_log != null)
                        {
                            if (p_fFanState == 0) { p_sInfo = "Fan Data is not corret"; }
                        }
                        return ((p_fFanState & 0x0001) == 0); 
                    } 
                }
                public bool p_bPressureOK 
                {
                    get 
                    {
                        if (m_log != null)
                        {
                            if (p_fFanState == 0) { p_sInfo = "Fan Data is not corret"; }
                        }
                        return ((p_fFanState & 0x0002) == 0); 
                    } 
                }
                public bool p_bRPMHigh
                {
                    get
                    {
                        if (m_log != null)
                        {
                            if (p_fFanState == 0) { p_sInfo = "Fan Data is not corret"; }
                        }
                        return ((p_fFanState & 0x0004) == 0);
                    }
                }
                public bool p_bRPMLow
                {
                    get
                    {
                        if (m_log != null)
                        {
                            if (p_fFanState == 0) { p_sInfo = "Fan Data is not corret"; }
                        }
                        return ((p_fFanState & 0x0008) == 0);
                    }
                }
                public bool p_bPressureLow
                {
                    get
                    {
                        if (m_log != null)
                        {
                            if (p_fFanState == 0) { p_sInfo = "Fan Data is not corret"; }
                        }
                        return ((p_fFanState & 0x0010) == 0);
                    }
                }
                public bool p_bTimeover
                {
                    get
                    {
                        if (m_log != null)
                        {
                            if (p_fFanState == 0) { p_sInfo = "Fan Data is not corret"; }
                        }
                        return ((p_fFanState & 0x0020) == 0);
                    }
                }
                public bool p_bCommOK
                {
                    get
                    {
                        if (m_log != null)
                        {
                            if (p_fFanState == 0) { p_sInfo = "Fan Data is not corret"; }
                        }
                        return ((p_fFanState & 0x0040) == 0);
                    }
                }

                public void RunTree(Tree tree)
                {
                    p_sFan = tree.Set(p_sFan, p_sFan, "Fan ID", "Fan ID");
                    p_fFanSet = tree.Set(p_fFanSet, p_fFanSet, "Set", "Fan Set Value (RPM or Pressure)");
                }

                public string m_id;
                public string p_sFan { get; set; }
                int m_nID;
                Unit m_unit;
                public Fan(Unit unit, int nID)
                {
                    m_unit = unit;
                    m_nID = nID;
                    m_id = "Fan" + nID.ToString("00");
                    p_sFan = m_id;
                }
            }
            public ObservableCollection<Fan> m_aFan = new ObservableCollection<Fan>();
            public ObservableCollection<Fan> p_aFan
			{
                get { return m_aFan; }
                set 
                {
                    if (m_aFan == value) return;
                    m_aFan = value;
                    OnPropertyChanged();
                }
			}

            public void InitFan()
            {
                for (int n = 0; n < c_lFan; n++)
                {
                    p_aFanRPM.Add(0);
                    m_aTempRPM.Add(0);

                    p_aFanSet.Add(0);
                    m_aTempSet.Add(0);

                    p_aFanState.Add(0);
                    m_aTempState.Add(0);

                    p_aFanReset.Add(0);
                    m_aTempReset.Add(0);

                    p_aFanPressure.Add(0);
                    m_aTempPressure.Add(0);
                    m_aFan.Add(new Fan(this, n));
                }
            }

            int m_lFan = 2; 
            public void RunTreeUnit(Tree tree)
            {
                p_sUnit = tree.Set(p_sUnit, p_sUnit, "Unit ID", "Unit ID");
                m_idUnit = (byte)tree.Set((int)m_idUnit, (int)m_idUnit, "Unit Address", "Unit Address");
                m_lFan = tree.Set(m_lFan, m_lFan, "Fan Count", "Fan Count"); 
                for (int n = 0; n < m_lFan; n++) m_aFan[n].RunTree(tree.GetTree(m_aFan[n].m_id));
            }
            #endregion
            List<int> m_aTempRPM = new List<int>();
            List<int> m_aTempState = new List<int>();
            List<int> m_aTempPressure = new List<int>();
            List<int> m_aTempSet = new List<int>();
            List<int> m_aTempReset = new List<int>();
            public void RunThreadFan()
            {
                Thread.Sleep(10);
				m_FFU.m_modbus.ReadHoldingRegister(m_idUnit, 0, ref m_aTempRPM);
                p_aFanRPM = m_aTempRPM;
                Thread.Sleep(10);
				m_FFU.m_modbus.ReadHoldingRegister(m_idUnit, 64, ref m_aTempState);
                p_aFanState = m_aTempState;
				Thread.Sleep(10);
				m_FFU.m_modbus.ReadHoldingRegister(m_idUnit, 128, ref m_aTempPressure);
                p_aFanPressure = m_aTempPressure;
                if (m_bInvalidSet)
				{
					Thread.Sleep(10);
					m_FFU.m_modbus.WriteHoldingRegister(m_idUnit, 32, m_aTempSet);
                    p_aFanSet = m_aTempSet;
				}
				if (m_FFU.m_bResetFan)
				{
					Thread.Sleep(10);
					for (int n = 0; n < c_lFan; n++) m_aFanReset[n] = 1;
					m_FFU.m_modbus.WriteHoldingRegister(m_idUnit, 96, m_aTempReset);
                    p_aFanReset = m_aTempReset;
					m_FFU.m_bResetFan = false;
				}
			}

            bool m_bInvalidSet = false;
            FFU m_FFU;
            int m_nID;
            byte m_idUnit = 0; 
            public string m_id; 
            public string p_sUnit { get; set; }
            public Unit(FFU FFU, int nID)
            {
                m_FFU = FFU;
                m_nID = nID;
                m_id = "Unit" + nID.ToString();
                p_sUnit = m_id; 
            }
        }
        List<Unit> m_aUnit = new List<Unit>();
		public List<Unit> p_aUnit
		{
			get
			{
				return m_aUnit;
			}
			set
			{
				if (m_aUnit == value) return;
				m_aUnit = value;
				OnPropertyChanged();
			}
		}
		public int m_nUnit = 1; 
        void RunTreeUnit(Tree tree)
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

        void RunThreadFan()
        {
            m_bThreadFan = true;
            Thread.Sleep(3000);
            while (m_bThreadFan)
            {
                Thread.Sleep(10);
                foreach (Unit unit in p_aUnit) unit.RunThreadFan(); 
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

        public FFU(string id, IEngineer engineer)
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
