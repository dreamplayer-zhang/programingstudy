using RootTools.Trees;
using System.Collections.Generic;
using System.Threading;

namespace RootTools.RTC5s
{
    public class RTC5 : NotifyProperty
    {
        public enum eState
        {
            Init,
            Send,
            Marking
        }
        eState _eState = eState.Init;
        public eState p_eState
        {
            get { return _eState; }
            set
            {
                _eState = value;
                OnPropertyChanged();
            }
        }

        #region Buffer
        class Buffer : NotifyProperty
        {
            public static uint c_lBuffer = 0x70000;
            public enum eState
            {
                Init,
                Send,
                Ready,
                Mark
            }
            eState _eState = eState.Init;
            public eState p_eState
            {
                get { return _eState; }
                set
                {
                    _eState = value;
                    OnPropertyChanged();
                }
            }

            StopWatch m_swSend = new StopWatch();
            public void SendStart()
            {
                m_swSend.Start();
                p_eState = eState.Send;
            }

            public string SendEnd()
            {
                p_eState = eState.Ready;
                RTC5Wrap.n_set_end_of_list(m_uHead);
                string sP = (100.0 * (RTC5Wrap.n_get_input_pointer(m_uHead) - m_uOffset) / c_lBuffer).ToString(".0");
                return m_id + ", " + sP + "%, " + m_swSend.p_sTime;
            }

            public uint GetFreeSize()
            {
                return c_lBuffer - (RTC5Wrap.n_get_input_pointer(m_uHead) - m_uOffset);
            }

            public string m_id;
            uint m_uHead;
            public uint m_uIndex;
            uint m_uOffset;
            public Buffer(uint uHead, uint uIndex)
            {
                m_uHead = uHead;
                m_uIndex = uIndex;
                m_id = uIndex.ToString();
                m_uOffset = (uint)(uIndex * c_lBuffer);
            }
        }
        Buffer[] m_aBuffer = new Buffer[2];
        void InitBuffer()
        {
            m_aBuffer[0] = new Buffer(p_uHead, 1);
            m_aBuffer[1] = new Buffer(p_uHead, 2);
        }
        #endregion

        #region Setting
        public RTC5Setting m_setting;
        public uint p_uHead { get { return m_setting.p_uHead; } }
        #endregion

        #region Design
        public RTC5Design m_design;
        #endregion

        #region RTC5Mark
        /// <summary> RTC5_UI 에서 Test Mark 를 하기 위해 만든 List </summary>
        List<RTC5Mark> m_aMarkTest = new List<RTC5Mark>();
        void RunTreeMarkTest(Tree tree)
        {
            for (int n = 0; n < m_aMarkTest.Count; n++)
            {
                RTC5Mark mark = m_aMarkTest[n];
                mark.RunTree(tree.GetTree("Mark." + n.ToString("00"), false));
            }
        }

        public void ClearMarkTest()
        {
            m_aMarkTest.Clear();
            RunTree(Tree.eMode.Init);
        }

        public void AddMarkTest()
        {
            RTC5Mark mark = new RTC5Mark(this, true);
            m_aMarkTest.Add(mark);
            RunTree(Tree.eMode.Init);
        }

        public void MarkTest()
        {
            if (p_eState != eState.Init) return;
            foreach (RTC5Mark mark in m_aMarkTest) Add(mark);
            p_eState = eState.Marking;
        }
        #endregion

        #region public Function 
        /// <summary> RTC5 Marking 용 Queue </summary>
        Queue<RTC5Mark> m_qMark = new Queue<RTC5Mark>();

        /// <summary> RTC5 Marking 용 Queue 에 MarkData 추가 </summary>
        public void Add(RTC5Mark mark)
        {
            m_qMark.Enqueue(mark);
        }

        /// <summary> RTC5 Marking 용 Queue 초기화 </summary>
        public void Clear()
        {
            m_qMark.Clear();
        }

