using Root.Module;
using Root_VEGA_P_Vision.Module;
using RootTools;
using RootTools.GAFs;
using RootTools.Module;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace Root
{
    public class Root_Handler : IHandler
    {
        #region Module
        public ModuleList p_moduleList { get; set; }
        //public Test m_test;
        //public BayerConvert m_bayer;
        //public ReadExcel m_readExcel;
        //public RemoteModule m_remote;
        //public RemoteModule m_server;
        //public TestServer m_tcpServer;
        //public TestClient m_tcpClient;
        public Vision m_bufferClient;
        public Vision m_bufferServer;
        void InitModule()
        {
            p_moduleList = new ModuleList(m_engineer);
            //m_test = new Test("Test", m_engineer);
            //InitModule(m_test);
            //m_bayer = new BayerConvert("BayerConvert", m_engineer);
            //InitModule(m_bayer);
            //m_readExcel = new ReadExcel("ReadExcel", m_engineer);
            //InitModule(m_readExcel);
            //m_server = new RemoteModule("Server", m_engineer, ModuleBase.eRemote.Server);
            //InitModule(m_server);
            //m_remote = new RemoteModule("Remote", m_engineer, ModuleBase.eRemote.Client);
            //InitModule(m_remote);
            //m_tcpServer = new TestServer("TestServer", m_engineer);
            //InitModule(m_tcpServer);
            //m_tcpClient = new TestClient("TestClient", m_engineer);
            //InitModule(m_tcpClient);
            m_bufferServer = new Vision("VisionServer", m_engineer, ModuleBase.eRemote.Server);
            InitModule(m_bufferServer);
            m_bufferClient = new Vision("VisionClient", m_engineer, ModuleBase.eRemote.Client);
            InitModule(m_bufferClient);
        }

        void InitModule(ModuleBase module)
        {
            ModuleBase_UI ui = new ModuleBase_UI();
            ui.Init(module);
            p_moduleList.AddModule(module, ui);
        }

        public bool IsEnableRecovery()
        {
            return false; 
        }
        #endregion

        #region StateHome
        public string StateHome()
        {
            string sInfo = StateHome(p_moduleList.m_aModule);
            if (sInfo == "OK") EQ.p_eState = EQ.eState.Ready;
            return sInfo;
        }

        protected string StateHome(params ModuleBase[] aModule)
        {
            List<ModuleBase> listModule = new List<ModuleBase>();
            foreach (ModuleBase module in aModule) listModule.Add(module);
            return StateHome(listModule);
        }

        protected string StateHome(Dictionary<ModuleBase, UserControl> aModule)
        {
            List<ModuleBase> listModule = new List<ModuleBase>();
            foreach (ModuleBase module in aModule.Keys) listModule.Add(module);
            return StateHome(listModule);
        }

        protected string StateHome(List<ModuleBase> aModule)
        {
            foreach (ModuleBase module in aModule) module.StartHome();
            bool bHoming = true;
            while (bHoming)
            {
                Thread.Sleep(10);
                bHoming = false;
                foreach (ModuleBase module in aModule)
                {
                    if (module.p_eState == ModuleBase.eState.Home) bHoming = true;
                }
            }
            foreach (ModuleBase module in aModule)
            {
                if (module.p_eState != ModuleBase.eState.Ready)
                {
                    EQ.p_bStop = true;
                    EQ.p_eState = EQ.eState.Init;
                    return module.p_id + " Home Error";
                }
            }
            return "OK";
        }
        #endregion

        #region Reset
        public string Reset()
        {
            Reset(m_gaf, p_moduleList); 
            return "OK"; 
        }

        void Reset(GAF gaf, ModuleList moduleList)
        {
            gaf?.ClearALID();
            foreach (ModuleBase module in moduleList.m_aModule.Keys) module.Reset();
        }
        #endregion

        #region Calc Sequence
        public string AddSequence(dynamic infoSlot)
        {
//            m_process.AddInfoReticle(infoSlot);
            return "OK";
        }

        public void CalcSequence()
        {
//            m_process.ReCalcSequence(true);
        }
        #endregion

        #region IHandler
        public void CheckFinish()
        {
        }

        public dynamic GetGemSlot(string sSlot)
        {
            return null; 
        }
        #endregion

        string m_id;
        public Root_Engineer m_engineer;
        GAF m_gaf; 
        public void Init(string id, IEngineer engineer)
        {
            m_id = id;
            m_engineer = (Root_Engineer)engineer;
            m_gaf = engineer.ClassGAF(); 
            InitModule();
            m_engineer.ClassMemoryTool().InitThreadProcess();
        }

        public void ThreadStop()
        {
            p_moduleList.ThreadStop();
            foreach (ModuleBase module in p_moduleList.m_aModule.Keys) module.ThreadStop(); 
        }

        public RnRData GetRnRData()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateEvent()
        {
            throw new System.NotImplementedException();
        }
    }
}
