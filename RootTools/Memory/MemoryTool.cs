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

namespace RootTools.Memory
{
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
        TCPIPServer m_Server;
        TCPIPClient m_Client;
        bool bServer = true;

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

        void RunTreeTCPSetup(Tree tree)
        {
            bServer = tree.Set(bServer, bServer, "MemServer", "Memory Tool Server");
            if (bServer && m_Server != null)
            {
                m_Server.RunTree(tree);
            }
            if (!bServer && m_Client != null)
                m_Client.RunTree(tree);
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
            RunTreeRun(Tree.eMode.RegRead);
            KillInspectProcess();
            if (bMaster == false) InitTimer();

            if (bServer)
            {
                m_Server = new TCPIPServer(id, m_log);
                RunTreeRun(Tree.eMode.RegRead);
                m_Server.Init();
                m_Server.EventReciveData += M_Server_EventReciveData;
            }
            else
            {
                m_Client = new TCPIPClient(id, m_log);
                RunTreeRun(Tree.eMode.RegRead);
                m_Client.EventReciveData += M_Client_EventReciveData;
            }
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
        BitmapSource m_ReciveBitmapSource;
        public BitmapSource GetOtherMemory(System.Drawing.Rectangle View_Rect, int CanvasWidth, int CanvasHeight)
        {
            string str = "GET" + Splitter + GetSerializeString(View_Rect) + Splitter + CanvasWidth + Splitter + CanvasHeight;
            _bRecieve = false;
            m_Server.Send(str);
            while (_bRecieve)
            {
                Thread.Sleep(100);
            }
            _bRecieve = false;
            return m_ReciveBitmapSource;
        }

        private void M_Server_EventReciveData(byte[] aBuf, int nSize, System.Net.Sockets.Socket socket)
        {
            //socket.Send(aBuf, nSize, SocketFlags.None);
            string str = Encoding.ASCII.GetString(aBuf, 0, nSize);
            //m_qLog.Enqueue(new Mars(0, Encoding.ASCII.GetString(aBuf, 0, nSize)));
            string[] aStr = str.Split(Splitter);
            switch (aStr[0])
            {
                case "GET":
                    m_ReciveBitmapSource = (BitmapSource)GetSerializeObject(aStr[1], m_ReciveBitmapSource.GetType());
                    _bRecieve = true;
                    break;
            }
        }

        private void M_Client_EventReciveData(byte[] aBuf, int nSize, Socket socket)
        {
            string str = Encoding.ASCII.GetString(aBuf, 0, nSize);
            string[] aStr = str.Split(Splitter);
            switch (aStr[0])
            {
                case "GET":
                    System.Drawing.Rectangle rect = new System.Drawing.Rectangle();
                    byte[] res = GetImageView((System.Drawing.Rectangle)(GetSerializeObject(aStr[1], rect.GetType())), Convert.ToInt32(aStr[2]), Convert.ToInt32(aStr[3]));
                    //string strResult = ImageSourceToString(res);
                    //string strResult = GetSerializeString(res);
                    //string strresult = Encoding.Default.GetString(res);
                    //string strresult = bytestostring(res);
                    string strresult = Convert.ToBase64String(res);
                    //string stst = BitConverter.ToString(res);

                    m_Client.Send(strresult);
                    break;
            }
            //System.Drawing.Rectangle viewrect = GetSerializeObject(aStr[1],     );
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



        private unsafe byte[] GetImageView(System.Drawing.Rectangle View_Rect, int CanvasWidth, int CanvasHeight)
        {
            object o = new object();

            Image<Gray, byte> view = new Image<Gray, byte>(CanvasWidth, CanvasHeight);
            MemoryData memdata = GetMemory("pool", "group", "mem");
            IntPtr ptrMem = memdata.GetPtr();
            if (ptrMem == IntPtr.Zero)
                return null;
            int pix_x = 0;
            int pix_y = 0;
            int rectX, rectY, rectWidth, rectHeight, sizeX;
            byte[] result = new byte[CanvasWidth * CanvasHeight];
            //byte* imageptr = (byte*)ptrMem.ToPointer();

            rectX = View_Rect.X;
            rectY = View_Rect.Y;
            rectWidth = View_Rect.Width;
            rectHeight = View_Rect.Height;

            sizeX = Convert.ToInt32(memdata.W);

            Parallel.For(0, CanvasHeight, (yy) =>
            {
                lock (o)
                {
                    pix_y = rectY + yy * rectHeight / CanvasHeight;

                    for (int xx = 0; xx < CanvasWidth; xx++)
                    {
                        pix_x = rectX + xx * rectWidth / CanvasWidth;
                        result[yy * CanvasWidth + xx] = ((byte*)ptrMem)[pix_x + (long)pix_y * sizeX];
                    }
                }
            });
            return result;
        }

        public void SendTest()
        {
            if (bServer)
            {
                m_Server.Send("testserver");
            }
            else
            {
                m_Client.Send("testclient");
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
