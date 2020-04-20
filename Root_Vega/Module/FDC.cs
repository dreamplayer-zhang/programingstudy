using RootTools;
using RootTools.Comm;
using RootTools.GAFs;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Root_Vega.Module
{
    public class FDC : ModuleBase
    {
        #region ToolBox
        RS232 m_rs232;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_rs232, this, "RS232");
            if (bInit)
            {
                m_rs232.OnRecieve += M_rs232_OnRecieve; 
                m_rs232.p_bConnect = true;
            }
        }
        #endregion

        #region RS232

        private void M_rs232_OnRecieve(string sRead)
        {
            throw new NotImplementedException();
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
            string _id = ""; 
            public string p_id
            { 
                get { return _id; }
                set
                {
                    _id = value;
                    OnPropertyChanged(); 
                }
            }

            eUnit _eUnit = eUnit.None; 
            eUnit p_eUnit
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
            int[] m_aLimit = new int[2] { 0, 0 };
            ALID[] m_alid = new ALID[2] { null, null };

            double _fValue = 0; 
            public double p_fValue
            { 
                get { return _fValue; }
                set
                {
                    _fValue = value / m_fDiv;
                    OnPropertyChanged();
                    m_alid[0].p_bSet = (_fValue < m_aLimit[0]);
                    m_alid[1].p_bSet = (_fValue > m_aLimit[1]); 
                }
            }

            Color _color = Colors.Black; 

            ModuleBase m_module;
            public Data(ModuleBase module)
            {
                m_module = module;
            }

            public void RunTree(Tree tree)
            {
                p_id = tree.Set(p_id, p_id, "Name", "FDC Name");
                p_eUnit = (eUnit)tree.Set(p_eUnit, p_eUnit, "Unit", "FDC Unit");
                m_nDigit = tree.Set(m_nDigit, m_nDigit, "Digit", "FDC Decimal Point");
                m_aLimit[0] = tree.Set(m_aLimit[0], m_aLimit[0], "Lower Limit", "FDC Lower Limit");
                m_aLimit[1] = tree.Set(m_aLimit[1], m_aLimit[1], "Upper Limit", "FDC Upper Limit");
                m_fDiv = 1;
                for (int n = 0; n < m_nDigit; n++) m_fDiv *= 10; 
                if (m_alid[0] == null)
                {
                    m_alid[0] = m_module.m_gaf.GetALID(m_module, p_id + ".LowerLimit", "FDC Lower Limit");
                    m_alid[0].p_sMsg = "FDC Value Smaller then Lower Limit"; 
                    m_alid[1] = m_module.m_gaf.GetALID(m_module, p_id + ".UpperLimit", "FDC Upper Limit");
                    m_alid[1].p_sMsg = "FDC Value Larger then Upper Limit";
                }
                m_alid[0].p_id = p_id + ".LowerLimit";
                m_alid[1].p_id = p_id + ".UpperLimit"; 
            }
        }
        #endregion

        #region List Data
        public ObservableCollection<Data> m_aData = new ObservableCollection<Data>();

        #endregion
    }
}
