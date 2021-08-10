using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;

namespace Root_JEDI_Sorter.Module
{
    public class Stage : NotifyProperty
    {
        #region ToolBox
        public DIO_I4O m_dioAlignY;
        public DIO_I2O m_dioAlignX;
        public DIO_Is m_diCheck;
        public string m_sGroup = ""; 
        public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
        {
            toolBox.GetDIO(ref m_dioAlignX, module, m_sGroup + "AlignX", "Off", "Align");
            toolBox.GetDIO(ref m_dioAlignY, module, m_sGroup + "AlignY", "Off", "Align");
            toolBox.GetDIO(ref m_diCheck, module, m_sGroup + "Check", new string[2] { "0", "1" });
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

        #region InfoTray
        InfoTray _infoTray = null; 
        public InfoTray p_infoTray
        {
            get { return _infoTray; }
            set
            {
                _infoTray = value;
                OnPropertyChanged(); 
            }
        }
        InfoTray _inspectTray = null;
        public InfoTray p_inspectTray
        {
            get { return _inspectTray; }
            set
            {
                _inspectTray = value;
                OnPropertyChanged();
            }
        }

        public void SetEmpty(string sID)
        {
            p_infoTray = new InfoTray(sID);
            p_infoTray.SetEmpty(); 
        }
        #endregion

        public void Reset()
        {
            p_infoTray = null;
            p_inspectTray = null;
        }

        public string p_id { get; set; }
        public Stage(string id, string sGroup = "")
        {
            p_id = id;
            m_sGroup = sGroup; 
        }

        public void ThreadStop()
        {
        }
    }
}
