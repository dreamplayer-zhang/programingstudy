using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using System;
using System.Collections.Generic;

namespace Root_JEDI.Module
{
    public class Flipper : NotifyProperty
    {
        #region Floor
        public enum eFloor
        {
            Down,
            Up,
        }

        public class Floor : NotifyProperty
        {
            public DIO_Is m_diCheck;
            public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
            {
                toolBox.GetDIO(ref m_diCheck, module, "Check " + m_eFloor.ToString(), new string[2] { "0", "1" });
            }

            public bool IsCheck(bool bCheck)
            {
                if (m_diCheck.ReadDI(0) != bCheck) return false;
                if (m_diCheck.ReadDI(1) != bCheck) return false;
                return true;
            }

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

            public string CheckTray(bool bCheck)
            {
                if (IsCheck(bCheck) == false) return m_eFloor.ToString() + " Check Sensor"; 
                if (bCheck != (p_infoTray != null)) return m_eFloor.ToString() + " InfoTray Check Error";
                return "OK";
            }

            eFloor m_eFloor;
            public Floor(eFloor eFloor)
            {
                m_eFloor = eFloor; 
            }
        }
        public Dictionary<eFloor, Floor> m_aFloor = new Dictionary<eFloor, Floor>(); 
        void initFloor()
        {
            m_aFloor.Add(eFloor.Down, new Floor(eFloor.Down));
            m_aFloor.Add(eFloor.Up, new Floor(eFloor.Up));
        }
        #endregion 

        #region ToolBox
        public Axis m_axis;
        public DIO_I4O2 m_dioGrip;
        
        public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
        {
            toolBox.GetAxis(ref m_axis, module, "Flip");
            toolBox.GetDIO(ref m_dioGrip, module, "Grip", "Off", "Grip");
            m_aFloor[eFloor.Down].GetTools(toolBox, module, bInit);
            m_aFloor[eFloor.Up].GetTools(toolBox, module, bInit);
            if (bInit)
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos))); 
                RunGrip(true);
            }
        }
        #endregion

        #region Axis Flip
        public enum ePos
        {
            Up,
            Flip,
            Bad
        }
        public string RunFlip(bool bFlip, bool bWait = true)
        {
            ePos ePos = bFlip ? ePos.Flip : ePos.Up;
            m_axis.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        ePos GetFlipPos()
        {
            double fPos = m_axis.p_posCommand;
            if (Math.Abs(fPos - m_axis.GetPosValue(ePos.Up)) < 1) return ePos.Up;
            if (Math.Abs(fPos - m_axis.GetPosValue(ePos.Flip)) < 1) return ePos.Flip;
            return ePos.Bad; 
        }
        #endregion

        #region DIO
        public string RunGrip(bool bGrip, bool bWait = true)
        {
            m_dioGrip.Write(bGrip);
            return bWait ? m_dioGrip.WaitDone() : "OK";
        }
        #endregion

        #region public
        public Floor GetFloor(eFloor eFloor)
        {
            switch (GetFlipPos())
            {
                case ePos.Up: return m_aFloor[eFloor];
                case ePos.Flip: return m_aFloor[1 - eFloor]; 
            }
            return null; 
        }
        #endregion

        public string p_id { get; set; }
        public Flipper(string id)
        {
            p_id = id;
            initFloor(); 
        }

        public void ThreadStop()
        {
        }
    }
}
