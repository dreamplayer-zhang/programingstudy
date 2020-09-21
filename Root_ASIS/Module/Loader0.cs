using RootTools;
using RootTools.Control;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Root_ASIS.Module
{
    public class Loader0 : ModuleBase
    {
        #region ToolBox
        Axis m_axis;
        DIO_I m_diPaperFull; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_axis, this, "Axis");
            p_sInfo = m_toolBox.Get(ref m_diPaperFull, this, "Paper Full"); 
            m_aPicker[ePicker.Strip].GetTools(this, bInit);
            m_aPicker[ePicker.Paper].GetTools(this, bInit);
            if (bInit) InitTools();
        }

        void InitTools()
        {
        }
        #endregion

        #region Picker
        enum ePicker
        {
            Strip,
            Paper
        }
        Dictionary<ePicker, Picker> m_aPicker = new Dictionary<ePicker, Picker>();

        void InitPicker()
        {
            m_aPicker.Add(ePicker.Strip, new Picker(p_id + ".StripPicker", this));
            m_aPicker.Add(ePicker.Paper, new Picker(p_id + ".PaperPicker", this));
        }
        #endregion

        #region Check Thread
        bool m_bThreadCheck = false;
        Thread m_threadCheck;
        void InitThreadCheck()
        {
            m_threadCheck = new Thread(new ThreadStart(RunThreadCheck));
            m_threadCheck.Start();
        }

        void RunThreadCheck()
        {
            m_bThreadCheck = true;
            Thread.Sleep(2000);
            while (m_bThreadCheck)
            {
                Thread.Sleep(10);
                switch (m_axis.p_eState)
                {
                    case Axis.eState.Home:
                    case Axis.eState.Jog:
                    case Axis.eState.Move:
                        if (m_aPicker[ePicker.Paper].IsDown() || m_aPicker[ePicker.Strip].IsDown())
                        {
                            m_axis.StopAxis(false);
                            m_axis.ServoOn(false);
                            m_axis.p_eState = Axis.eState.Init;
                            EQ.p_bStop = true;
                            p_sInfo = "Picker Down when Axis Move"; 
                        }
                        break;
                }
            }
        }

        #endregion

        public Loader0(string id, IEngineer engineer, LoadEV loadEV)
        {
            InitPicker();
            base.InitBase(id, engineer);
            InitThreadCheck();
        }

        public override void ThreadStop()
        {
            if (m_bThreadCheck)
            {
                m_bThreadCheck = false;
                m_threadCheck.Join();
            }
        }

    }
}
