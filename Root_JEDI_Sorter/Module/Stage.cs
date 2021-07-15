using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_JEDI_Sorter.Module
{
    public class Stage : NotifyProperty
    {
        #region ToolBox
        public DIO_I4O m_dioAlignY;
        public DIO_I2O m_dioAlignX;
        public DIO_Is m_diCheck;
        public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
        {
            toolBox.GetDIO(ref m_dioAlignX, module, p_id + ".AlignX", "Off", "Align");
            toolBox.GetDIO(ref m_dioAlignY, module, p_id + ".AlignY", "Off", "Align");
            toolBox.GetDIO(ref m_diCheck, module, p_id + ".Check", new string[2] { "0", "1" });
        }
        #endregion

        #region DIO
        public string RunAlign(bool bAlign, bool bWait = true)
        {
            m_dioAlignX.Write(bAlign);
            m_dioAlignY.Write(bAlign);
            if (bWait == false) return "OK";
            string sX = m_dioAlignX.WaitDone();
            string sY = m_dioAlignY.WaitDone();
            if ((sX == "OK") && (sY == "OK")) return "OK"; 
            return sX + ", " + sY;
        }

        public bool IsCheck(bool bCheck)
        {
            if (m_diCheck.ReadDI(0) != bCheck) return false;
            if (m_diCheck.ReadDI(1) != bCheck) return false;
            return true;
        }
        #endregion

        public string p_id { get; set; }
        public Stage(string id)
        {
            p_id = id; 
        }

        public void ThreadStop()
        {

        }
    }
}
