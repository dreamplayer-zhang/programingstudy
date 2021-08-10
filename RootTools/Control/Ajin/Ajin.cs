using RootTools.Trees;
using System.Collections.ObjectModel;
using System.IO;

namespace RootTools.Control.Ajin
{
    public class Ajin : IToolSet, IControl
    {
        #region Init AXL
        bool InitCAXL(ref int nInputModule, ref int nOutputModule)
        {
            CopyDllFile();
            uint uError = CAXL.AxlOpen(7); //Copy Dll : Root/RootTools.Control.Ajin/DLL/*.* -> ?.exe 또는 빌드 옵션에서 32bit설정 제거
            if (uError == 0)
            {
                CheckModuleNum(ref nInputModule, ref nOutputModule);
                return true;
            }
            else TestModule(ref nInputModule, ref nOutputModule);
            m_log.Error("AXL Init Error (ReStart SW) : " + uError.ToString());
            return false;
        }

        bool CopyDllFile(string sPath)
        {
            string MainFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            DirectoryInfo dir = new DirectoryInfo(sPath);
            if (dir.Exists == false) return false;
            FileInfo[] file = dir.GetFiles();
            for (int i = 0; i < file.Length; i++)
            {
                file[i].CopyTo(Path.Combine(MainFolder ,file[i].Name), true);
            }
            return true;
        }
        void CopyDllFile()
        {
            string MainFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string[] sFindDll = Directory.GetFiles(MainFolder, "AXL.dll");
            if (sFindDll.Length != 0) return;

            string pathUC = @"C:\Program Files (x86)\EzSoftware UC\AXL(Library)\Library\64Bit";
            string pathRM = @"C:\Program Files (x86)\EzSoftware RM\AXL(Library)\Library\64Bit";

            if (CopyDllFile(pathUC)) return;
            if (CopyDllFile(pathRM)) return;
        }
        #endregion

        #region Search DIO Module
        public ObservableCollection<AJINModule> p_SearchModule { get; set; }

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
                                        p_SearchModule.Add(new AJINModule(AXT_MODULE.AXT_SIO_DI32.ToString(), nBoardNo));
                                        nInputModule++;
                                        break;
                                    case AXT_MODULE.AXT_SIO_DO32P:
                                    case AXT_MODULE.AXT_SIO_RDO32MLIII:
                                    case AXT_MODULE.AXT_SIO_RDO32PMLIII:
                                        p_SearchModule.Add(new AJINModule(AXT_MODULE.AXT_SIO_DO32P.ToString(), nBoardNo));
                                        nOutputModule++; break;
                                    case AXT_MODULE.AXT_SIO_DB32P:
                                    case AXT_MODULE.AXT_SIO_RDB32MLIII:
                                    case AXT_MODULE.AXT_SIO_RDB32PMLIII:
                                        p_SearchModule.Add(new AJINModule(AXT_MODULE.AXT_SIO_DB32P.ToString(), nBoardNo));
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

            p_SearchModule.Add(new AJINModule(AXT_MODULE.AXT_SIO_DI32.ToString(), 0));
            p_SearchModule.Add(new AJINModule(AXT_MODULE.AXT_SIO_RDI32MLIII.ToString(), 1));
            p_SearchModule.Add(new AJINModule(AXT_MODULE.AXT_SIO_DO32P.ToString(), 2));
            p_SearchModule.Add(new AJINModule(AXT_MODULE.AXT_SIO_DB32P.ToString(), 3));
        }
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

        public AxisXZ GetAxisXZ(string id, Log log)
        {
            return m_listAxis.GetAxisXZ(id, log);
        }

        public Axis3D GetAxis3D(string id, Log log)
        {
            return m_listAxis.GetAxis3D(id, log);
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
        Log m_log;
        public TreeRoot m_treeRoot;
        public AjinDIO m_dio = new AjinDIO();
        public AjinListAxis m_listAxis = new AjinListAxis();

        public void Init(string id, IEngineer engineer)
        {
            p_SearchModule = new ObservableCollection<AJINModule>(); 
            int nInput=0, nOutput = 0;
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            bool bAXL = InitCAXL(ref nInput,ref nOutput);
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            m_dio.Init(id + ".DIO", nInput, nOutput);
            m_listAxis.Init(id + ".Axis", engineer, bAXL); 
            
            for (int i = 0; i<m_listAxis.m_aAxis.Count; i++)
            {
                m_listAxis.m_aAxis[i].RunTreeInterlock(Tree.eMode.RegRead);
            }

            RunTree(Tree.eMode.RegRead);
            RunTree(Tree.eMode.Init);
        }

        public void ThreadStop()
        {
            m_listAxis.ThreadStop(); 
            m_dio.ThreadStop();
            CAXL.AxlClose();
        }
    }

    public class AJINModule : ObservableObject
    {
        string m_sModuleName = "";
        public string p_sModuleName
        {
            get
            {
                return m_sModuleName;
            }
            set
            {
                SetProperty(ref m_sModuleName, value);
            }
        }
    
        int m_nModuleNum = -1;
        public int p_nModuleNum
        {
            get
            {
                return m_nModuleNum;
            }
            set
            {
                SetProperty(ref m_nModuleNum, value);
            }
        }
        int m_nOffset = -1;
        public int p_nOffset
        {
            get
            {
                return m_nOffset;
            }
            set
            {
                SetProperty(ref m_nOffset, value);
            }
        }

        public AJINModule(string sName ,int module)
        {
            p_sModuleName = sName;
            m_nModuleNum = module;
        }
    }
}
