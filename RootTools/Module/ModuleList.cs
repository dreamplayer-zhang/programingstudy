﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace RootTools.Module
{
    /// <summary> ModuleList : Module List 관리, Module들의 ModuleRun을 순차적으로 실행 가능 (when EQ.p_eState.Ready) </summary>
    public class ModuleList : NotifyProperty
    {
        #region Property
        bool _bEnableRun = false;
        public bool p_bEnableRun
        {
            get { return _bEnableRun; }
            set
            {
                if (_bEnableRun == value) return;
                _bEnableRun = value;
                OnPropertyChanged();
            }
        }

        string _sRun = "Initialize";
        public string p_sRun
        {
            get { return _sRun; }
            set
            {
                if (_sRun == value) return;
                _sRun = value;
                OnPropertyChanged();
            }
        }

        string _sRunStep = "RunStep";
        public string p_sRunStep
        {
            get { return _sRunStep; }
            set
            {
                if (_sRunStep == value) return;
                _sRunStep = value;
                OnPropertyChanged();
            }
        }

        public Listp_sInfo m_infoList = new Listp_sInfo();
        string _sInfo = "Info";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_log.Info(m_id + " Info : " + value.ToString());
                m_infoList.Add(_sInfo);
            }
        }
        #endregion

        #region List ModuleBase
        /// <summary> m_asModule : Module들의 m_id List </summary>
        public List<string> m_asModule = new List<string>();
        /// <summary> m_aModule : Modulebase와 해당하는 UI 모음 (UI를 TabControl에 표시하기 위함) </summary>
        public Dictionary<ModuleBase, UserControl> m_aModule = new Dictionary<ModuleBase, UserControl>();
        public void AddModule(ModuleBase module, UserControl uc)
        {
            m_aModule.Add(module, uc);
            m_asModule.Add(module.p_id);
        }

        public ModuleBase GetModule(string id)
        {
            foreach (ModuleBase module in m_aModule.Keys)
            {
                if (module.p_id == id) return module;
            }
            return null;
        }
        #endregion

        #region Thread
        bool m_bThread = false;
        Thread m_thread;
        void StartThread()
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
                Thread.Sleep(10);
                p_bEnableRun = (EQ.p_eState == EQ.eState.Ready);
                switch (EQ.p_eState)
                {
                    case EQ.eState.Init: p_sRun = "Home"; break;
                    case EQ.eState.Home: p_sRun = "Stop"; break;
                    case EQ.eState.Ready: p_sRun = "Run"; break;
                    case EQ.eState.Run:
                    case EQ.eState.Recovery:
                        p_sRun = "Stop";
                        if (m_qModuleRun.Count > 0)
                        {
                            ModuleRunBase moduleRun = m_qModuleRun.Peek();
                            p_iRun = m_qModuleRun.Count;
                            moduleRun.StartRun();
                            Thread.Sleep(100);
                            while (moduleRun.m_moduleBase.m_qModuleRun.Count > 0) Thread.Sleep(10);
                            while (moduleRun.m_moduleBase.p_eState == ModuleBase.eState.Run) Thread.Sleep(10);
                            if (m_qModuleRun.Count <= 1)
                            {
                                p_visibleRnR = Visibility.Visible;
                                EQ.p_eState = EQ.eState.Ready;
                            }
                            if (m_qModuleRun.Count > 0) m_qModuleRun.Dequeue();
                        }
                        break;
                    case EQ.eState.Error: p_sRun = "Reset"; break;
                }
            }
        }
        #endregion

        #region StateRun
        /// <summary> m_qModuleRun : if (EQ.p_eState == Ready) 일 때 RunThread()에서 순차적으로 실행 </summary>
        public Queue<ModuleRunBase> m_qModuleRun = new Queue<ModuleRunBase>();

        public string ClickRun()
        {
            switch (EQ.p_eState)
            {
                case EQ.eState.Init:
                    EQ.p_eState = EQ.eState.Home;
                    break;
                case EQ.eState.Home:
                    EQ.p_bStop = true;
                    EQ.p_eState = EQ.eState.Init;
                    break;
                case EQ.eState.Ready:
                    if (m_qModuleRun.Count == 0) 
                        StartModuleRuns();
                    else
                    {
                        m_qModuleRun.Clear();
                        p_iRun = m_qModuleRun.Count;
                        EQ.p_bStop = true;
                    }
                    break;
                case EQ.eState.Run:
                case EQ.eState.Recovery:
                    m_qModuleRun.Clear();
                    p_iRun = m_qModuleRun.Count;
                    EQ.p_bStop = true;
                    break;
                case EQ.eState.Error: m_handler.Reset(); break;
            }
            return "OK";
        }

        public void StartModuleRuns()
        {
            EQ.p_bStop = false;
            foreach (ModuleRunBase moduleRun in m_moduleRunList.p_aModuleRun) m_qModuleRun.Enqueue(moduleRun);
            p_maxRun = m_qModuleRun.Count;
            EQ.p_eState = EQ.eState.Run;
        }

        public string ClickRunStep()
        {
            if (EQ.p_eState != EQ.eState.Ready) return "EQ p_eSrate Not Ready";
            EQ.p_bStop = false;
            foreach (ModuleRunBase moduleRun in m_moduleRunList.p_aModuleRun)
            {
                if (moduleRun.p_id == p_sRunStep)
                {
                    m_qModuleRun.Enqueue(moduleRun);
                    return "OK";  //forget
                }
            }
            p_maxRun = m_qModuleRun.Count;
            EQ.p_eState = EQ.eState.Run;
            return "OK";
        }
        #endregion

        #region RnR
        Visibility _visibleRnR = Visibility.Hidden;
        public Visibility p_visibleRnR
        {
            get { return _visibleRnR; }
            set
            {
                if (_visibleRnR == value) return;
                _visibleRnR = value;
                OnPropertyChanged();
            }
        }

        int _nRnR = 1;
        public int p_nRnR
        {
            get { return _nRnR; }
            set
            {
                if (_nRnR == value) return;
                _nRnR = value;
                OnPropertyChanged();
            }
        }

        int _maxRun = 1;
        public int p_maxRun
        {
            get { return _maxRun; }
            set
            {
                if (_maxRun == value) return;
                _maxRun = Math.Max(value, 1);
                OnPropertyChanged();
            }
        }

        public int p_iRun
        {
            get { return (m_qModuleRun.Count == 0) ? 0 : p_maxRun - m_qModuleRun.Count; }
            set { OnPropertyChanged(); }
        }

        int _percent;
        public int p_Percent
        {
            get
            {
                return (int)(p_iRun / p_maxRun * 100);
            }
            set
            {
                _percent = value;
                OnPropertyChanged();
            }
        }

        public string ClickRunRnR()
        {
            if (EQ.p_eState != EQ.eState.Ready) return "EQ not Ready"; 
            if (m_qModuleRun.Count > 0)
            {
                m_qModuleRun.Clear();
                EQ.p_bStop = true;
                return "ModuleRun Queue Clear"; 
            }
            EQ.p_bStop = false;
            for (int n = 0; n < p_nRnR; n++)
            {
                foreach (ModuleRunBase moduleRun in m_moduleRunList.p_aModuleRun) m_qModuleRun.Enqueue(moduleRun);
            }
            p_maxRun = m_qModuleRun.Count;
            EQ.p_eState = EQ.eState.Run;
            return "OK"; 
        }
        #endregion

        string m_id;
        IEngineer m_engineer;
        IHandler m_handler;
        Log m_log;
        /// <summary> m_moduleRunList : ModuleRun 편집용 -> m_qModuleRun 으로 실행 </summary>
        public ModuleRunList m_moduleRunList;
        public ModuleList(IEngineer engineer)
        {
            m_id = EQ.m_sModel;
            m_engineer = engineer;
            m_handler = engineer.ClassHandler();
            m_log = LogView.GetLog(m_id);
            m_moduleRunList = new ModuleRunList(m_id, engineer);

            StartThread();
        }

        public void ThreadStop()
        {
            if (m_bThread)
            {
                m_bThread = false;
                m_thread.Join();
            }
        }
    }
}
