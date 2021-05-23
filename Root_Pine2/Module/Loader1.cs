using Root_Pine2.Engineer;
using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;

namespace Root_Pine2.Module
{
    public class Loader1 : ModuleBase
    {
        #region ToolBox
        AxisXY m_axisXZ;
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisXZ, this, "Loader1");
            m_picker.GetTools(m_toolBox, this, bInit);
            if (bInit) InitPosition();
        }

        public enum ePos
        {
            Ready,
            Turnover
        }
        void InitPosition()
        {
            m_axisXZ.AddPos(Enum.GetNames(typeof(ePos)));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top3D, Vision.eWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top3D, Vision.eWorks.B));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top2D, Vision.eWorks.A));
            m_axisXZ.AddPos(GetPosString(Vision.eVision.Top2D, Vision.eWorks.B));
        }

        string GetPosString(Vision.eVision eVision, Vision.eWorks eVisionWorks)
        {
            return eVision.ToString() + eVisionWorks.ToString();
        }

        public string RunMoveX(ePos ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(ePos);
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveX(Vision.eVision eVision, Vision.eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisX.StartMove(GetPosString(eVision, ePos));
            return bWait ? m_axisXZ.p_axisX.WaitReady() : "OK";
        }

        public string RunMoveZ(ePos ePos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(ePos);
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }

        public string RunMoveZ(Vision.eVision eVision, Vision.eWorks ePos, bool bWait = true)
        {
            m_axisXZ.p_axisY.StartMove(GetPosString(eVision, ePos));
            return bWait ? m_axisXZ.p_axisY.WaitReady() : "OK";
        }
        #endregion

        #region RunLoad
        public string RunLoad(Vision.eVision eVision, Vision.eWorks eVisionWorks)
        {
            Boats boats = m_handler.m_aBoats[eVision];
            Boats.Boat boat = boats.m_aBoat[eVisionWorks];
            if (boat.p_eStep != Boats.Boat.eStep.Done) return "Boat not Done";
            if (m_picker.p_infoStrip != null) return "Picker has InfoStrip";
            try
            {
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (Run(RunMoveX(eVision, eVisionWorks))) return p_sInfo;
                if (Run(RunMoveZ(eVision, eVisionWorks))) return p_sInfo;
                boat.RunVacuum(false);
                boat.RunBlow(true); 
                if (Run(m_picker.RunVacuum(true))) return p_sInfo;
                boat.RunBlow(false);
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (m_picker.IsVacuum() == false) return "Pick Strip Error";
                m_picker.p_infoStrip = boat.p_infoStrip;
                boat.p_infoStrip = null;
                boat.p_eStep = Boats.Boat.eStep.RunReady; 
            }
            finally
            {
                boat.RunBlow(false);
                RunMoveZ(ePos.Ready);
            }
            return "OK";
        }
        #endregion

        #region RunUnload
        public string RunUnloadTurnover()
        {
            Loader2 loader2 = m_handler.m_loader2;
            if (loader2.p_eState != eState.Ready) return "Loader2 is not Ready"; 
            if (loader2.m_qModuleRun.Count > 0) return "Loader2 is not Ready";
            try
            {
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (Run(RunMoveX(ePos.Turnover))) return p_sInfo;
                if (Run(RunMoveZ(ePos.Turnover))) return p_sInfo;
                loader2.RunVacuum(true);
                m_picker.RunVacuum(false);
                loader2.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (Run(RunMoveX(Vision.eVision.Top2D, Vision.eWorks.B))) return p_sInfo;
                loader2.StartLoader2();
            }
            finally
            {
                RunMoveZ(ePos.Ready);
                RunMoveX(Vision.eVision.Top2D, Vision.eWorks.B);
            }
            return "OK";
        }

        public string RunUnload(Vision.eWorks eVisionWorks)
        {
            Boats boats = m_handler.m_aBoats[Vision.eVision.Top2D];
            Boats.Boat boat = boats.m_aBoat[eVisionWorks];
            if (IsBoatReady(boats) == false) return "Boats p_eState is not OK";
            if (boat.p_eStep != Boats.Boat.eStep.Ready) return "Boat not Ready";
            try
            {
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
                if (Run(RunMoveX(Vision.eVision.Top2D, eVisionWorks))) return p_sInfo;
                if (Run(RunMoveZ(Vision.eVision.Top2D, eVisionWorks))) return p_sInfo;
                boat.RunVacuum(true);
                m_picker.RunVacuum(false);
                boat.p_infoStrip = m_picker.p_infoStrip;
                m_picker.p_infoStrip = null;
                if (Run(RunMoveZ(ePos.Ready))) return p_sInfo;
            }
            finally
            {
                RunMoveZ(ePos.Ready);
            }
            return "OK";
        }

        bool IsBoatReady(Boats boats)
        {
            switch (boats.p_eState)
            {
                case eState.Ready:
                case eState.Run: return true;
                default: return false;
            }
        }

        #endregion

        #region override
        public override void Reset()
        {
            base.Reset();
            m_picker.p_infoStrip = null;
        }

        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            m_picker.RunTreeVacuum(tree.GetTree("Vacuum"));
        }
        #endregion

        Picker m_picker = null;
        Pine2 m_pine2 = null;
        Pine2_Handler m_handler = null; 
        public Loader1(string id, IEngineer engineer, Pine2_Handler handler)
        {
            m_picker = new Picker(id);
            m_handler = handler; 
            m_pine2 = handler.m_pine2;
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            m_picker.ThreadStop();
            base.ThreadStop();
        }

    }
}
