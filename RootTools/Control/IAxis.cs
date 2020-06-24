using RootTools.Trees;
using System;
using System.Windows.Controls;

namespace RootTools.Control
{
    public interface IAxis
    {
        Axis.eState p_eState { get; set; }
        UserControl p_ui { get; }
        Log p_log { get; }
        string p_sID { get; set; }
        int p_nAxisID { get; set; }
        int p_prgHome
        {
            get;
            set;
        }
        bool p_sensorHome { get; set; }
        bool p_sensorLimitM { get; set; }
        bool p_sensorLimitP { get; set; }

        void ServoOn(bool bOn, bool bAbsoluteEncoder = false);

        string HomeStart();

        void OverrideV();
        void RunTree(Tree.eMode mode);

        void ClearPos();
        void AddPos(string sPos);
        double GetPos(string sPos);
        double p_posCommand { get; set; }
        double p_posActual { get; set; }
        double p_vNow { get; set; }

        double p_vRate { get; set; }
        string Move(double fPos, double vMove = -1, double secAcc = -1, double secDec = -1);
        string Move(string pos, double fOffset = 0, double vMove = -1, double secAcc = -1, double secDec = -1);
        void SetTrigger(double fPos0, double fPos1, double dPos, bool bCmd, bool bLevel = true, double dTrigTime = 2);
        void ResetTrigger();
    }
}
