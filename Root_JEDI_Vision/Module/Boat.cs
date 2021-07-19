using Root_JEDI_Sorter.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;

namespace Root_JEDI_Vision.Module
{
    public class Boat : NotifyProperty
    {
        public enum eDirection
        {
            Forward,
            Backward
        }

        #region ToolBox
        public Axis m_axis;
        public Stage m_stage;
        public void GetTools(ToolBox toolBox, ModuleBase module, bool bInit)
        {
            toolBox.GetAxis(ref m_axis, module, p_id + ".Snap");
            m_stage.GetTools(toolBox, module, bInit); 
        }
        #endregion

        #region Axis
        public enum ePos
        {
            Ready,
            Done,
            SnapStart,
        }
        void InitPosition()
        {
            m_axis.AddPos(Enum.GetNames(typeof(ePos)));
            m_axis.AddSpeed("Snap");
        }

        public string RunMove(ePos ePos, bool bWait = true)
        {
            m_axis.StartMove(ePos);
            return bWait ? m_axis.WaitReady() : "OK";
        }
        #endregion

        #region Snap Position
        double m_yScale = 10000;    // pulse / mm
        double[] m_pSnap = new double[2] { 0, 0 };
        void CalcSnapPos(eDirection eDirection, double mmSnap, double mmOffset)
        {
            double pStart = m_axis.GetPosValue(ePos.SnapStart) + m_yScale * mmOffset;
            double pEnd = pStart + m_yScale * mmSnap;
            double dpAcc = CalcAccDist();
            switch (eDirection)
            {
                case eDirection.Forward:
                    m_pSnap[0] = pStart - dpAcc;
                    m_pSnap[1] = pEnd + dpAcc;
                    m_axis.m_trigger.m_aPos[0] = pStart;
                    m_axis.m_trigger.m_aPos[1] = pEnd + 100;
                    break;
                case eDirection.Backward:
                    m_pSnap[0] = pEnd + dpAcc;
                    m_pSnap[1] = pStart - dpAcc;
                    m_axis.m_trigger.m_aPos[0] = pStart - 100;
                    m_axis.m_trigger.m_aPos[1] = pEnd;
                    break;
            }
        }
        public double CalcAccDist()
        {
            Axis.Speed SnapSpeed = m_axis.GetSpeedValue("Snap");
            double dVel = SnapSpeed.m_v / m_yScale;    // 최종 속도 (등속도)       [mm/s]
            double dSec = SnapSpeed.m_acc;             // 가속하는데 걸리는 시간   [s]
            double dAcc = dVel / dSec;
            return m_yScale * (0.5 * dAcc * dSec * dSec);         // 가속하는 거리 [pulse]
        }
        #endregion

        #region Snap
        public string RunMoveSnapStart(eDirection eDirection, double mmSnap, double mmOffset, bool bWait = true)
        {
            CalcSnapPos(eDirection, mmSnap, mmOffset);
            m_axis.StartMove(m_pSnap[0]);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string StartSnap()
        {
            m_axis.SetTrigger(m_axis.m_trigger.m_aPos[0], m_axis.m_trigger.m_aPos[1], m_axis.m_trigger.m_dPos, 5, false);
            m_axis.StartMove(m_pSnap[1], "Snap");
            return "OK";
        }

        public string WaitSnap()
        {
            try { return m_axis.WaitReady(); }
            finally { m_axis.RunTrigger(false); }
        }
        #endregion

        public void RunTreeAxis(Tree tree)
        {
            m_yScale = tree.Set(m_yScale, m_yScale, "Scale", "Snap Axis Scale (pulse / mm)");
        }

        public string p_id { get; set; }
        public Boat(string id)
        {
            p_id = id;
            m_stage = new Stage(id + ".Stage"); 
        }
    }
}
