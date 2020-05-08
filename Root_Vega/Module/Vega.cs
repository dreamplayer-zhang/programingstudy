using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.Trees;
using System;

namespace Root_Vega.Module
{
    public class Vega : ModuleBase
    {
        #region ToolBox
        enum eLamp
        { 
            Red,
            Yellow,
            Green
        }
        enum eBuzzer
        { 
            Buzzer1,
            Buzzer2,
            Buzzer3,
            Buzzer4,
            BuzzerOff
        }
        eBuzzer m_eBuzzer = eBuzzer.BuzzerOff; 
        string[] m_asLamp = Enum.GetNames(typeof(eLamp)); 
        string[] m_asBuzzer = Enum.GetNames(typeof(eBuzzer));
        DIO_Os m_doLamp;
        DIO_Os m_doBuzzer;
        DIO_I m_diEMS;
        DIO_I m_diProtectionBar;
        DIO_I m_diMCReset;
        DIO_I m_diInterlockKey;
        DIO_I m_diCDALow; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doLamp, this, "Lamp", m_asLamp);
            p_sInfo = m_toolBox.Get(ref m_doBuzzer, this, "Buzzer", m_asBuzzer);
            p_sInfo = m_toolBox.Get(ref m_diEMS, this, "EMS");
            p_sInfo = m_toolBox.Get(ref m_diProtectionBar, this, "ProtectionBar");
            p_sInfo = m_toolBox.Get(ref m_diMCReset, this, "MC Reset");
            p_sInfo = m_toolBox.Get(ref m_diInterlockKey, this, "Interlock Key");
            p_sInfo = m_toolBox.Get(ref m_diCDALow, this, "CDA Low");
        }
        #endregion

        #region Thread
        protected override void RunThread()
        {
            base.RunThread();
            m_doLamp.Write(eLamp.Red, EQ.p_eState == EQ.eState.Error);
            m_doLamp.Write(eLamp.Yellow, EQ.p_eState == EQ.eState.Run);
            m_doLamp.Write(eLamp.Green, EQ.p_eState == EQ.eState.Ready);
            m_eBuzzer = eBuzzer.BuzzerOff; 
            m_doBuzzer.Write(m_eBuzzer);
            if (m_diEMS.p_bIn) EQ.p_eState = EQ.eState.Error; 
            //
        }
        #endregion

        #region Tree
        public override void RunTree(Tree tree)
        {
            base.RunTree(tree);
            //
        }
        #endregion

        public Vega(string id, IEngineer engineer, string sLogGroup = "")
        {
            p_id = id;
            base.InitBase(id, engineer, sLogGroup);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }
    }
}
