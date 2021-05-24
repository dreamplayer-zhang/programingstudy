using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools.Control
{
    public class Axis3D : NotifyProperty
    {
        #region Property
        public string p_id { get; set; }
        #endregion

        #region Position & Velocity
        public R3Point p_posCommand
        {
            get { return new R3Point(p_axisX.p_posCommand, p_axisY.p_posCommand, p_axisZ.p_posCommand); }
        }

        public R3Point p_posActual
        {
            get { return new R3Point(p_axisX.p_posActual, p_axisY.p_posActual, p_axisZ.p_posActual); }
        }

        public R3Point p_vNow
        {
            get { return new R3Point(p_axisX.p_vNow, p_axisY.p_vNow, p_axisZ.p_vNow); }
        }
        #endregion

        #region List Position
        public List<string> m_asPos = new List<string>();
        public void AddPos(params string[] asPos)
        {
            p_axisX.AddPos(asPos);
            p_axisY.AddPos(asPos);
            p_axisZ.AddPos(asPos);
            foreach (string sPos in asPos) m_asPos.Add(sPos);
        }

        public R3Point GetPosValue(Enum pos)
        {
            return new R3Point(p_axisX.GetPosValue(pos), p_axisY.GetPosValue(pos), p_axisZ.GetPosValue(pos));
        }

        public R3Point GetPosValue(string sPos)
        {
            return new R3Point(p_axisX.GetPosValue(sPos), p_axisY.GetPosValue(sPos), p_axisZ.GetPosValue(sPos));
        }

        public bool IsInPos(Enum pos, double posError = 10)
        {
            R3Point dp = GetPosValue(pos) - p_posCommand;
            return (Math.Abs(dp.X) <= posError) && (Math.Abs(dp.Y) <= posError) && (Math.Abs(dp.Z) <= posError);
        }
        #endregion

        #region List Speed
        public void AddSpeed(params string[] asSpeed)
        {
            p_axisX.AddPos(asSpeed);
            p_axisY.AddPos(asSpeed);
            p_axisZ.AddPos(asSpeed);
        }
        #endregion

        #region Move
        public string StartMove(Enum pos, R3Point rpOffset = null, Enum speed = null)
        {
            return StartMove(pos.ToString(), rpOffset, speed);
        }

        public string StartMove(string sPos, R3Point rpOffset = null, Enum speed = null)
        {
            R3Point rp = GetPosValue(sPos) + (rpOffset ?? new R3Point());
            return StartMove(rp, (speed == null) ? null : speed.ToString());
        }

        public string StartMove(double x, double y, double z, string sSpeed = null)
        {
            return StartMove(new R3Point(x, y, z), sSpeed);
        }

        R3Point m_rpDst = new R3Point();
        public string StartMove(R3Point rpPos, string sSpeed = null)
        {
            m_rpDst = rpPos;
            string xInfo = p_axisX.StartMove(rpPos.X, sSpeed);
            string yInfo = p_axisY.StartMove(rpPos.Y, sSpeed);
            string zInfo = p_axisZ.StartMove(rpPos.Z, sSpeed);
            if (xInfo != "OK") return "AxisX StartMove Error : " + xInfo;
            if (yInfo != "OK") return "AxisY StartMove Error : " + yInfo;
            if (zInfo != "OK") return "AxisZ StartMove Error : " + zInfo;
            return "OK";
        }

        public string WaitReady(double dInPos = -1)
        {
            string xInfo = p_axisX.WaitReady(dInPos);
            string yInfo = p_axisY.WaitReady(dInPos);
            string zInfo = p_axisZ.WaitReady(dInPos);
            if (xInfo != "OK") return "AxisX WaitReady Error : " + xInfo;
            if (yInfo != "OK") return "AxisY WaitReady Error : " + yInfo;
            if (zInfo != "OK") return "AxisZ WaitReady Error : " + zInfo;
            return "OK";
        }
        #endregion

        #region Shift
        public string StartShift(R3Point drpPos, string sSpeed = null)
        {
            R3Point rpPos = p_posCommand + drpPos;
            m_rpDst = rpPos;
            string xInfo = p_axisX.StartShift(drpPos.X, sSpeed);
            string yInfo = p_axisY.StartShift(drpPos.Y, sSpeed);
            string zInfo = p_axisZ.StartShift(drpPos.Z, sSpeed);
            if (xInfo != "OK") return "AxisX StartMove Error : " + xInfo;
            if (yInfo != "OK") return "AxisY StartMove Error : " + yInfo;
            if (zInfo != "OK") return "AxisZ StartMove Error : " + yInfo;
            return "OK";
        }
        #endregion

        #region Functions
        public void ServoOn(bool bOn)
        {
            p_axisX.ServoOn(bOn);
            p_axisY.ServoOn(bOn);
            p_axisZ.ServoOn(bOn);
        }
        #endregion

        public Axis p_axisX { get; set; }
        public Axis p_axisY { get; set; }
        public Axis p_axisZ { get; set; }

        public void Init(string id, Log log)
        {
            p_id = id;
        }

    }
}
