using System;
using System.Collections.Generic;

namespace RootTools.Control
{
    public class AxisXY : NotifyProperty
    {
        #region Property
        public string p_id { get; set; }
        #endregion

        #region Position & Velocity
        public RPoint p_posCommand
        { 
            get { return new RPoint(p_axisX.p_posCommand, p_axisY.p_posCommand); }
        }

        public RPoint p_posActual
        {
            get { return new RPoint(p_axisX.p_posActual, p_axisY.p_posActual); }
        }

        public RPoint p_vNow
        {
            get { return new RPoint(p_axisX.p_vNow, p_axisY.p_vNow); }
        }
        #endregion

        #region List Position
        public List<string> m_asPos = new List<string>();
        public void AddPos(params string[] asPos)
        {
            p_axisX.AddPos(asPos);
            p_axisY.AddPos(asPos);
            foreach (string sPos in asPos) m_asPos.Add(sPos); 
        }

        public RPoint GetPosValue(Enum pos)
        {
            return new RPoint(p_axisX.GetPosValue(pos), p_axisY.GetPosValue(pos)); 
        }

        public RPoint GetPosValue(string sPos)
        {
            return new RPoint(p_axisX.GetPosValue(sPos), p_axisY.GetPosValue(sPos));
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
            p_axisY.AddPos(asSpeed);
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

        RPoint m_rpDst = new RPoint();
        public string StartMove(RPoint rpPos, string sSpeed = null)
        {
            m_rpDst = rpPos;
            string xInfo = p_axisX.StartMove(rpPos.X, sSpeed);
            string yInfo = p_axisY.StartMove(rpPos.Y, sSpeed);
            if (xInfo != "OK") return "AxisX StartMove X Error : " + xInfo;
            if (yInfo != "OK") return "AxisY StartMove Y Error : " + yInfo;
            return "OK";
        }

        public string WaitReady(double dInPos = -1)
        {
            string xInfo = p_axisX.WaitReady(dInPos);
            string yInfo = p_axisY.WaitReady(dInPos);
            if (xInfo != "OK") return "AxisX WaitReady Error : " + xInfo;
            if (yInfo != "OK") return "AxisY WaitReady Error : " + yInfo;
            return "OK";
        }
        #endregion

        #region Shift
        public string StartShift(RPoint drpPos, string sSpeed = null)
        {
            RPoint rpPos = p_posCommand + drpPos; 
            m_rpDst = rpPos;
            string xInfo = p_axisX.StartShift(drpPos.X, sSpeed);
            string yInfo = p_axisY.StartShift(drpPos.Y, sSpeed);
            if (xInfo != "OK") return "AxisX StartMove X Error : " + xInfo;
            if (yInfo != "OK") return "AxisY StartMove Y Error : " + yInfo;
            return "OK";
        }
        #endregion

        #region Functions
        public void ServoOn(bool bOn)
        {
            p_axisX.ServoOn(bOn);
            p_axisY.ServoOn(bOn); 
        }
        #endregion

        public Axis p_axisX { get; set; }
        public Axis p_axisY { get; set; }

        public void Init(string id, Log log)
        {
            p_id = id;
        }
    }
}
