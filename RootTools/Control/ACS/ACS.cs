using RootTools.Trees;
using SPIIPLUSCOM660Lib;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.Control.ACS
{
    public class ACS : IToolSet, IControl //forgetACS
    {
        #region Axis
        void GetAxisCount()
        {
            if (p_bConnect == false) return; 
            try
            {
                string sAxis = m_channel.Transaction("?SYSINFO(13)");
                m_listAxis.m_lAxis = Convert.ToInt32(sAxis.Trim());
            }
            catch (Exception e) { m_log.Error("Get Axis Count Error : " + e.Message); }
        }
        #endregion

        #region Connect
        public Channel m_channel = new Channel();
        bool m_bSimul = true; 
        string m_sIP = "10.0.0.100";
        int m_nPort = 701;

        bool _bConnect = false; 
        public bool p_bConnect
        {
            get { return _bConnect; }
            set
            {
                if (_bConnect == value) return;
                m_listAxis.m_lAxis = 0;
                _bConnect = value;
                if (value)
                {
                    try
                    {
                        if (m_bSimul) m_channel.OpenCommDirect();
                        else m_channel.OpenCommEthernetTCP(m_sIP, m_nPort);
                    }
                    catch (Exception e) 
                    { 
                        m_log.Error("ACS Open Error : " + e.Message);
                        _bConnect = false;
                    }
                    GetAxisCount();
                    InitBuffer(); 
                }
                else m_channel.CloseComm();
                RunTree(Tree.eMode.Init);
            }
        }

        void RunTreeConnect(Tree tree)
        {
            m_bSimul = tree.Set(m_bSimul, m_bSimul, "Simulation", "Simulation Connect");
            m_sIP = tree.Set(m_sIP, m_sIP, "IP Address", "ACS Remote IP Address", !m_bSimul);
            m_nPort = tree.Set(m_nPort, m_nPort, "Port", "ACS Remote Port Number", !m_bSimul); 
        }
        #endregion

        #region Buffer Command
        public class Buffer
        {
            public int m_nBuffer;
            public bool m_bRun = false;

            public void CheckState()
            {
                if (m_acs.p_bConnect == false) return; 
                try { m_bRun = ((m_acs.m_channel.GetProgramState(m_nBuffer) & m_acs.m_channel.ACSC_PST_RUN) != 0); }
                catch (Exception e) { m_acs.m_log.Error(p_id + " Run Error : " + e.Message); }
            }

            public string Run()
            {
                if (m_acs.p_bConnect == false) return m_acs.p_id + " not Connected";
                try
                {
                    m_acs.m_channel.RunBuffer(m_nBuffer);
                    m_acs.m_log.Info(p_id + " Run");
                }
                catch (Exception e) { m_acs.m_log.Error(p_id + " Run Error : " + e.Message); }
                return "OK";
            }

            public string Stop()
            {
                if (m_acs.p_bConnect == false) return m_acs.p_id + " not Connected";
                try
                {
                    m_acs.m_channel.StopBuffer(m_nBuffer);
                    m_acs.m_log.Info(p_id + " Stop");
                }
                catch (Exception e) { m_acs.m_log.Error(p_id + " Run Error : " + e.Message); }
                return "OK";
            }

            string p_id { get; set; }
            ACS m_acs; 
            public Buffer(ACS acs, int nBuffer)
            {
                m_nBuffer = nBuffer;
                p_id = acs.p_id + ".Buffer" + nBuffer.ToString("00"); 
            }
        }

        List<Buffer> m_aBuffer = new List<Buffer>(); 
        void InitBuffer()
        {
            if (p_bConnect == false) return;
            try
            {
                string sBuffer = m_channel.Transaction("?SYSINFO(10)");
                int nBuffer = Convert.ToInt32(sBuffer.Trim());
                while (m_aBuffer.Count < nBuffer) m_aBuffer.Add(new Buffer(this, m_aBuffer.Count));
                while (m_aBuffer.Count > nBuffer) m_aBuffer.RemoveAt(m_aBuffer.Count - 1);
            }
            catch (Exception e) { m_log.Error("Get Axis Count Error : " + e.Message); }
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;
        void InitThread()
        {
            m_thread = new Thread(new ThreadStart(RunThread));
            m_thread.Start(); 
        }

        void RunThread()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(1);
                //forget p_bEnable ??
                m_dio.RunThread(); 
            }
        }
        #endregion

        //=============================================
        #region Init ACS
        bool InitChannel(ref int nInputModule, ref int nOutputModule)
        {
//            CopyDllFile();
            uint uError = CAXL.AxlOpen(7); //Copy Dll : Root/RootTools.Control.ACS/DLL/*.* -> ?.exe 또는 빌드 옵션에서 32bit설정 제거

            if (uError == 0)
            {
//                CheckModuleNum(ref nInputModule, ref nOutputModule);
                return true;
            }
//            else TestModule(ref nInputModule, ref nOutputModule);

            m_log.Error("AXL Init Error (ReStart SW) : " + uError.ToString());
            return false;
        }

        //        bool CopyDllFile(string sPath)
        //        {
        //            DirectoryInfo dir = new DirectoryInfo(sPath);
        //            if (dir.Exists == false) return false;
        //            FileInfo[] file = dir.GetFiles();
        //            for (int i = 0; i < file.Length; i++)
        //            {
        //                file[i].CopyTo(Directory.GetCurrentDirectory() + @"\" + file[i].Name, true);
        //            }
        //            return true;
        //        }
        //        void CopyDllFile()
        //        {
        //            string[] sFindDll = Directory.GetFiles(Directory.GetCurrentDirectory(), "AXL.dll");
        //            if (sFindDll.Length != 0) return;
        //            if (CopyDllFile(@"C:\Program Files (x86)\EzSoftware UC\AXL(Library)\Library\64Bit")) return;
        //            if (CopyDllFile(@"C:\Program Files (x86)\EzSoftware RM\AXL(Library)\Library\64Bit")) return;
        //        }
        #endregion

        #region Search DIO Module
        /*
                public ObservableCollection<ACSModule> p_SearchModule { get; set; }

                void CheckModuleNum(ref int nInputModule, ref int nOutputModule)
                {
                    nInputModule = 0;
                    nOutputModule = 0;
                    uint uStatus = 0;

                    if (CAXD.AxdInfoIsDIOModule(ref uStatus) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                    {
                        if ((AXT_EXISTENCE)uStatus == AXT_EXISTENCE.STATUS_EXIST)
                        {
                            int nModuleCount = 0;

                            if (CAXD.AxdInfoGetModuleCount(ref nModuleCount) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                            {
                                short i = 0;
                                int nBoardNo = 0;
                                int nModulePos = 0;
                                uint uModuleID = 0;
                                for (i = 0; i < nModuleCount; i++)
                                {
                                    if (CAXD.AxdInfoGetModule(i, ref nBoardNo, ref nModulePos, ref uModuleID) == (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                                    {
                                        switch ((AXT_MODULE)uModuleID)
                                        {
                                            case AXT_MODULE.AXT_SIO_DI32:
                                            case AXT_MODULE.AXT_SIO_RDI32MLIII:
                                            case AXT_MODULE.AXT_SIO_RDI32PMLIII:
                                                p_SearchModule.Add(new ACSModule(AXT_MODULE.AXT_SIO_DI32.ToString(), nBoardNo));
                                                nInputModule++;
                                                break;
                                            case AXT_MODULE.AXT_SIO_DO32P:
                                            case AXT_MODULE.AXT_SIO_RDO32MLIII:
                                            case AXT_MODULE.AXT_SIO_RDO32PMLIII:
                                                p_SearchModule.Add(new ACSModule(AXT_MODULE.AXT_SIO_DO32P.ToString(), nBoardNo));
                                                nOutputModule++; break;
                                            case AXT_MODULE.AXT_SIO_DB32P:
                                            case AXT_MODULE.AXT_SIO_RDB32MLIII:
                                            case AXT_MODULE.AXT_SIO_RDB32PMLIII:
                                                p_SearchModule.Add(new ACSModule(AXT_MODULE.AXT_SIO_DB32P.ToString(), nBoardNo));
                                                nInputModule++;
                                                nOutputModule++;
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                void TestModule(ref int nInputModule, ref int nOutputModule)
                {
                    nInputModule = 0;
                    nOutputModule = 0;

                    p_SearchModule.Add(new ACSModule(AXT_MODULE.AXT_SIO_DI32.ToString(), 0));
                    p_SearchModule.Add(new ACSModule(AXT_MODULE.AXT_SIO_RDI32MLIII.ToString(), 1));
                    p_SearchModule.Add(new ACSModule(AXT_MODULE.AXT_SIO_DO32P.ToString(), 2));
                    p_SearchModule.Add(new ACSModule(AXT_MODULE.AXT_SIO_DB32P.ToString(), 3));
                }
        */
        #endregion

        #region ITool
        public string p_id { get; set; }
        #endregion

        #region IControl
        public Axis GetAxis(string id, Log log)
        {
            return m_listAxis.GetAxis(id, log);
        }

        public AxisXY GetAxisXY(string id, Log log)
        {
            return m_listAxis.GetAxisXY(id, log);
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.Init);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            m_listAxis.RunTree(m_treeRoot.GetTree("Axis"));
            m_dio.RunTree(m_treeRoot);
        }
        #endregion

        IEngineer m_engineer;
        public Log m_log;
        public TreeRoot m_treeRoot;
        public ACSDIO m_dio = new ACSDIO();
        public ACSListAxis m_listAxis = new ACSListAxis();

        public void Init(string id, IEngineer engineer)
        {
//            p_SearchModule = new ObservableCollection<ACSModule>();
            int nInput = 0, nOutput = 0;
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            bool bChannel = InitChannel(ref nInput, ref nOutput);
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            m_dio.Init(id + ".DIO", this);
            m_listAxis.Init(id + ".Axis", engineer, this, bChannel);
            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
            InitThread(); 
        }

        public void ThreadStop()
        {
            m_listAxis.ThreadStop();
            m_dio.ThreadStop();
//            CAXL.AxlClose();
        }

    }
}
