using RootTools;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace Root_TactTime
{
    public class Loader
    {
        #region TimeEvent
        public class Event
        {
            public double p_secNow { get; set; }
            public double p_secAdd { get; set; }
            public string p_sEvent { get; set; }

            public Event(double secNow, double secAdd, string sEvent)
            {
                p_secNow = Math.Round(100 * secNow) / 100;
                p_secAdd = Math.Round(100 * secAdd) / 100;
                p_sEvent = sEvent;
            }
        }
        public ObservableCollection<Event> m_aEvent = new ObservableCollection<Event>();
        public void AddEvent(ref double secNow, double secAdd, string sEvent)
        {
            m_aEvent.Add(new Event(secNow, secAdd, sEvent));
            secNow += secAdd; 
            m_tact.p_secRun = secNow;
        }

        public void Clear()
        {
            m_aEvent.Clear();
            m_secReady = 0;
            foreach (Picker picker in m_aPicker) picker.p_sStrip = ""; 
        }
        #endregion

        #region UI
        Loader_UI _ui = null;
        public Loader_UI p_ui
        {
            get
            {
                if (_ui == null)
                {
                    _ui = new Loader_UI();
                    _ui.Init(this); 
                }
                return _ui; 
            }
            set { }
        }
        #endregion

        #region Loader
        public double m_v = 1;
        public double m_secAcc = 0.3;
        public double m_secDec = 0.3;
        public void RunTree(Tree tree)
        {
            m_v = tree.Set(m_v, m_v, "Speed", "Axis Move Speed (m / sec)");
            m_secAcc = tree.Set(m_secAcc, m_secAcc, "Acc", "Axis Acc Time (sec)");
            m_secDec = tree.Set(m_secDec, m_secDec, "Dec", "Axis Dec Time (sec)");
            foreach (Picker picker in m_aPicker) picker.RunTree(tree.GetTree(picker.p_id));
        }
        #endregion

        #region Move
        public double m_secReady = 0; 
        public RPoint m_rp = new RPoint();
        public void Move(ref double secNow, RPoint rpDst)
        {
            RPoint dp = rpDst - m_rp;
            double dL = Math.Max(dp.X, dp.Y);
            double fTime = m_secAcc + m_secDec;
            dL -= fTime * m_v;
            if (dL > 0) fTime += dL / m_v;
            AddEvent(ref secNow, fTime, "Loader Axis Move");
            m_rp = rpDst;
            m_secReady = secNow;
        }
        #endregion

        List<Picker> m_aPicker = new List<Picker>(); 
        public void Add(Picker picker)
        {
            m_tact.m_aPicker.Add(picker);
            m_aPicker.Add(picker); 
        }

        public TactTime m_tact;
        public string p_id { get; set; }
        public Loader(TactTime tact, string id)
        {
            m_tact = tact;
            p_id = id; 
        }
    }
}
