using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Root.Module
{
    public class TestClient : ModuleBase
    {
        #region ToolBox
        TCPAsyncClient m_tcpip;
        //TCPSyncClient m_tcpip;
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_tcpip, this, "Client", 2000000);
            if (bInit)
            {
                m_tcpip.EventReciveData += M_tcpip_EventReciveData;
            }
        }
        #endregion

        #region TCPIP
        StopWatch m_swTCPIP = new StopWatch();
        public List<long> m_aTime = new List<long>(); 
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            m_aTime.Add(m_swTCPIP.ElapsedMilliseconds); 
        }

        public string Send(string sSend)
        {
            m_swTCPIP.Start(); 
            m_tcpip.Send(sSend);
            return "OK"; 
        }

        public string RunSend()
        {
            int nSend = 0; 
            m_aTime.Clear(); 
            while (m_aTime.Count < 100)
            {
                Thread.Sleep(1);
                if (nSend == m_aTime.Count)
                {
                    Send("Send");
                    nSend++; 
                }
            }
            long nSum = 0;
            foreach (long nTime in m_aTime) nSum += nTime;
            p_sInfo = "Time = " + (nSum / 100.0).ToString(); 
            return "OK"; 
        }
        #endregion

        #region override
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
        }

        public override void Reset()
        {
            base.Reset();
        }
        #endregion

        public TestClient(string id, IEngineer engineer)
        {
            InitBase(id, engineer); 
        }

        #region ModuleRun
        protected override void InitModuleRuns()
        {
            AddModuleRunList(new Run_Run(this), true, "Desc Run");
        }

        public class Run_Run : ModuleRunBase
        {
            TestClient m_module;
            public Run_Run(TestClient module)
            {
                m_module = module;
                InitModuleRun(module);
            }

            public override ModuleRunBase Clone()
            {
                Run_Run run = new Run_Run(m_module);
                return run;
            }

            public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
            {
            }

            public override string Run()
            {
                return m_module.RunSend(); 
            }
        }
        #endregion
    }
}
