using RootTools;
using RootTools.Control;
using RootTools.Module;
using System;

namespace Root_ASIS.Module
{
    public class ASIS : ModuleBase
    {
        #region ToolBox
        DIO_Os m_doBuzzer; 

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_doBuzzer, this, "Buzzer", Enum.GetNames(typeof(eBuzzer))); 
            if (bInit) InitTools();
        }

        void InitTools()
        {
        }
        #endregion

        #region Buzzer
        public enum eBuzzer
        {
            Error,
            Call,
            Warning,
            Home
        }

        public void RunBuzzer(eBuzzer buzzer, bool bOn)
        {
            m_doBuzzer.Write(buzzer, bOn); 
        }
        #endregion

        public ASIS(string id, IEngineer engineer)
        {
            base.InitBase(id, engineer);
        }

        public override void ThreadStop()
        {
            base.ThreadStop();
        }

    }
}
