using RootTools;
using RootTools.Comm;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Media;

namespace Root_Vega.Module
{
    public class FDC : ModuleBase
    {
        #region ToolBox
        public Modbus m_modbus; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_modbus, this, "Modbus");
        }
        #endregion

        #region Data
        public enum eUnit
        {
            None,
            KPA,
            MPA,
            Temp,
            Voltage,
        }

        public class Data : NotifyProperty
        {
            public string p_id { get; set; }

            eUnit _eUnit = eUnit.None;
            public eUnit p_eUnit
            {
                get { return _eUnit; }
                set
                {
                    if (_eUnit == value) return;
                    _eUnit = value;
                    OnPropertyChanged();
                }
            }

            int m_nDigit = 2;
            double m_fDiv = 100;
            public int[] m_aLimit = new int[2] { 0, 0 };
            ALID[] m_alid = new ALID[2] { null, null };
            SVID m_svValue;
            public double p_fValue
            {
                get { return (m_svValue.p_value != null) ? m_svValue.p_value : 0; }
                set
                {
                    if ((m_svValue.p_value != null) && (m_svValue.p_value == value)) return; 
                    m_svValue.p_value = value;
                    OnPropertyChanged();
                    m_alid[0].p_bSet = (m_svValue.p_value < m_aLimit[0]);
                    m_alid[1].p_bSet = (m_svValue.p_value > m_aLimit[1]);
                    double dValue = Math.Abs(m_svValue.p_value - (m_aLimit[0] + m_aLimit[1]) / 2);
                    int nRed = (int)(500 * dValue / (m_aLimit[1] - m_aLimit[0]));
                    if (nRed > 250) nRed = 250;
                    p_color = Color.FromRgb((byte)nRed, (byte)(250 - nRed), 0); 
                }
            }

            Color _color = Colors.Green;
            public Color p_color
            {
                get { return _color; }
                set
                {
                    if (_color == value) return;
                    _color = value;
                    OnPropertyChanged(); 
                }
            }

            int m_nUnitID = 0;
            int m_nAddress = 0; 
            public void ReadInputRegister(Modbus modbus)
            {
                int nValue = (int)(p_fValue * m_fDiv);
                modbus.ReadInputRegister((byte)m_nUnitID, m_nAddress, ref nValue);
                p_fValue = nValue / m_fDiv; 
            }

            public void RunTree(Tree tree, int module_number)
            {
                p_id = tree.Set(p_id, p_id, "ID." + module_number.ToString("00"), "FDC Module Name");

                p_eUnit = (eUnit)tree.Set(p_eUnit, p_eUnit, "Unit", "FDC Unit");
                m_nDigit = tree.Set(m_nDigit, m_nDigit, "Digit", "FDC Decimal Point");
                m_fDiv = 1;
                for (int n = 0; n < m_nDigit; n++) m_fDiv *= 10;
                m_aLimit[0] = tree.Set(m_aLimit[0], m_aLimit[0], "Lower Limit", "FDC Lower Limit");
                m_aLimit[1] = tree.Set(m_aLimit[1], m_aLimit[1], "Upper Limit", "FDC Upper Limit");
                if (m_alid[0] == null)
                {
                    m_alid[0] = m_module.m_gaf.GetALID(m_module, ".LowerLimit", "FDC Lower Limit");
                    m_alid[0].p_sMsg = "FDC Value Smaller then Lower Limit";
                    m_alid[1] = m_module.m_gaf.GetALID(m_module, ".UpperLimit", "FDC Upper Limit");
                    m_alid[1].p_sMsg = "FDC Value Larger then Upper Limit";
                    m_svValue = m_module.m_gaf.GetSVID(m_module, p_id); 
                }
                m_alid[0].p_id = "LowerLimit";
                m_alid[1].p_id = "UpperLimit";
                m_nUnitID = tree.Set(m_nUnitID, m_nUnitID, "Comm UnitID", "RS485 UnitID");
                m_nAddress = tree.Set(m_nAddress, m_nAddress, "Comm Address", "RS485 Address");
            }

            ModuleBase m_module;
            public Data(ModuleBase module, string id)
            {
                m_module = module;
                p_id = id;
            }
        }
        #endregion

        #region List Data
        public ObservableCollection<Data> m_aData = new ObservableCollection<Data>();

        public int p_lData
        {
            get { return m_aData.Count; }
            set
            {
                if (m_aData.Count == value) return;
                while (m_aData.Count < value) m_aData.Add(new Data(this, m_aData.Count.ToString()));
                while (m_aData.Count > value) m_aData.RemoveAt(m_aData.Count - 1);
            }
        }
        
        void RunTreeData(Tree tree)
        {
            int module_number = 0;
            p_lData = tree.Set(p_lData, p_lData, "Count", "FDC Module Count");
            for (int n = 0; n < m_aData.Count; n++)
            {
                Data data = m_aData[n];
            }
            foreach (Data data in m_aData)
            {
                module_number++;
                data.RunTree(tree.GetTree(data.p_id), module_number); 
            }
        }
        #endregion

        #region Check Thread
        int m_iData = 0; 
        protected override void RunThread()
        {
            base.RunThread();
            Thread.Sleep(m_msInterval);
            if (m_aData.Count > m_iData)
            {
                m_aData[m_iData].ReadInputRegister(m_modbus); 
                m_iData = (m_iData + 1) % m_aData.Count;
            }
        }

        int m_msInterval = 100; 
        void RunTreeThread(Tree tree)
        {
            m_msInterval = tree.Set(m_msInterval, m_msInterval, "Interval", "Check Interval (ms)"); 
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            RunTreeData(tree.GetTree("FDC Module", false));
            RunTreeThread(tree.GetTree("Thread", false)); 
        }
        #endregion

        public FDC(string id, IEngineer engineer)
        {
            p_id = id;
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
