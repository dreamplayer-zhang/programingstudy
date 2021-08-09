using System;
using System.Collections.Generic;

namespace RootTools.Control
{
    public class AxisXZ : NotifyProperty
    {
        #region Property
        public string p_id { get; set; }
        #endregion

        #region Position & Velocity
        public RPoint p_posCommand
        {
            get { return new RPoint(p_axisX.p_posCommand, p_axisZ.p_posCommand); }
        }

        public RPoint p_posActual
        {
            get { return new RPoint(p_axisX.p_posActual, p_axisZ.p_posActual); }
        }

        public RPoint p_vNow
        {
            get { return new RPoint(p_axisX.p_vNow, p_axisZ.p_vNow); }
        }
        #endregion

        #region List Position
        public List<string> m_asPos = new List<string>();
        public void AddPos(params string[] asPos)
        {
            p_axisX.AddPos(asPos);
            p_axisZ.AddPos(asPos);
            foreach (string sPos in asPos) m_asPos.Add(sPos);
        }

        public RPoint GetPosValue(Enum pos)
        {
            return new RPoint(p_axisX.GetPosValue(pos), p_axisZ.GetPosValue(pos));
        }

        public RPoint GetPosValue(string sPos)
        {
            return new RPoint(p_axisX.GetPosValue(sPos), p_axisZ.GetPosValue(sPos));
        }

        public bool IsInPos(Enum pos, double posError = 10)
        {
            RPoint dp = GetPosValue(pos) - p_posCommand;
            return (Math.Abs(dp.X) <= posError) && (Math.Abs(dp.Y) <= posError);
        }
        #endregion

        #region List Speed
        public void AddSpeed(params string[] asSpeed)
        {
            p_axisX.AddPos(asSpeed);
            p_axisZ.AddPos(asSpeed);
        }
        #endregion

        #region Move
        public string StartMove(Enum pos, RPoint rpOffset = null, Enum speed = null)
        {
            return StartMove(pos.ToString(), rpOffset, speed);
        }

        public string StartMove(string sPos, RPoint rpOffset = null, Enum speed = null)
        {
            RPoint rp = GetPosValue(sPos) + (rpOffset ?? new RPoint());
            return StartMove(rp, (speed == null) ? null : speed.ToString());
        }

        public string StartMove(double x, double y, string sSpeed = null)
        {
            return StartMove(new RPoint(x, y), sSpeed);
        }

        RPoint m_rpDst = new RPoint();
        public string StartMove(RPoint rpPos, string sSpeed = null)
        {
            m_rpDst = rpPos;
            string xInfo = p_axisX.StartMove(rpPos.X, sSpeed);
            string yInfo = p_axisZ.StartMove(rpPos.Y, sSpeed);
            if (xInfo != "OK") return "AxisX StartMove Error : " + xInfo;
            if (yInfo != "OK") return "AxisZ StartMove Error : " + yInfo;
            return "OK";
        }

        public string WaitReady(double dInPos = -1)
        {
            string xInfo = p_axisX.WaitReady(dInPos);
            string yInfo = p_axisZ.WaitReady(dInPos);
            if (xInfo != "OK") return "AxisX WaitReady Error : " + xInfo;
            if (yInfo != "OK") return "AxisZ WaitReady Error : " + yInfo;
            return "OK";
        }
        #endregion

        #region Shift
        public string StartShift(RPoint drpPos, string sSpeed = null)
        {
            RPoint rpPos = p_posCommand + drpPos;
            m_rpDst = rpPos;
            string xInfo = p_axisX.StartShift(drpPos.X, sSpeed);
            string yInfo = p_axisZ.StartShift(drpPos.Y, sSpeed);
            if (xInfo != "OK") return "AxisX StartMove Error : " + xInfo;
            if (yInfo != "OK") return "AxisZ StartMove Error : " + yInfo;
            return "OK";
        }
        #endregion

        #region Functions
        public void ServoOn(bool bOn)
        {
            p_axisX.ServoOn(bOn);
            p_axisZ.ServoOn(bOn);
        }
        #endregion

        public Axis p_axisX { get; set; }
        public Axis p_axisZ { get; set; }

        public void Init(string id, Log log)
        {
            p_id = id;
        }
    }
}
