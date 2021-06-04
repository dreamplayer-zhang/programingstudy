using Root_Pine2_Vision.Module;
using RootTools;
using RootTools.Comm;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Root_Pine2.Module
{
    public class Boat : NotifyProperty
    {
        #region Step
        public enum eStep
        {
            Init,
            Ready,
            Run,
            Done,
            RunReady,
        }
        eStep _eStep = eStep.Init;
        public eStep p_eStep
        {
            get { return _eStep; }
            set
            {
                if (_eStep == value) return;
                _eStep = value;
                OnPropertyChanged();
                if (value == eStep.RunReady) m_bgwRunReady.RunWorkerAsync();
            }
        }

        BackgroundWorker m_bgwRunReady = new BackgroundWorker();
        private void M_bgwRunReady_DoWork(object sender, DoWorkEventArgs e)
        {
            m_axis.StartMove(m_boats.p_ePosLoad);
            p_eStep = (m_axis.WaitReady() == "OK") ? eStep.Ready : eStep.Init;
        }
        #endregion

        #region ToolBox
        public Axis m_axis;
        DIO_O m_doVacuumPump;
        public DIO_Os m_doVacuum;
        public DIO_O m_doBlow;
        DIO_I2O m_dioRollerDown;
        DIO_O m_doRollerPusher;
        DIO_O m_doCleanerBlow;
        DIO_O m_doCleanerSuction;
        public void GetTools(ToolBox toolBox, Boats boats, bool bInit)
        {
            toolBox.GetAxis(ref m_axis, boats, p_id + ".Snap");
            toolBox.GetDIO(ref m_doVacuumPump, boats, p_id + ".Vacuum Pump");
            toolBox.GetDIO(ref m_doVacuum, boats, p_id + ".Vacuum", new string[4] { "1", "2", "3", "4" });
            toolBox.GetDIO(ref m_doBlow, boats, p_id + ".Blow");
            toolBox.GetDIO(ref m_dioRollerDown, boats, p_id + ".Roller", "Up", "Down");
            toolBox.GetDIO(ref m_doRollerPusher, boats, p_id + ".Roller Pusher");
            toolBox.GetDIO(ref m_doCleanerBlow, boats, p_id + ".Cleaner Blow");
            toolBox.GetDIO(ref m_doCleanerSuction, boats, p_id + ".Cleaner Suction");
            m_remoteSnap.GetTools(toolBox, bInit); 
            if (bInit) InitPosition();
        }
        #endregion

        #region Remote Snap
        public class RemoteSnap
        {
            TCPAsyncServer m_tcpip;
            public void GetTools(ToolBox toolBox, bool bInit)
            {
                toolBox.GetComm(ref m_tcpip, m_boats, p_id + ".RemoteSnap"); 
                if (bInit) m_tcpip.EventReciveData += M_tcpip_EventReciveData;
            }

            private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
            {
                string sSend = Encoding.Default.GetString(aBuf, 0, nSize);
                if (sSend.Length <= 0) return;
                MemoryStream memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(sSend));
                m_treeRoot.m_job = new Job(memoryStream, false, m_boats.m_log);
                m_treeRoot.p_eMode = Tree.eMode.JobOpen;
                m_recipe.RunTree(m_treeRoot, true);
                m_treeRoot.m_job.Close();
                m_boats.StartSnap(m_recipe); 
            }

            string p_id { get; set; }
            Boats m_boats;
            TreeRoot m_treeRoot;
            Vision2D.Recipe m_recipe; 
            public RemoteSnap(string id, Boats boats)
            {
                p_id = id; 
                m_boats = boats;
                m_treeRoot = new TreeRoot(id, boats.m_log);
                m_recipe = new Vision2D.Recipe(boats.m_vision); 
            }
        }
        public RemoteSnap m_remoteSnap; 
        #endregion

        #region Axis
        public enum ePos
        {
            Handler,
            Vision,
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

        public string MoveSnap(double dPosAcc)
        {
            m_axis.StartMove(m_axis.m_trigger.m_aPos[0] + dPosAcc);
            return m_axis.WaitReady();
        }

        double[] m_pSnap = new double[2] { 0, 0 }; 
        void CalcSnapPos(Vision2D.Recipe.Snap snapData)
        {
            double pStart = m_axis.GetPosValue(ePos.SnapStart) + m_yScale * snapData.m_dpAxis.Y;
            double pEnd = m_pSnap[0] + m_yScale * m_mmSnap;
            m_axis.m_trigger.m_aPos[0] = pStart;
            m_axis.m_trigger.m_aPos[1] = pEnd; 
            double dpAcc = m_yScale * m_mmAcc; 
            switch (snapData.m_eDirection)
            {
                case Vision2D.Recipe.Snap.eDirection.Forward:
                    m_pSnap[0] = pStart - dpAcc;
                    m_pSnap[1] = pEnd + dpAcc;
                    break;
                case Vision2D.Recipe.Snap.eDirection.Backward:
                    m_pSnap[0] = pEnd + dpAcc;
                    m_pSnap[1] = pStart - dpAcc;
                    break;
            }
        }

        double m_yScale = 10000;
        double m_mmSnap = 300;
        double m_mmAcc = 20;
        public string RunMoveSnapStart(Vision2D.Recipe.Snap snapData, bool bWait = true)
        {
            CalcSnapPos(snapData);
            m_axis.StartMove(m_pSnap[0]);
            return bWait ? m_axis.WaitReady() : "OK";
        }

        public string RunSnap()
        {
            try
            {
                m_axis.RunTrigger(true);
                m_axis.StartMove(m_pSnap[1], "Snap");
                return m_axis.WaitReady();

            }
            finally { m_axis.RunTrigger(false); }
        }

        public void RunTreeAxis(Tree tree)
        {
            m_yScale = tree.Set(m_yScale, m_yScale, "Scale", "Snap Axis Scale (pulse / mm)");
            m_mmSnap = tree.Set(m_mmSnap, m_mmSnap, "Snap Length", "Snap Length (mm)");
            m_mmAcc = tree.Set(m_mmAcc, m_mmAcc, "Acc Length", "Acceleration Length (mm)"); 
        }
        #endregion

        #region Vacuum
        public void SetVacuum(bool[] aVacuum)
        {
            for (int n = 0; n < Math.Min(4, aVacuum.Length); n++) m_doVacuum.Write(n, aVacuum[n]);
        }

        public void RunVacuum(bool bOn)
        {
            m_doVacuumPump.Write(bOn);
        }

        public void RunBlow(bool bBlow)
        {
            m_doBlow.Write(bBlow);
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

        public void Reset(ModuleBase.eState eState)
        {
            p_infoStrip = null;
            if (eState == ModuleBase.eState.Ready) p_eStep = eStep.RunReady;
        }

        public InfoStrip p_infoStrip { get; set; }
        public string p_id { get; set; }
        Boats m_boats;
        IWorks m_works;
        public Boat(string id, Boats boats, IWorks works)
        {
            m_bgwRunReady.DoWork += M_bgwRunReady_DoWork;
            p_id = id + works.p_eWorks.ToString();
            m_remoteSnap = new RemoteSnap(p_id, boats); 
            m_boats = boats;
            m_works = works;
        }
    }
}
