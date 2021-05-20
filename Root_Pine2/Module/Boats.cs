using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.Threading;

namespace Root_Pine2.Module
{
    public class Boats : ModuleBase
    {
        #region ToolBox
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetAxis(ref m_axisCam, this, "Camera"); 
            m_aBoat[0].GetTools(m_toolBox, this, bInit);
            m_aBoat[1].GetTools(m_toolBox, this, bInit);
        }
        #endregion

        #region Cam Axis
        AxisXY m_axisCam; 
        #endregion

        #region Boat
        public class Boat
        {
            Axis m_axis; 
            DIO_O m_doVacuumPump; 
            DIO_Os m_doVacuum;
            DIO_O m_doBlow;
            DIO_I2O m_dioRollerDown;
            DIO_O m_doRollerPusher;
            DIO_O m_doCleanerBlow;
            DIO_O m_doCleanerSuction; 
            public void GetTools(ToolBox toolBox, Boats boats, bool bInit)
            {
                toolBox.GetAxis(ref m_axis, boats, p_id + ".Scan"); 
                toolBox.GetDIO(ref m_doVacuumPump, boats, p_id + ".Vacuum Pump");
                toolBox.GetDIO(ref m_doVacuum, boats, p_id + ".Vacuum", new string[4] { "1", "2", "3", "4" });
                toolBox.GetDIO(ref m_doBlow, boats, p_id + ".Blow");
                toolBox.GetDIO(ref m_dioRollerDown, boats, p_id + ".Roller", "Up", "Down");
                toolBox.GetDIO(ref m_doRollerPusher, boats, p_id + ".Roller Pusher");
                toolBox.GetDIO(ref m_doCleanerBlow, boats, p_id + ".Cleaner Blow");
                toolBox.GetDIO(ref m_doCleanerSuction, boats, p_id + ".Cleaner Suction");
                if (bInit) InitPosition();
            }

            #region Axis
            public enum ePos
            {
                Ready,
                Done,
            }
            void InitPosition()
            {
                m_axis.AddPos(Enum.GetNames(typeof(ePos)));
                m_axis.AddSpeed("Grab"); 
            }

            public string RunMove(ePos ePos, bool bWait = true)
            {
                m_axis.RunTrigger(false); 
                m_axis.StartMove(ePos);
                return bWait ? m_axis.WaitReady() : "OK"; 
            }

            public string RunMoveStartGrab(double dPosAcc)
            {
                m_axis.RunTrigger(false);
                m_axis.StartMove(m_axis.m_trigger.m_aPos[0] + dPosAcc);
                return m_axis.WaitReady(); 
            }

            public string StartGrab()
            {
                m_axis.RunTrigger(true); 
                return m_axis.StartMove(m_axis.m_trigger.m_aPos[1], "Grab"); 
            }
            #endregion

            #region Vacuum
            public void SetVacuum(bool[] aVacuum)
            {
                for (int n = 0; n < Math.Min(4, aVacuum.Length); n++) m_doVacuum.Write(n, aVacuum[n]); 
            }

            double m_secBlow = 0.2;
            public void RunVacuum(bool bOn)
            {
                m_doVacuumPump.Write(bOn);
                if (bOn) return;
                m_doBlow.Write(true);
                Thread.Sleep((int)(1000 * m_secBlow));
                m_doBlow.Write(false);
            }
            #endregion

            #region Clean Roller
            public string RunRoller(bool bDown)
            {
                m_doRollerPusher.Write(bDown);
                m_dioRollerDown.Write(bDown);
                return m_dioRollerDown.WaitDone(); 
            }
            #endregion

            #region Cleaner
            public void RunCleaner(bool bBlow, bool bSuction)
            {
                m_doCleanerBlow.Write(bBlow);
                m_doCleanerSuction.Write(bSuction); 
            }
            #endregion

            public void Reset()
            {
                p_infoStrip = null; 
            }

            public InfoStrip p_infoStrip { get; set; }
            public string p_id { get; set; }
            Boats m_boats;
            Vision.VisionWorks m_visionWorks; 
            public Boat(string id, Boats boats, Vision.VisionWorks visionWorks)
            {
                p_id = id;
                m_boats = boats;
                m_visionWorks = visionWorks; 
            }
        }
        Boat[] m_aBoat;
        void InitBoat(string id)
        {
            m_aBoat = new Boat[2];
            m_aBoat[0] = new Boat(id + "A", this, m_vision.m_aVisionWorks[0]);
            m_aBoat[1] = new Boat(id + "B", this, m_vision.m_aVisionWorks[1]);
        }
        #endregion

        #region State Home
        public override string StateHome()
        {
            if (EQ.p_bSimulate)
            {
                p_eState = eState.Ready;
                return "OK";
            }
            p_sInfo = base.StateHome();
            p_eState = (p_sInfo == "OK") ? eState.Ready : eState.Error;
            return p_sInfo;
        }

        public override void Reset()
        {
            m_aBoat[0].Reset();
            m_aBoat[1].Reset(); 
            base.Reset();
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }
        #endregion

        Pine2 m_pine2;
        Vision m_vision; 
        public Boats(string id, IEngineer engineer, Pine2 pine2, Vision vision)
        {
            p_id = id;
            m_pine2 = pine2;
            m_vision = vision; 
            InitBoat(id); 
            InitBase(id, engineer); 
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
