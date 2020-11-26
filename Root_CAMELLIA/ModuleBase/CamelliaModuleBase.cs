using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_CAMELLIA
{
    public class CamelliaModuleBase : NotifyProperty
    {
        #region Stage
        public enum EquipState
        {
            Init,
            Home,
            Ready,
            Run,
            Error
        };
        #endregion
        protected EquipState equipState = EquipState.Error;
        public EquipState State
        {
            get 
            {
                return equipState; 
            }
            set
            {
                equipState = value;
                OnPropertyChanged();
            }
        }

        bool m_bThread = false;
        Thread m_thread;
        void ThreadRun()
        {
            m_bThread = true;
            Thread.Sleep(2000);
            while (m_bThread)
            {
                Thread.Sleep(10);
                RunThread();
            }
        }

        protected virtual void RunThread()
        {
            //switch (State)
            //{
            //    case State.Init:
            //        p_bEnableHome = true;
            //        p_sRun = "Initialize";
            //        break;
            //    case eState.Home:
            //        p_bEnableHome = false;
            //        p_sRun = "Stop";
            //        string sStateHome = StateHome();
            //        if (sStateHome == "OK") p_eState = eState.Ready;
            //        else StopHome();
            //        break;
            //    case eState.Ready:
            //        p_bEnableHome = true;
            //        p_sRun = p_sModuleRun;
            //        string sStateReady = StateReady();
            //        if (sStateReady != "OK")
            //        {
            //            p_eState = eState.Error;
            //            m_qModuleRun.Clear();
            //        }
            //        if (m_qModuleRun.Count > 0) p_eState = eState.Run;
            //        break;
            //    case eState.Run:
            //        p_bEnableHome = false;
            //        p_sRun = "Stop";
            //        string sStateRun = StateRun();
            //        if (sStateRun != "OK")
            //        {
            //            p_eState = eState.Error;
            //            m_qModuleRun.Clear();
            //        }
            //        if (m_qModuleRun.Count == 0) p_eState = eState.Ready;
            //        break;
            //    case eState.Error:
            //        p_bEnableHome = false;
            //        p_sRun = "Reset";
            //        break;
            //}
        }
    }
}
