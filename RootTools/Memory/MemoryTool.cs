using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using RootTools.Comm;
using RootTools.Inspects;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Serialization;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;

namespace RootTools.Memory
{
    public class AsyncObject
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[(long)BufferSize];
        }

        public void ClearBuffer()
        {
         //   Array.Clear(Buffer, 0, BufferSize);
        }
    }
    public class MemServer
    {
        public delegate void OnReciveData(byte[] aBuf, int nSize);
        public event OnReciveData EventReciveData;
        const int nSize = 1920 * 1080 * 3;

        #region Setting
        int m_port = 5000;
        Socket mainSock;
        Log m_log;
        string p_id;

        void RunTreeSetting(Tree tree)
        {
            m_port = tree.Set(m_port, m_port, "Port", "Port Number");
        }
        #endregion
        
        #region Tree
        public TreeRoot m_treeRoot;
        void InitTree()
        {
            m_treeRoot = new TreeRoot(p_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }

        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTree(m_treeRoot);
        }

        public void RunTree(Tree treeRoot)
        {
            RunTreeSetting(treeRoot.GetTree("Setting"));
        }
        #endregion
        public MemServer(Log log,string id)
        {
            //mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            this.m_log = log;
            this.p_id = id;
            InitTree();
        }
        public void Start()
        {
            try
            {
                mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, m_port);
                mainSock.Bind(serverEP);
                mainSock.Listen(10);
                mainSock.BeginAccept(AcceptCallback, null);
            }
            catch(Exception e)
            {
                m_log?.Error(p_id + " Server Bind & Listen Fail !!");
                m_log?.Error(p_id + " Exception : " + e.Message);
            }
        }
        public void Close()
        {
            if(mainSock != null)
            {
                mainSock.Close();
                mainSock.Dispose();
            }

            foreach(Socket socket in connectedClients)
            {
                socket.Close();
                socket.Dispose();
            }
            connectedClients.Clear();

            //mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        }
        public bool IsConnected()
        {
            return (connectedClients.Count > 0);
        }
        List<Socket> connectedClients = new List<Socket>();
        void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // 클라이언트의 연결 요청을 수락한다.

                Socket client = mainSock.EndAccept(ar);














































































































































































                // 또 다른 클라이언트의 연결을 대기한다.
                mainSock.BeginAccept(AcceptCallback, null);

                AsyncObject obj = new AsyncObject(nSize);
                obj.WorkingSocket = client;

                m_log.Info(p_id + " is Accepted !!");

                // 연결된 클라이언트 리스트에 추가해준다.
                connectedClients.Add(client);

                // 클라이언트의 데이터를 받는다.
                client.BeginReceive(obj.Buffer, 0, nSize, 0, DataReceived, obj);
            }
            catch(Exception e)
            {
                m_log.Info("AcceptCallback : " + e.Message);
            }
            
        }
        void DataReceived(IAsyncResult ar)
        {          
            AsyncObject obj = (AsyncObject)ar.AsyncState;

            try
            {
                if (obj.WorkingSocket.Connected == false)
                {
                    connectedClients.Remove(obj.WorkingSocket);

                    m_log.Warn("Server : Disconnected from the client");
                    obj.WorkingSocket.Close();
                    return;
                }
                int received = obj.WorkingSocket.EndReceive(ar);
                if (received <= 0)
                {
                    connectedClients.Remove(obj.WorkingSocket);

                    m_log.Warn("Server : Received nothing from the client");
                    obj.WorkingSocket.Close();
                    return;
                }

                EventReciveData(obj.Buffer, received);

                obj.ClearBuffer();

                obj.WorkingSocket.BeginReceive(obj.Buffer, 0, nSize, 0, DataReceived, obj);
            }
            catch (Exception e)
            {
                connectedClients.Remove(obj.WorkingSocket);

                m_log.Warn("Server : Disconnected from the client");
                obj.WorkingSocket.Close();

                mainSock.Listen(10);
                mainSock.BeginAccept(AcceptCallback, null);
                return;
            }

        }
        public void Send(byte[] p)
        {
            for (int i = connectedClients.Count - 1; i >= 0; i--)
            {
                Socket socket = connectedClients[i];
                try
                {
                    socket.Send(p);
                }
                catch (Exception e)
                {
                    // 오류 발생하면 전송 취소하고 리스트에서 삭제한다.
                    try
                    {
                        socket.Dispose();
                    }
                    catch { }
                    connectedClients.RemoveAt(i);
                }
            }
        }
        public void Send(string p)
        {
            for (int i = connectedClients.Count - 1; i >= 0; i--)
            {
                Socket socket = connectedClients[i];
                try
                {
                    byte[] b = Encoding.ASCII.GetBytes(p);
                    socket.Send(b);
                }
                catch (Exception e)
                {
                    // 오류 발생하면 전송 취소하고 리스트에서 삭제한다.
                    try
                    {
                        socket.Dispose();
                    }
                    catch { }
                    connectedClients.RemoveAt(i);
                }
            }
        }
    }
    public class MemClient
    {
        public delegate void OnReciveData(byte[] aBuf, int nSize);
        public event OnReciveData EventReciveData;

        #region Setting
        Socket mainSock;
        IPAddress serverAddr;
        string m_strAddr= "10.0.0.10";
        int m_port = 5000;
        Log m_log;
        string p_id;
        public TreeRoot m_treeRoot;

        void RunTreeSetting(Tree tree)
        {
            m_strAddr = tree.Set(m_strAddr, m_strAddr, "IP", "IP Address");
            serverAddr = IPAddress.Parse(m_strAddr);
            m_port = tree.Set(m_port, m_port, "Port", "Port Number");
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeSetting(m_treeRoot.GetTree("Setting"));
        }

        public void RunTree(Tree treeRoot)
        {
            RunTreeSetting(treeRoot.GetTree("Setting"));
        }
        #endregion
        public MemClient(Log log,string id)
        {
            this.m_log = log;
            this.p_id = id;
            //mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            m_treeRoot = new TreeRoot(id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }
        public void Connect()
        {
            try
            {
                if(mainSock == null)
                    mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

                IPEndPoint clientEP = new IPEndPoint(serverAddr, m_port);
                mainSock.BeginConnect(clientEP, new AsyncCallback(ConnectCallback), mainSock);
            }
            catch (Exception e)
            {
                m_log.Info("Connect : " + e.Message);
            }
        }
        public void Close()
        {
            if(mainSock != null)
            {
                mainSock.Close();
                mainSock.Dispose();
            }

            //mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        }
        public bool IsConnected()
        {
            return mainSock.Connected;
        }
        void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                if (client.Connected == false) return;
                client.EndConnect(ar);

                m_log.Info(p_id + " is connected !!");

                AsyncObject obj = new AsyncObject(4096);
                obj.WorkingSocket = mainSock;
                mainSock.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);
            }
            catch(Exception e)
            {
                m_log.Info("ConnectCallback : " + e.Message);

                Connect();
            }
        }

        void DataReceived(IAsyncResult ar)
        {
            // BeginReceive에서 추가적으로 넘어온 데이터를 AsyncObject 형식으로 변환한다.
            AsyncObject obj = (AsyncObject)ar.AsyncState;
            try
            {
                // 데이터 수신을 끝낸다.
                int received = obj.WorkingSocket.EndReceive(ar);

                // 받은 데이터가 없으면(연결끊어짐) 끝낸다.
                if (received <= 0)
                {
                    obj.WorkingSocket.Close();
                    return;
                }

                EventReciveData(obj.Buffer, received);

                obj.ClearBuffer();
                // 수신 대기
                obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
            }
            catch(SocketException e)
            {
                m_log.Warn("Client : Disconnected from the server");
                //obj.WorkingSocket.Disconnect(true);
                Close();

                Connect();
            }
            catch(Exception e)
            {
                m_log.Warn("Client : Disconnected from the server");
                //obj.WorkingSocket.Disconnect(true);
                Close();
            }
            
        }
        public void Send(byte[] p)
        {
            mainSock.Send(p);
        }
    }
    public class MemoryTool : ObservableObject, IToolSet
    {
        const double c_fGB = 1024 * 1024 * 1024;

        #region Property
        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (_sInfo == value) return;
                _sInfo = value;
                if (value == "OK") return;
                m_log.Warn("p_sInfo = " + value);
            }
        }
        
        double m_fTotalPageFile = 0;
        public double p_fTotalPageFile
        {
            get { return m_fTotalPageFile; } 
            set { SetProperty(ref m_fTotalPageFile, value); }
        }
        
        double m_fAvailPageFile = 0;
        public double p_fAvailPageFile
        {
            get { return m_fAvailPageFile; }
            set { SetProperty(ref m_fAvailPageFile, value); }
        }
        
        double m_fNotRootPageFile = 0;
        public double p_fNotRootPageFile
        {
            get { return m_fNotRootPageFile; } 
            set { SetProperty(ref m_fNotRootPageFile, value); }
        }
        #endregion

        #region Memory Pool
        public delegate void dgOnChangeMemoryPool();
        public event dgOnChangeMemoryPool OnChangeMemoryPool;

        public List<string> m_asPool = new List<string>(); 
        public void MemoryPoolChanged()
        {
            m_asPool.Clear();
            foreach (MemoryPool pool in p_aPool) m_asPool.Add(pool.p_id);
            if (OnChangeMemoryPool != null) OnChangeMemoryPool();
            RunTreeRun(Tree.eMode.Init); 
        }

        ObservableCollection<MemoryPool> _aPool = new ObservableCollection<MemoryPool>();
        public ObservableCollection<MemoryPool> p_aPool
        {
            get { return _aPool; }
            set { SetProperty(ref _aPool, value); }
        }

        public MemoryPool CreatePool(string sPool, double fGB)
          {
            MemoryPool memoryPool = new MemoryPool(sPool, this, fGB);
            p_aPool.Add(memoryPool);
            MemoryChanged(); 
            MemoryPoolChanged();
            return memoryPool;
        }

        public MemoryPool GetPool(string sPool)
        {
            foreach (MemoryPool pool in p_aPool)
            {
                if (pool.p_id == sPool) return pool;
            }
            return null; 
        }

        public string DeletePool(string sPool)
        {
            MemoryPool memoryPool = GetPool(sPool);
            if (memoryPool != null) return "Memory Pool Not Exist";
            p_aPool.Remove(memoryPool);
            MemoryPoolChanged();
            return "OK";
        }
        #endregion

        #region Memoey
        DispatcherTimer m_timer = new DispatcherTimer();
        void InitTimer()
        {
            m_timer.Interval = TimeSpan.FromSeconds(0.2);
            m_timer.Tick += m_timer_Tick;
            m_timer.Start();
        }

        const string c_sBusy = "Busy";
        void m_timer_Tick(object sender, EventArgs e)
        {
            MEMORYSTATUSEX stats = GlobalMemoryStatusEx();
            p_fTotalPageFile = stats.ullTotalPageFile / c_fGB;
            p_fAvailPageFile = stats.ullAvailPageFile / c_fGB;
            if (m_bMaster) return; 
            string sUpdateTime = m_reg.Read("Time", c_sBusy);
            if (sUpdateTime == c_sBusy) return;
            if (sUpdateTime == m_sUpdateTime) return;
            if (UpdateMemoryData()) return;
            m_sUpdateTime = sUpdateTime;
            OnChangeMemoryPool();
        }

        bool UpdateMemoryData()
        {
            p_aPool.Clear(); 
            int nPool = m_reg.Read("Count", 0); 
            for (int n = 0; n < nPool; n++)
            {
                string sPool = m_reg.Read("MemoryPool" + n.ToString(), "");
                MemoryPool pool = new MemoryPool(sPool, this, 1);
                p_aPool.Add(pool);
            }
            return false; 
        }

        string m_sUpdateTime = ""; 
        public void MemoryChanged()
        {
            if (m_bMaster == false) return;
            m_reg.Write("Count", p_aPool.Count); 
            for (int n = 0; n < p_aPool.Count; n++)
            {
                m_reg.Write("MemoryPool" + n.ToString(), p_aPool[n].p_id);
                p_aPool[n].RunTree(Tree.eMode.RegWrite); 
            }
            KillInspectProcess();
            RunTreeRun(Tree.eMode.Init);
            NotifyMemoryChange(); 
        }

        public void NotifyMemoryChange()
        {
            if (m_bThreadProcess == false) m_reg.Write("Time", c_sBusy);
            else
            {
                m_sUpdateTime = DateTime.Now.ToString();
                m_reg.Write("Time", m_sUpdateTime);
            }
        }

        public MemoryData GetMemory(string sPool, string sGroup, string sMemory)
        {
            MemoryPool pool = GetPool(sPool);
            return (pool == null) ? null : pool.GetMemory(sGroup, sMemory);
        }
        #endregion

        #region MemoryProcess
        bool m_bThreadProcess = false;
        Thread m_threadProcess = null;
        MemServer m_Server;
        MemClient m_Client;

        //TCPAsyncClient m_Client;
        //TCPAsyncServer m_Server;

        bool m_bChanged = true;
        bool m_bServer = true;
        bool p_bServer
        {
            get => m_bServer;
            set
            {
                if (!m_bChanged && m_bServer == value) return;
                m_bChanged = false;
                
                m_bServer = value;
                if(m_bUseServer)
                {
                    if(m_bServer)
                    {
                        if (m_Client != null)
                        {
                            m_Client.Close();
                        }
                        m_Client = new MemClient(m_log, p_id);
                        m_Client.EventReciveData += M_Client_EventReciveData;

                        m_Server.Start();
                    }
                    else
                    {
                        if (m_Server != null)
                        {
                            m_Server.Close();
                        }
                        m_Server = new MemServer(m_log, p_id);
                        m_Server.EventReciveData += M_Server_EventReciveData;

                        m_Client.Connect();
                    }
                }
                else
                {
                    if (m_Client != null)
                        m_Client.Close();

                    if (m_Server != null)
                        m_Server.Close();
                }
            }
        }
        //int nPort = 5000;

        public void InitThreadProcess()
        {
            m_bThreadProcess = true;
            m_threadProcess = new Thread(new ThreadStart(RunThreadProcess));
            m_threadProcess.Start();
            NotifyMemoryChange(); 
        }

        void RunThreadProcess()
        {
            Thread.Sleep(1000);
            while (m_bThreadProcess)
            {
                Thread.Sleep(1000);
                if (m_bStartProcess)
                {
                    try
                    {
                        Process[] aProcess = Process.GetProcessesByName(m_idProcess);
                        if (aProcess.Length == 0)
                        {
                            Process.Start(m_sProcessFile);
                        }
                    }
                    catch (Exception e) { p_sInfo = p_id + " StartProcess Error : " + e.Message; }
                }
            }
        }

        bool m_bStartProcess = false;
        string m_idProcess = "Root_Memory";
        string m_sProcessFile = "";
        void RunTreeProcess(Tree tree, bool bVisible)
        {
            m_bStartProcess = tree.Set(m_bStartProcess, m_bStartProcess, "Start", "Start Memory Process", bVisible);
            m_idProcess = tree.Set(m_idProcess, m_idProcess, "ID", "Memory Process ID", bVisible && m_bStartProcess);
            m_sProcessFile = tree.SetFile(m_sProcessFile, m_sProcessFile, "exe", "File", "Process File Name", bVisible && m_bStartProcess);
        }
        bool m_bUseServer = false;

        void RunTreeTCPSetup(Tree tree)
        {
            bool bUseServer = tree.Set(m_bUseServer, m_bUseServer, "Use Memory Server", "Use Mem Server");

            bool bServer = false;
            if (bUseServer)
            {
                bServer = tree.Set(p_bServer, p_bServer, "MemServer", "Memory Tool Server");
                if (bServer)
                {
                    if (m_Server != null)
                        m_Server.RunTree(tree);
                }
                else
                {
                    if (m_Client != null)
                        m_Client.RunTree(tree);
                }
            }
            else
            {
                if (m_Client != null)
                    m_Client.Close();

                if (m_Server != null)
                    m_Server.Close();
            }

            m_bUseServer = bUseServer;

            p_bServer = bServer;
        }

        #endregion

        #region Read & Save Memory
        public string SaveMemory()
        {
            if (m_memory == null) return "Memory not Exist";
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Image RAW File (*.raw)|*.raw";
            if (dlg.ShowDialog() == false) return "Save File Dialog not OK";
            return m_memory.SaveMemory(dlg.FileName);
        }

        public string ReadMemory()
        {
            if (m_memory == null) return "Memory not Exist";
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image RAW File (*.raw)|*.raw";
            if (dlg.ShowDialog() == false) return "RAW File not Found !!";
            return m_memory.ReadMemory(dlg.FileName);
        }

        string m_sPool = "Pool";
        string m_sGroup = "Group";
        string m_sMemory = "Memory";
        MemoryData m_memory = null;
        void RunTreeFile(Tree tree)
        {
            m_memory = null;
            m_sPool = tree.Set(m_sPool, "Pool", m_asPool, "Pool", "Pool Name");
            MemoryPool pool = GetPool(m_sPool);
            if (pool == null) return;
            m_sGroup = tree.Set(m_sGroup, m_sGroup, pool.m_asGroup, "Group", "Group Name");
            MemoryGroup group = pool.GetGroup(m_sGroup, false);
            if (group == null) return;
            m_sMemory = tree.Set(m_sMemory, m_sMemory, group.m_asMemory, "Name", "Memory Name");
            m_memory = group.GetMemory(m_sMemory);
            if (m_memory == null) return;
        }
        #endregion

        #region Tree
        private void M_treeRootRun_UpdateTree()
        {
            RunTreeRun(Tree.eMode.Update);
            RunTreeRun(Tree.eMode.RegWrite);
            RunTreeRun(Tree.eMode.Init);
        }

        public void RunTreeRun(Tree.eMode mode)
        {
            m_treeRootRun.p_eMode = mode;
            RunTreeFile(m_treeRootRun.GetTree("File"));
            if (m_bMaster == false) return; 
            bool bVisible = true; 
            RunTreeProcess(m_treeRootRun.GetTree("Process"), bVisible);
            RunTreeTCPSetup(m_treeRootRun.GetTree("TCP Set"));
        }
        #endregion

        #region Inspect Process
        public void KillInspectProcess()
        {
            if (m_bMaster == false) return;
            Process[] aProcess = Process.GetProcessesByName(InspectTool.m_idProcess);
            foreach (Process process in aProcess) process.Kill();
        }
        #endregion

        public string p_id { get; set; }
        public bool m_bMaster = true; 
        IEngineer m_engineer;
        public Log m_log;
        Registry m_reg; 
        public TreeRoot m_treeRootRun;
        public MemoryTool(string id, IEngineer engineer, bool bMaster = true)
        {
            MEMORYSTATUSEX stats = GlobalMemoryStatusEx();
            p_fTotalPageFile = stats.ullTotalPageFile / c_fGB;
            p_fAvailPageFile = stats.ullAvailPageFile / c_fGB;
            p_fNotRootPageFile = p_fTotalPageFile - p_fAvailPageFile;
            p_id = id;
            m_engineer = engineer;
            m_bMaster = bMaster; 
            m_log = LogView.GetLog(id);
            m_reg = new Registry("MemoryTool", "MemoryTools");
            if (bMaster) NotifyMemoryChange(); 
            m_treeRootRun = new TreeRoot(id, m_log);
            m_treeRootRun.UpdateTree += M_treeRootRun_UpdateTree;
            KillInspectProcess();
            if (bMaster == false) InitTimer();

            //if (!bUseServer) return;

            m_Server = new MemServer(m_log,id);
            m_Server.EventReciveData += M_Server_EventReciveData;
            
            m_Client = new MemClient(m_log, id);
            m_Client.EventReciveData += M_Client_EventReciveData;

            if (m_bServer)
            {
                //m_Server.RunTree(Tree.eMode.RegRead);
                //m_Server.Start();
                //m_Server.EventReciveData += M_Server_EventReciveData;
                //m_Server = new MemServer(m_log);
                //RunTreeRun(Tree.eMode.RegRead);
                //m_Server.Start(nPort);
                //m_Server.EventReciveData += M_Server_EventReciveData;
            }
            else
            {
                //m_Client.RunTree(Tree.eMode.RegRead);
                //m_Client.Connect();
                //m_Client.EventReciveData += M_Client_EventReciveData;
            }
            RunTreeRun(Tree.eMode.RegRead);


        }

        public void ThreadStop()
        {
            if (m_bThreadProcess)
            {
                m_bThreadProcess = false;
                m_threadProcess.Join(); 
            }
        }

        #region TCP
        const char Splitter = '+';
        bool _bRecieve = false;
        byte[] m_abuf;
        public byte[] GetOtherMemory(System.Drawing.Rectangle View_Rect, int CanvasWidth, int CanvasHeight,  string sPool, string sGourp, string sMem, int nByte, int nCount)
        {  
            Stopwatch watch = new Stopwatch();
            watch.Start();
            string str = "GET" + Splitter + GetSerializeString(View_Rect) + Splitter + CanvasWidth + Splitter + CanvasHeight + Splitter + sPool+ Splitter + sGourp + Splitter + sMem + Splitter + nByte + Splitter + nCount;
           
            _bRecieve = true;
            if (m_Server == null)
                return m_abuf;

            if(m_Server.IsConnected())
                m_Server.Send(str);
            else
                return m_abuf;
        
            while (_bRecieve)
            {
                if (!m_Server.IsConnected())
                    break;

                Thread.Sleep(5);
                if (watch.ElapsedMilliseconds > 10000)
                    return m_abuf;
            }
            _bRecieve = false;
            m_log.Warn(watch.ElapsedMilliseconds.ToString());
            return m_abuf;
        }


        public async Task<byte[]> GetOtherMemoryAsync(System.Drawing.Rectangle View_Rect, int CanvasWidth, int CanvasHeight, string sPool, string sGourp, string sMem, int nByte, int nCount)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            string str = "GET" + Splitter + GetSerializeString(View_Rect) + Splitter + CanvasWidth + Splitter + CanvasHeight + Splitter + sPool + Splitter + sGourp + Splitter + sMem + Splitter + nByte + Splitter + nCount;

            _bRecieve = true;
            if (m_Server == null)
                return m_abuf;

            if (m_Server.IsConnected())
                m_Server.Send(str);
            else
                return m_abuf;

            while (_bRecieve)
            {
                if (!m_Server.IsConnected())
                    break;

                await Task.Delay(5);
                if (watch.ElapsedMilliseconds > 10000)
                    return m_abuf;
            }
            _bRecieve = false;
            m_log.Warn(watch.ElapsedMilliseconds.ToString());
            return m_abuf;
        }
        private void M_Server_EventReciveData(byte[] aBuf, int nSize)
        {
            //socket.Send(aBuf, nSize, SocketFlags.None);
            //string str = Encoding.Default.GetString(aBuf, 0, nSize);
            //m_qLog.Enqueue(new Mars(0, Encoding.ASCII.GetString(aBuf, 0, nSize)));
            //string[] aStr = str.Split(Splitter);
            //string astr = str;

            m_abuf = aBuf;// Encoding.Default.GetBytes(str);//            Convert.FromBase64String(str);
           // m_abuf = Decompress(m_abuf);
            _bRecieve = false;
            //switch (aStr)
            //{
            //    case "GET":
            //m_ReciveBitmapSource = StringToImageSource(astr);

            //      m_ReciveBitmapSource = (BitmapSource)GetSerializeObject(aStr, m_ReciveBitmapSource.GetType());
            //        _bRecieve = true;
            //        break;
            //}
        }

        private void M_Server_EventReciveData(byte[] aBuf, int nSize,Socket socket)
        {
            //socket.Send(aBuf, nSize, SocketFlags.None);
            //string str = Encoding.Default.GetString(aBuf, 0, nSize);
            //m_qLog.Enqueue(new Mars(0, Encoding.ASCII.GetString(aBuf, 0, nSize)));
            //string[] aStr = str.Split(Splitter);
            //string astr = str;

            m_abuf = aBuf;// Encoding.Default.GetBytes(str);//            Convert.FromBase64String(str);
                          // m_abuf = Decompress(m_abuf);
            _bRecieve = false;
            //switch (aStr)
            //{
            //    case "GET":
            //m_ReciveBitmapSource = StringToImageSource(astr);

            //      m_ReciveBitmapSource = (BitmapSource)GetSerializeObject(aStr, m_ReciveBitmapSource.GetType());
            //        _bRecieve = true;
            //        break;
            //}
        }

        private void M_Client_EventReciveData(byte[] aBuf, int nSize)
        {
            string str = Encoding.ASCII.GetString(aBuf, 0, nSize);
            string[] aStr = str.Split(Splitter);
            switch (aStr[0])
            {
                case "GET":
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                    byte[] res = GetImageView((System.Drawing.Rectangle)(GetSerializeObject(aStr[1], rect.GetType())), Convert.ToInt32(aStr[2]), Convert.ToInt32(aStr[3]), Convert.ToString(aStr[4]), Convert.ToString(aStr[5]), Convert.ToString(aStr[6]), Convert.ToInt32(aStr[7]), Convert.ToInt32(aStr[8]));
                   // res = Compress(res);
                    m_Client.Send(res);
                    break;
            }
            //System.Drawing.Rectangle viewrect = GetSerializeObject(aStr[1],     );
        }

        private void M_Client_EventReciveData(byte[] aBuf, int nSize,Socket socket)
        {
            string str = Encoding.ASCII.GetString(aBuf, 0, nSize);
            string[] aStr = str.Split(Splitter);
            switch (aStr[0])
            {
                case "GET":
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                    byte[] res = GetImageView((System.Drawing.Rectangle)(GetSerializeObject(aStr[1], rect.GetType())), Convert.ToInt32(aStr[2]), Convert.ToInt32(aStr[3]), Convert.ToString(aStr[4]), Convert.ToString(aStr[5]), Convert.ToString(aStr[6]), Convert.ToInt32(aStr[7]), Convert.ToInt32(aStr[8]));
                     //res = Compress(res);
                    m_Client.Send(res);
                    //m_Client.Send(Encoding.Default.GetString(res));
                    break;
            }
            //System.Drawing.Rectangle viewrect = GetSerializeObject(aStr[1],     );
        }
        public static Byte[] Compress(Byte[] buffer)
        {
            Byte[] compressedByte;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
                {
                    ds.Write(buffer, 0, buffer.Length);
                }

                compressedByte = ms.ToArray();
            }
            return compressedByte;
        }
        public static Byte[] Decompress(Byte[] buffer)
        {
            MemoryStream resultStream = new MemoryStream();

            using (MemoryStream ms = new MemoryStream(buffer))
            {
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
                {
                    ds.CopyTo(resultStream);
                    ds.Close();
                }
            }
            Byte[] decompressedByte = resultStream.ToArray();
            resultStream.Dispose();
            return decompressedByte;
        }
        static string bytestostring(byte[] bytesss)
        {
            using (MemoryStream stream = new MemoryStream(bytesss))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private string ImageSourceToString(BitmapSource imageSource)
        {
            byte[] bytes = null;
            var bitmapSource = imageSource as BitmapSource;
            var encoder = new BmpBitmapEncoder();
            if (bitmapSource != null)
            {
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                using (var stream = new MemoryStream())
                {
                    encoder.Save(stream);
                    bytes = stream.ToArray();
                }
            }
            return Convert.ToBase64String(bytes);
        }
        private BitmapSource StringToImageSource(string str)
        {
            byte[] bytes = Convert.FromBase64String(str);
            var bitImg = new BitmapImage();
            BitmapSource imageSource = null;
            using (var stream = new MemoryStream(bytes))
            {
                bitImg.BeginInit();
                bitImg.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bitImg.CacheOption = BitmapCacheOption.OnLoad;
                bitImg.StreamSource = stream;
                bitImg.EndInit();
                imageSource = bitImg as BitmapSource;
            }
            return imageSource;
        }



        private unsafe byte[] GetImageView(System.Drawing.Rectangle View_Rect, int CanvasWidth, int CanvasHeight, string sPool, string sGroup, string sMem, int nByte, int nCount)
        {
            object o = new object();

            //Image<Gray, byte> view = new Image<Gray, byte>(CanvasWidth, CanvasHeight);
            MemoryData memdata = GetMemory(sPool, sGroup, sMem);
            if (memdata == null)
                return new byte[(long)CanvasWidth * CanvasHeight * nByte * nCount];

            IntPtr ptrMem = memdata.GetPtr();
            IntPtr ptrMem2 = memdata.GetPtr(1); // G
            IntPtr ptrMem3 = memdata.GetPtr(2); // B

            if (ptrMem == IntPtr.Zero)
                return null;
           
            int rectX, rectY, rectWidth, rectHeight, sizeX;
            byte[] result = new byte[(long)CanvasWidth * CanvasHeight * nByte * nCount];
            rectX = View_Rect.X;
            rectY = View_Rect.Y;
            rectWidth = View_Rect.Width;
            rectHeight = View_Rect.Height;
            sizeX = Convert.ToInt32(memdata.W);

            //byte[,,] viewptr = view.Data;
            //byte* imageptr = (byte*)ptrMem.ToPointer();
            switch(nByte * nCount)
            {
                case 1:
                    {
                        Parallel.For(0, CanvasHeight, (yy) =>
                        {
                            int pix_y = rectY + yy * rectHeight / CanvasHeight;

                            for (int xx = 0; xx < CanvasWidth; xx++)
                            {
                                int pix_x = rectX + xx * rectWidth / CanvasWidth;
                                result[yy * CanvasWidth + xx] = ((byte*)ptrMem)[pix_x + (long)pix_y * sizeX];
                            }
                        });
                    }
                    break;
                case 2:
                    {
                        Parallel.For(0, CanvasHeight, (yy) =>
                        {
                            int pix_y = rectY + yy * rectHeight / CanvasHeight;

                            for (int xx = 0; xx < CanvasWidth; xx++)
                            {
                                long pix_x = rectX + xx * rectWidth / CanvasWidth;
                                byte b1 = ((byte*)ptrMem)[(long)pix_y * sizeX + pix_x * nByte + 0];
                                byte b2 = ((byte*)ptrMem)[(long)pix_y * sizeX + pix_x * nByte + 1];

                                result[(yy * CanvasWidth + xx) * 2 + 0] = b1;
                                result[(yy * CanvasWidth + xx) * 2 + 1] = b2;
                            }
                        });
                    }
                    break;
                case 3:
                    {
                        if (ptrMem != null && ptrMem2 != null && ptrMem3 != null)
                        {
                            int nTerm = CanvasWidth * CanvasHeight;
                            Parallel.For(0, CanvasHeight, new ParallelOptions { MaxDegreeOfParallelism = 12 }, (yy) =>
                            {
                                int pix_y = rectY + yy * rectHeight / CanvasHeight;

                                for (int xx = 0; xx < CanvasWidth; xx++)
                                {
                                    int pix_x = rectX + xx * rectWidth / CanvasWidth;
                                    result[yy * CanvasWidth + xx] = ((byte*)ptrMem)[pix_x + (long)pix_y * sizeX];
                                    result[yy * CanvasWidth + xx + nTerm] = ((byte*)ptrMem2)[pix_x + (long)pix_y * sizeX];
                                    result[yy * CanvasWidth + xx + nTerm * 2] = ((byte*)ptrMem3)[pix_x + (long)pix_y * sizeX];
                                }
                            });
                        }
                    }
                    break;
                default:
                    break;
            }
            
            return result;
        }

        public void SendTest()
        {
            if (m_bServer)
            {
              //  m_Server.Send("testserver");
            }
            else
            {
               // m_Client.Send("testclient");
            }
        }

        public string GetSerializeString(object obj)
        {
            string result;
            XmlSerializer xmlSerializer;
            StringWriter textWriter = new StringWriter();
            xmlSerializer = new XmlSerializer(obj.GetType());
            System.IO.Stream stream = new System.IO.MemoryStream();
            xmlSerializer.Serialize(textWriter, obj);
            result = textWriter.ToString();
            textWriter.Dispose();
            return result;
        }

        public object GetSerializeObject(string str, Type type)
        {
            object result;
            XmlSerializer xmlSerializer;
            StringReader xmlReader;
            xmlSerializer = new XmlSerializer(type);
            xmlReader = new StringReader(str);
            result = xmlSerializer.Deserialize(xmlReader);
            xmlReader.Dispose();
            return result;
        }

        #endregion

        #region MemCheck
        [DllImport("kernel32", EntryPoint = "GetLastError")]
        private extern static int __GetLastError();
        [DllImport("Kernel32.dll", EntryPoint = "GlobalMemoryStatusEx", SetLastError = true)]
        private extern static bool __GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        public static MEMORYSTATUSEX GlobalMemoryStatusEx()
        {
            MEMORYSTATUSEX memstat = new MEMORYSTATUSEX();

            memstat.dwLength = (uint)Marshal.SizeOf(memstat);
            if (__GlobalMemoryStatusEx(ref memstat) == false)
            {
                int error = __GetLastError();
                throw new Win32Exception(error);
            }
            return memstat;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }
        #endregion 
    }

    #region Converter
    public class TestConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                double fFullPage = ((MemoryTool)((MemoryTool_UI)values[2]).DataContext).p_fTotalPageFile;
                double FullWidth = (double)values[1];
                int nPoolGb = (int)values[0];
                double width = FullWidth * nPoolGb / fFullPage;
                return width;
            }
            catch
            {
                return 0;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    public class TestConverter2 : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                MemoryTool tool = ((MemoryTool)((MemoryTool_UI)values[1]).DataContext);
                double fFullPage = tool.p_fTotalPageFile;
                double fAvailPage = tool.p_fAvailPageFile;
                double FullWidth = (double)values[0];
                int FullGb = 0;
                for (int i = 0; i < tool.p_aPool.Count; i++)
                {
                    FullGb += (int)Math.Round(tool.p_aPool[i].p_fGB);
                }
                double width = FullWidth * (fFullPage - fAvailPage - FullGb) / fFullPage;
                return width;
            }
            catch
            {
                return 0;
            }
        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }
    #endregion 
}
