using RootTools;
using RootTools.Control;
using RootTools.Module;
using System;
using System.Threading;

namespace Root_VEGA_D.Module
{
    public class TowerLamp : ModuleBase
    {
        #region ToolBox
        public enum eLamp
        {
            Red,
            Yellow,
            Green
        }

        public enum eBuzzer
        {
            Error,
            Warning,
            Finish,
            Home,
        }

        string[] m_asLamp = Enum.GetNames(typeof(eLamp));
        string[] m_asBuzzer = Enum.GetNames(typeof(eBuzzer));
        DIO_Os m_doLamp;
        DIO_Os m_doBuzzer;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.GetDIO(ref m_doLamp, this, "Lamp", m_asLamp);
            p_sInfo = m_toolBox.GetDIO(ref m_doBuzzer, this, "Buzzer", m_asBuzzer);
            if (bInit)
            {
                EQ.m_EQ.OnChanged += M_EQ_OnChanged;
            }
        }

        private void M_EQ_OnChanged(_EQ.eEQ eEQ, dynamic value)
        {
            switch (eEQ)
            {
                case _EQ.eEQ.State:
                    switch ((EQ.eState)value)
                    {
                        case EQ.eState.Error:
                            RunBuzzer(eBuzzer.Error);
                            break;
                        case EQ.eState.Home:
                            RunBuzzer(eBuzzer.Home);
                            Thread.Sleep(200);
                            BuzzerOff();
                            break;
                        case EQ.eState.Ready:
                            BuzzerOff();
                            break;
                    }
                    break;
            }
        }
        #endregion

        #region Thread
        public EQ.eState m_eState = EQ.eState.Init;
        protected override void RunThread()
        {
            base.RunThread();
            {
                switch (EQ.p_eState)
                {
                    case EQ.eState.Error:
                        m_doLamp.Write(eLamp.Red);
                        break;
                    case EQ.eState.Run:
                        m_doLamp.Write(eLamp.Green);
                        break;
                    case EQ.eState.Home:
                        break;
                    case EQ.eState.Ready:
                        m_doLamp.Write(eLamp.Yellow);
                        break;
                    case EQ.eState.Init:
                        m_doLamp.Write(eLamp.Yellow);
                        BuzzerOff();
                        break;
                }
                m_eState = EQ.p_eState;
            }
        }
        #endregion

        public void RunBuzzer(eBuzzer ebuzzer)
        {
            m_doBuzzer.Write(ebuzzer);
        }

        public string BuzzerOff()
        {
            m_doBuzzer.AllOff();
            return "OK";
        }

        public TowerLamp(string id, IEngineer engineer)
        {
            p_id = id;
            base.InitBase(id, engineer);
        }
    }
}
