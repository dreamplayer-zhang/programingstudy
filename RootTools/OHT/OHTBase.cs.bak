using RootTools.Control;
using RootTools.Gem;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace RootTools.OHT
{
    public class OHTBase : NotifyProperty
    {
        #region Property
        protected string _sInfo = "OK"; 
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                //if (value != "OK")  //KHD 201222 del /test
                //{
                //    m_log.Error(value);
                //    m_module.p_eState = ModuleBase.eState.Error; 
                //}
                OnPropertyChanged(); 
            }
        }

        public string p_id { get; set; }

        string _sState = "Init"; 
        public string p_sState
        {
            get { return _sState; }
            set
            {
                if (_sState == value) return;
                _sState = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region OHT DIO
        public enum eDIO
        {
            DI,
            DO,
        }
        public interface IDIO
        {
            string p_id { get; set; }
            eDIO p_eDIO { get; }
            bool p_bOn { get; set; }
            bool p_bWait { get; set; }
            void RunTree(Tree tree);
            void RunTreeToolBox(Tree tree, OHTBase oht);
        }

        public class DI : IDIO
        {
            public DIO_I m_di = null;

            public string p_id { get; set; }
            public eDIO p_eDIO { get { return eDIO.DI; } }

            StopWatch m_sw = new StopWatch(); 
            public int m_msChange = 100;
            bool _bOn = false; 
            public bool p_bOn
            {
                get
                {
                    if (m_di == null) return false; 
                    if (_bOn == m_di.p_bIn) m_sw.Start();
                    else if (m_sw.ElapsedMilliseconds >= m_msChange) _bOn = m_di.p_bIn;
                    return _bOn;
                }
                set { }
            }

            public bool p_bWait { get; set; }

            public void RunTree(Tree tree)
            {
                m_msChange = tree.Set(m_msChange, m_msChange, p_id, "DI Change Delay for Remove Noise (ms)"); 
            }

            public void RunTreeToolBox(Tree tree, OHTBase oht)
            {
                if (m_di == null) m_di = new DIO_I(oht.m_toolDIO, oht.p_id + "." + p_id, oht.m_log, false);
                if (m_di.RunTree(tree) != "OK") return;
                oht.m_module.m_listDI.AddBit(m_di.m_bitDI);
            }

            public DI(string id)
            {
                p_id = id;
                p_bWait = false;
            }
        }

        public class DO : IDIO
        {
            public DIO_O m_do = null;

            public string p_id { get; set; }
            public eDIO p_eDIO { get { return eDIO.DO; } }

            public bool p_bOn
            {
                get { return (m_do == null) ? false : m_do.p_bOut; }
                set 
                {
                    p_bWait = value; 
                    m_do.Write(value); 
                }
            }

            public bool p_bWait { get; set; }

            public void Toggle()
            {
                p_bOn = !p_bOn; 
            }

            public void RunTree(Tree tree) { }

            public void RunTreeToolBox(Tree tree, OHTBase oht)
            {
                if (m_do == null) m_do = new DIO_O(oht.m_toolDIO, oht.p_id + "." + p_id, oht.m_log, false);
                if (m_do.RunTree(tree) != "OK") return;
                oht.m_module.m_listDO.AddBit(m_do.m_bitDO);
            }

            public DO(string id)
            {
                p_id = id;
                p_bWait = false; 
            }
        }

        public List<IDIO> m_aDIO = new List<IDIO>();
        protected void RunTreeDIO(Tree tree)
        {
            foreach (IDIO dio in m_aDIO) dio.RunTree(tree); 
        }
        #endregion

        #region Time Data
        public int m_lHistory = 30;
        public class History
        {
            DateTime _dateTime = DateTime.Now;
            public string p_dateTime { get { return _dateTime.ToString("HH:mm:ss"); } }
            public class Data
            {
                public string p_id { get; set; }
                public bool p_bOn { get; set; }
                public Brush p_brush { get; set; }
                public eDIO p_eDIO
                {
                    set { p_brush = (value == eDIO.DI) ? Brushes.Blue : Brushes.Red; } 
                }

                public Data(IDIO dio)
                {
                    p_id = dio.p_id;
                    p_eDIO = dio.p_eDIO;
                    p_bOn = dio.p_bOn; 
                }
            }
            public List<Data> m_aData = new List<Data>();
        }

        void RunTreeHistory(Tree tree)
        {
            m_lHistory = tree.Set(m_lHistory, m_lHistory, "Count", "History Display Count"); 
        }
        #endregion

        #region TreeToolBox
        public void RunTreeToolBox(Tree tree)
        {
            foreach (IDIO dio in m_aDIO)
            {
                dio.RunTreeToolBox(tree.GetTree(dio.p_id), this); 
            }
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.RegWrite);
        }

        public virtual void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunTreeDIO(m_treeRoot.GetTree("Digital Input"));
            RunTreeHistory(m_treeRoot.GetTree("History"));
        }

        #endregion

        public ModuleBase m_module; 
        public Log m_log;
        public TreeRoot m_treeRoot;
        public GemCarrierBase m_carrier = null;
        IToolDIO m_toolDIO;
        public void InitBase(ModuleBase module, GemCarrierBase carrier, IToolDIO toolDIO)
        {
            m_module = module;
            m_carrier = carrier; 
            m_log = module.m_log;
            m_toolDIO = toolDIO;
            
            m_treeRoot = new TreeRoot(p_id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead);
        }
    }
}