        public void Reset(string sError)
        {
            RTC5Wrap.n_stop_execution(p_uHead);
            Clear();
            m_bufferSend = null;
            m_bufferMark = null;
            m_qMarkBuffer.Clear();
            p_eState = eState.Init;
            m_aBuffer[0].p_eState = Buffer.eState.Init;
            m_aBuffer[1].p_eState = Buffer.eState.Init;
            m_log.Error(sError);
        }

        public string Mark(RTC5Mark mark)
        {
            if (m_bufferMark != null) return "Marking is On";
            if (m_bufferSend != null) return "Send is On";
            Add(mark);
            p_eState = eState.Marking;
            return "OK";
        }
        #endregion

        #region Thread Send
        bool m_bThread = false;
        Thread m_threadSend;
        void RunThreadSend()
        {
            m_bThread = true;
            while (m_bThread)
            {
                Thread.Sleep(10);
                if (p_eState != eState.Init)
                {
                    while (m_qMark.Count > 0)
                    {
                        RTC5Mark mark = m_qMark.Dequeue();
                        CheckBuffer(mark.p_lBuffer);
                        mark.Send();
                    }
                    BufferEnd();
                }
            }
        }

        Buffer m_bufferSend = null;
        void CheckBuffer(int lBuffer)
        {
            if (m_bufferSend != null)
            {
                if (m_bufferSend.GetFreeSize() > lBuffer) return;
                BufferEnd();
            }
            while (m_bufferSend == null)
            {
                if (m_aBuffer[1].p_eState == Buffer.eState.Init) m_bufferSend = m_aBuffer[1];
                if (m_aBuffer[0].p_eState == Buffer.eState.Init) m_bufferSend = m_aBuffer[0];
                if (m_bufferSend != null)
                {
                    if (RTC5Wrap.n_load_list(p_uHead, m_bufferSend.m_uIndex, 0) < 1)
                    {
                        Reset("Load List Buffer Error");
                        return;
                    }
                    m_bufferSend.SendStart();
                }
                else Thread.Sleep(10);
            }
        }

        void BufferEnd()
        {
            m_log.Info("Send Done : " + m_bufferSend.SendEnd());
            m_qMarkBuffer.Enqueue(m_bufferSend);
            m_bufferSend = null;
        }
        #endregion

        #region Thread Mark
        Thread m_threadMark;
        void RunThreadMark()
        {
            StopWatch swMark = new StopWatch();
            uint uState = 1;
            uint uPos = 0;
            m_bThread = true;
            while (m_bThread)
            {
                Thread.Sleep(10);
                if (p_eState == eState.Marking)
                {
                    if (m_bufferMark != null)
                    {
                        RTC5Wrap.n_get_status(p_uHead, out uState, out uPos);
                        if ((uState & 0x0001) == 0)
                        {
                            m_log.Info("Maring Done : " + m_bufferMark.m_id + ", " + swMark.p_sTime);
                            m_bufferMark.p_eState = Buffer.eState.Init;
                            m_bufferMark = null;
                        }
                    }
                    else if (m_qMarkBuffer.Count > 0)
                    {
                        m_bufferMark = m_qMarkBuffer.Dequeue();
                        m_bufferMark.p_eState = Buffer.eState.Mark;
                        RTC5Wrap.n_execute_list(p_uHead, m_bufferMark.m_uIndex);
                        swMark.Start();
                    }
                    else if (m_bufferSend == null) p_eState = eState.Init;
                }
                if (p_eState == eState.Init)
                {
                    if (p_bLaserOn && m_swLaserOn.IsTimeover()) p_bLaserOn = false;
                }
            }
        }

        Buffer m_bufferMark = null;
        Queue<Buffer> m_qMarkBuffer = new Queue<Buffer>();
        #endregion

