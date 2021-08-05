using RootTools.Control;
using RootTools.Module;

namespace Root_JEDI.Module
{
    public class TrayStack : ModuleBase
    {
        #region ToolBox
        public DIO_I4O m_dioAlignY;
        public DIO_I2O m_dioAlignX;
        public DIO_Is[] m_diCheck = new DIO_Is[4] { null, null, null, null };
        public override void GetTools(bool bInit)
        {
            m_toolBox.GetDIO(ref m_dioAlignX, this, "AlignX", "Off", "Align");
            m_toolBox.GetDIO(ref m_dioAlignY, this, "AlignY", "Off", "Align");
            m_toolBox.GetDIO(ref m_diCheck[0], this, "Check 1F", new string[2] { "A", "B" });
            m_toolBox.GetDIO(ref m_diCheck[1], this, "Check 2F", new string[2] { "A", "B" });
            m_toolBox.GetDIO(ref m_diCheck[2], this, "Check 3F", new string[2] { "A", "B" });
            m_toolBox.GetDIO(ref m_diCheck[3], this, "Check 4F", new string[2] { "A", "B" });
        }
        #endregion
    }
}
