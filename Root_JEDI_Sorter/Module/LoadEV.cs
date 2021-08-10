using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using System;

namespace Root_JEDI_Sorter.Module
{
    public class LoadEV : NotifyProperty
    {
        #region ToolBox
        public Axis m_axis;
        public DIO_I8O2 m_dioGrip;
        public DIO_Is m_diCheck;
        public DIO_I m_diFull;
        public DIO_I m_diProtrude;
        public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
        {
            toolBox.GetAxis(ref m_axis, module, "Elevator");
            toolBox.GetDIO(ref m_dioGrip, module, "Grip", "Off", "Grip");
            toolBox.GetDIO(ref m_diCheck, module, "Check", new string[2] { "0", "1"});
            toolBox.GetDIO(ref m_diFull, module, "Full");
            toolBox.GetDIO(ref m_diProtrude, module, "Protrude");
            if (bInit)
            {
                InitPosition();
                RunGrip(true); 
            }
        }
        #endregion

        #region Axis
        public enum ePos
        {
            Up,
            Grip,
            Stage,
            Down,
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
        }

        public string RunMove(ePos ePos, bool bWait = true)
        {
            if (IsProtrude()) return "Check Tray Protrude";
            if (IsFull()) return "Check Tray Full";
            m_axis.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }
        #endregion

        #region DIO
        public string RunGrip(bool bGrip, bool bWait = true)
        {
            m_dioGrip.Write(bGrip);
            return bWait ? m_dioGrip.WaitDone() : "OK";
        }

        public bool IsCheck(bool bCheck)
        {
            if (m_diCheck.ReadDI(0) != bCheck) return false;
            if (m_diCheck.ReadDI(1) != bCheck) return false;
            return true; 
        }

        public bool IsFull()
        {
            return m_diFull.p_bIn; 
        }

        public bool IsProtrude()
        {
            return m_diProtrude.p_bIn; 
        }
        #endregion

        #region Run
        public string RunLoad()
        {
            try
            {
                if (Run(RunGrip(true))) return m_sInfo;
                if (Run(RunMove(ePos.Up))) return m_sInfo;
                if (Run(RunGrip(false))) return m_sInfo;
                if (Run(RunMove(ePos.Grip))) return m_sInfo;
                if (Run(RunGrip(true))) return m_sInfo;
            }
            finally 
            { 
                RunGrip(true);
                RunMove(ePos.Grip);
            }
            return "OK"; 
        }

        public string RunUnload()
        {
            try
            {
                if (Run(RunGrip(true))) return m_sInfo;
                if (Run(RunMove(ePos.Grip))) return m_sInfo;
                if (Run(RunGrip(false))) return m_sInfo;
                if (Run(RunMove(ePos.Up))) return m_sInfo;
                if (Run(RunGrip(true))) return m_sInfo;
                if (Run(RunMove(ePos.Grip))) return m_sInfo;
            }
            finally 
            { 
                RunGrip(true);
                RunMove(ePos.Grip); 
            }
            return "OK";
        }

        string m_sInfo = "OK";
        bool Run(string sRun)
        {
            m_sInfo = sRun;
            return (sRun == "OK");
        }
        #endregion

        public string p_id { get; set; }
        public LoadEV(string id)
        {
            p_id = id; 
        }

        public void ThreadStop()
        {
        }
    }
}
