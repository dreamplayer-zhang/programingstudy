using Root_AxisMapping.Module;
using RootTools;

namespace Root_AxisMapping.MainUI
{
    public class Result : NotifyProperty
    {
        #region Mapping Property
        int p_xArray {  get { return m_mapping.p_xArray; } }
        int p_yArray { get { return m_mapping.p_yArray; } }
        Array[,] p_aArray {  get { return m_mapping.m_aArray; } }
        #endregion

        string m_id; 
        Mapping m_mapping;
        AxisMapping_Engineer m_engineer;
        public AxisMapping m_axisMapping;
        Log m_log;
        public Result(string id, Mapping mapping, AxisMapping_Engineer engineer)
        {
            m_id = id;
            m_engineer = engineer;
            m_axisMapping = ((AxisMapping_Handler)engineer.ClassHandler()).m_axisMapping;
            m_log = LogView.GetLog(id);

        }
    }
}
