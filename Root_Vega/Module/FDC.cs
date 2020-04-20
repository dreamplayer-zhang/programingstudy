using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        #region Module
        public enum eUnit
        {
            None,
            KPA,
            MPA,
            Temp,
            Voltage,
        }

        public class Module : NotifyProperty
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

            eUnit m_eUnit = eUnit.None; 
            int m_nDigit = 2;
            int[] m_aLimit = new int[2] { 0, 0 };

            double _fValue = 0; 
            public double p_fValue
            { 
                get { return _fValue; }
                set
                {
                    _fValue = value;
                    OnPropertyChanged(); 
                }
            }

            public void RunTree(Tree tree)
            {

            }
        }
        #endregion
    }
}
