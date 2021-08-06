using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_JEDI_Sorter.Module
{
    public class UnloadEV : NotifyProperty
    {
        #region ToolBox
        public Axis m_axis;
        public DIO_Is m_diCheck;
        public DIO_I m_diFull;
        public DIO_I m_diProtrude;
        public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
        {
            toolBox.GetAxis(ref m_axis, module, "Elevator");
            toolBox.GetDIO(ref m_diCheck, module, "Check", new string[2] { "0", "1" });
            toolBox.GetDIO(ref m_diFull, module, "Full");
            toolBox.GetDIO(ref m_diProtrude, module, "Protrude");
            if (bInit) InitPosition();
        }
        #endregion

        #region Axis
        public enum ePos
        {
            Up,
            Elevator,
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
        double m_secUp = 1; 
        public string RunUnload()
        {
            try
            {
                if (Run(RunMove(ePos.Elevator))) return m_sInfo;
                if (Run(RunMove(ePos.Up))) return m_sInfo;
                Thread.Sleep((int)(1000 * m_secUp));
                if (Run(RunMove(ePos.Elevator))) return m_sInfo;
            }
            finally { RunMove(ePos.Elevator); }
            return "OK"; 
        }

        string m_sInfo = "OK";
        bool Run(string sRun)
        {
            m_sInfo = sRun;
            return (sRun == "OK");
        }

        public void RunTree(Tree tree)
        {
            m_secUp = tree.Set(m_secUp, m_secUp, "Up Delay", "Up Delay Time (sec)"); 
        }
        #endregion

        public string p_id { get; set; }
        public UnloadEV(string id)
        {
            p_id = id;
        }

        public void ThreadStop()
        {

        }

    }
}
