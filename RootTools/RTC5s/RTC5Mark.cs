using RootTools.Trees;

namespace RootTools.RTC5s
{
    public class RTC5Mark
    {
        #region Property
        bool p_b3D
        {
            get { return m_setting.m_b3D; }
        }

        double m_fRotate = 0;
        RPoint m_rpShift = new RPoint();
        double m_zShift = 0;
        void RunTreeShift(Tree tree)
        {
            m_fRotate = tree.Set(m_fRotate, m_fRotate, "Rotate", "Rotate Design (deg)", m_bShowShift);
            m_rpShift = tree.Set(m_rpShift, m_rpShift, "Shift", "Shift XY (mm)", m_bShowShift);
            m_zShift = tree.Set(m_zShift, m_zShift, "Shift Z", "Shift Z (mm)", p_b3D && m_bShowShift);
        }
        #endregion

        #region DesignBase
        string _sDesign = "";
        string p_sDesign
        {
            get { return _sDesign; }
            set
            {
                if (_sDesign == value) return;
                _sDesign = value;
                m_design = m_rtc5Design.Get(_sDesign);
                if (m_design != null) m_designMark = m_design.MakeData(_sCode);
            }
        }

        string _sCode = "";
        public string p_sCode
        {
            get { return _sCode; }
            set
            {
                if (_sCode == value) return;
                if (m_design == null) return;
                _sCode = value;
                if (m_design != null) m_designMark = m_design.MakeData(_sCode);
            }
        }

        DesignBase m_design = null;
        DesignBase m_designMark = null;
        void RunTreeDesign(Tree tree)
        {
            p_sDesign = tree.Set(p_sDesign, p_sDesign, m_rtc5Design.p_asDesign, "Design", "SelectDesign");
            bool bCodeChange = (m_design != null) && m_design.m_bEnableCode;
            p_sCode = tree.Set(p_sCode, p_sCode, "Code", "Design Code", bCodeChange);
        }
        #endregion

        #region Send
        public int p_lBuffer
        {
            get
            {
                return (m_designMark != null) ? m_designMark.p_lBuffer : 0;
            }
        }

        public string Send()
        {
            if (m_designMark == null) return "Mark Design is null";
            return m_designMark.Send(m_fRotate, m_rpShift, m_zShift);
        }
        #endregion 

        #region Tree
        public void RunTree(Tree tree)
        {
            RunTreeDesign(tree.GetTree("Design"));
            RunTreeShift(tree.GetTree("Shift", false));
        }
        #endregion

        RTC5 m_rtc5;
        RTC5Setting m_setting;
        RTC5Design m_rtc5Design;
        bool m_bShowShift = false;
        public RTC5Mark(RTC5 rtc5, bool bShowShift = false)
        {
            m_rtc5 = rtc5;
            m_setting = rtc5.m_setting;
            m_rtc5Design = rtc5.m_design;
            m_bShowShift = bShowShift;
        }
    }
}
