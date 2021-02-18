using RootTools;
using RootTools.Control;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Module
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
            Buzzer1,
            Buzzer2,
            Buzzer3,
            Buzzer4
        }

        string[] m_asLamp = Enum.GetNames(typeof(eLamp));
        string[] m_asBuzzer = Enum.GetNames(typeof(eBuzzer));
        DIO_Os m_doLamp;
        DIO_Os m_doBuzzer;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doLamp, this, "Lamp", m_asLamp);
            p_sInfo = m_toolBox.Get(ref m_doBuzzer, this, "Buzzer", m_asBuzzer);
        }
        #endregion

        #region Thread
        public EQ.eState m_eState = EQ.eState.Init;
        protected override void RunThread()
        {
            base.RunThread();
            if(m_eState != EQ.p_eState)
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

        public string BuzzerOff()
        {
            m_doBuzzer.Write(eBuzzer.Buzzer1, false);
            m_doBuzzer.Write(eBuzzer.Buzzer2, false);
            m_doBuzzer.Write(eBuzzer.Buzzer3, false);
            m_doBuzzer.Write(eBuzzer.Buzzer4, false);
            return "OK";
        }

        public TowerLamp(string id, IEngineer engineer)
        {
            p_id = id;
            base.InitBase(id, engineer);
        }
    }
}
