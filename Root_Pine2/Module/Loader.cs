using Root_Pine2.Engineer;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_Pine2.Module
{
    public class Loader : ModuleBase
    {
        #region ToolBox
        AxisXY m_axisXY;
        Axis m_axisZ;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisXY, this, "XY");
            m_toolBox.GetAxis(ref m_axisZ, this, "Z");
            m_picker.GetTools(m_toolBox, this, bInit); 
            if (bInit) InitPosition();
        }

        public enum ePosLoad
        {
            LoadEV,
            Transfer0,
            Transfer1,
            Transfer2,
            Transfer3,
            Transfer4,
            Transfer5,
            Transfer6,
            Transfer7,
        }
        public enum ePosUnload
        {
            Boat_3A,
            Boat_3B,
            Boat_2A,
            Boat_2B,
            Tray7,
        }
        void InitPosition()
        {
            m_axisXY.AddPos(Enum.GetNames(typeof(ePosLoad)));
            m_axisXY.AddPos(Enum.GetNames(typeof(ePosUnload)));
            m_axisZ.AddPos(Enum.GetNames(typeof(ePosLoad)));
            m_axisZ.AddPos(Enum.GetNames(typeof(ePosUnload)));

        }
        #endregion

        #region AxisXY
        public string RunMoveXY(ePosLoad ePos, bool bWait = true)
        {
            m_axisXY.StartMove(ePos);
            return bWait ? m_axisXY.WaitReady() : "OK";
        }

        public string RunMoveXY(ePosUnload ePos, bool bWait = true)
        {
            m_axisXY.StartMove(ePos);
            return bWait ? m_axisXY.WaitReady() : "OK";
        }

        int m_pulsemm = 1000; 
        public string RunMoveLoadEV(bool bWait = true)
        {
            double dPos = m_pulsemm * (77 - m_pine2.p_widthStrip);
            m_axisXY.StartMove(ePosLoad.LoadEV, new RPoint(dPos, 0));
            return bWait ? m_axisXY.WaitReady() : "OK"; 
        }

        void RunTreeAxisXY(Tree tree)
        {
            m_pulsemm = tree.Set(m_pulsemm, m_pulsemm, "Pulse / mm", "Axis X (Pulse / mm)");
        }
        #endregion

        #region AxisZ
        public string RunMoveZ(ePosLoad ePos, bool bWait = true)
        {
            m_axisZ.StartMove(ePos);
            return bWait ? m_axisZ.WaitReady() : "OK";
        }

        public string RunMoveZ(ePosUnload ePos, bool bWait = true)
        {
            m_axisZ.StartMove(ePos);
            return bWait ? m_axisZ.WaitReady() : "OK";
        }

        public string RunMoveUp(bool bWait = true)
        {
            m_axisZ.StartMove(0);
            return bWait ? m_axisZ.WaitReady() : "OK";
        }

        public string RunShakeUp(int nShake, int dzPulse)
        {
            int iShake = 0; 
            while (iShake < nShake)
            {
                if (Run(RunShakeUp(-dzPulse))) return p_sInfo; 
                if (Run(RunShakeUp(0.9 * dzPulse))) return p_sInfo;
                iShake++;
            }
            return RunMoveUp(); 
        }

        string RunShakeUp(double dzPulse)
        {
            m_axisZ.StartShift(dzPulse);
            return m_axisZ.WaitReady(); 
        }
        #endregion

        #region RunLoad
        public string RunLoad(ePosLoad ePos, int nShake, int dzPulse)
        {
            if (Run(RunMoveUp())) return p_sInfo; 
            if (ePos == ePosLoad.LoadEV)
            {
                if (Run(RunMoveLoadEV())) return p_sInfo;
                if (Run(RunMoveZ(ePos))) return p_sInfo;
                if (Run(m_picker.RunVacuum(true))) return p_sInfo; 
                if (Run(RunShakeUp(nShake, dzPulse))) return p_sInfo;
                if (m_picker.IsVacuum() == false) return p_sInfo; 
                //m_picker.p_infoStrip = 
            }
            else
            {
                if (Run(RunMoveXY(ePos))) return p_sInfo; 
            }
            return "OK";
        }
        #endregion

        Picker m_picker = null;
        LoadEV m_loadEV = null; 
        Pine2 m_pine2 = null; 
        public Loader(string id, IEngineer engineer, Pine2_Handler handler)
        {
            m_picker = new Picker(id);
            m_pine2 = handler.m_pine2;
            m_loadEV = handler.m_loadEV; 
            base.InitBase(id, engineer);
        }
    }
}