        #region LaserOn
        StopWatch m_swLaserOn = new StopWatch();
        bool _bLaserOn = false;
        public bool p_bLaserOn
        {
            get { return _bLaserOn; }
            set
            {
                if (_bLaserOn == value) return;
                _bLaserOn = value;
                if (_bLaserOn)
                {
                    m_laserParameter.SendLaserParam(false);
                    CPoint cp = m_setting.CalcPos(m_rpLaserOn.X, m_rpLaserOn.Y);
                    RTC5Wrap.n_goto_xy(p_uHead, cp.X, cp.Y);
                    RTC5Wrap.n_laser_signal_on(p_uHead);
                    m_swLaserOn.Start();
                }
                else
                {
                    RTC5Wrap.n_laser_signal_off(p_uHead);
                    RTC5Wrap.n_goto_xy(p_uHead, 0, 0);
                }
                OnPropertyChanged();
                OnPropertyChanged("p_sLaserOnContext");
            }
        }
        public string p_sLaserOnContext
        {
            get { return p_bLaserOn ? "Laser Off" : "Laser On"; }
        }

        RPoint m_rpLaserOn = new RPoint();
        double m_secLaserOn = 3;
        void RunTreeLaserOn(Tree tree)
        {
            m_rpLaserOn = tree.Set(m_rpLaserOn, m_rpLaserOn, "Position", "Laser On Position (mm)");
            m_secLaserOn = tree.Set(m_secLaserOn, m_secLaserOn, "Time", "Laser On Time (sec)");
            m_swLaserOn.p_secTimeout = m_secLaserOn;
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
            RunTreeMarkTest(m_treeRoot.GetTree("Mark"));
            RunTreeLaserOn(m_treeRoot.GetTree("LaserOn"));
            m_setting.RunTree(m_treeRoot.GetTree("Setting", false));
            m_laserParameter.RunTree(m_treeRoot.GetTree("Default Laser Parameter", false), true);
        }
        #endregion

        public string p_id { get; set; }
        public LogWriter m_log;
        public TreeRoot m_treeRoot;
        public LaserParameter m_laserParameter;

        public RTC5(string sLaser, LogWriter log)
        {
            p_id = sLaser + ".RTC5";
            m_log = log;

            RTC5Wrap.init_rtc5_dll(); //Copy RootTools_RTC5/DLL/RTC5DLLx64.dll -> c:/RTC5/RTC5DLLx64.dll
            m_setting = new RTC5Setting(m_log);
            m_laserParameter = new LaserParameter(this);
            m_design = new RTC5Design(p_id + ".Design", this);

            m_treeRoot = new TreeRoot(p_id, log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);

            InitBuffer();
            string sInit = InitRTC5();
            if (sInit != "OK") m_log.Error("Init RTC5 : " + sInit);

            m_threadSend = new Thread(new ThreadStart(RunThreadSend));
            m_threadSend.Start();
            m_threadMark = new Thread(new ThreadStart(RunThreadMark));
            m_threadMark.Start();
        }

        string InitRTC5()
        {
            uint nError = RTC5Wrap.n_get_last_error(p_uHead);
            if (nError > 0)
            {
                m_log.Info("Last Error Code = " + nError.ToString());
                RTC5Wrap.n_reset_error(p_uHead, nError);
            }
            RTC5Wrap.n_stop_execution(p_uHead);
            nError = RTC5Wrap.n_load_program_file(p_uHead, "c:\\RTC5");
            if (nError > 0) return "Program file loading error = " + nError.ToString();
            nError = RTC5Wrap.n_load_correction_file(p_uHead, "c:\\RTC5\\Cor_1to1.ct5", 1, 2);
            if (nError > 0) return "Correction file loading error = " + nError.ToString();
            RTC5Wrap.n_select_cor_table(p_uHead, 1, 0);
            RTC5Wrap.n_reset_error(p_uHead, 0xffffffff);
            RTC5Wrap.n_config_list(p_uHead, Buffer.c_lBuffer, Buffer.c_lBuffer);
            RTC5Wrap.n_set_start_list(p_uHead, 1);
            RTC5Wrap.n_jump_abs(p_uHead, 0, 0);
            RTC5Wrap.n_set_end_of_list(p_uHead);
            RTC5Wrap.n_execute_list(p_uHead, 1);
            return "OK";
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_qMark.Clear();
                m_threadSend.Join();
                m_threadMark.Join();
            }
        }

    }
}
