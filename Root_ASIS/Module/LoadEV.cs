using RootTools.Control;
using RootTools.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_ASIS.Module
{
    public class LoadEV : ModuleBase
    {
        #region ToolBox
        DIO_I2O2 m_dioEV;
        DIO_I m_diTop;
        DIO_I m_diCheck;
        DIO_I m_diIonizer;
        DIO_I m_diPaper;
        DIO_I m_diPaperCheck;
        DIO_I m_diPaperFull; 
        DIO_O m_doIonBlow;
        DIO_O m_doAlignBlow;

        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_dioEV, this, "Elevator", "Up", "Down");
            p_sInfo = m_toolBox.Get(ref m_diTop, this, "Top");
            p_sInfo = m_toolBox.Get(ref m_diCheck, this, "Check");
            p_sInfo = m_toolBox.Get(ref m_diIonizer, this, "Ionizer");
            p_sInfo = m_toolBox.Get(ref m_diPaper, this, "Paper");
            p_sInfo = m_toolBox.Get(ref m_diPaperCheck, this, "PaperCheck");
            p_sInfo = m_toolBox.Get(ref m_diPaperFull, this, "PaperFull");
            p_sInfo = m_toolBox.Get(ref m_doIonBlow, this, "IonBlow");
            p_sInfo = m_toolBox.Get(ref m_doAlignBlow, this, "AlignBlow");
            if (bInit) InitTools();
        }

        void InitTools()
        {
        }
        #endregion

    }
}
