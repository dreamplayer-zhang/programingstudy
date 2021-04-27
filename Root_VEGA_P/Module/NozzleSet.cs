using RootTools;
using RootTools.Control;
using RootTools.Module;
using RootTools.ToolBoxs;
using RootTools.Trees;
using System.Collections.Generic;

namespace Root_VEGA_P.Module
{
    public class NozzleSet : NotifyProperty
    {
        #region ToolBox
        DIO_Os m_doNozzle; 
        public string GetTools(ToolBox toolBox)
        {
            toolBox.GetDIO(ref m_doNozzle, m_module, "Nozzle", m_asNozzle); 
            return "OK";
        }
        #endregion

        #region Nozzle Open
        public List<bool> m_aOpen = new List<bool>();

        public List<bool> GetCloneOpen()
        {
            List<bool> aOpen = new List<bool>();
            foreach (bool bOpen in m_aOpen) aOpen.Add(bOpen);
            return aOpen; 
        }

        public void RunTreeOpen(Tree tree, List<bool> aOpen)
        {
            for (int n = 0; n < p_nNozzle; n++)
            {
                aOpen[n] = tree.Set(aOpen[n], aOpen[n], (n + 1).ToString("00"), "Nozzle Open");
            }
        }
        #endregion

        #region Property
        string[] m_asNozzle = new string[1] { "1" };
        int _nNozzle = 1; 
        public int p_nNozzle
        {
            get { return _nNozzle; }
            set
            {
                if (_nNozzle == value) return;
                _nNozzle = value;
                m_asNozzle = new string[value];
                for (int n = 0; n < value; n++) m_asNozzle[n] = (n + 1).ToString("00");
                m_aOpen.Clear();
                for (int n = 0; n < value; n++) m_aOpen.Add(false);
                m_reg.Write("nNozzle", value);
            }
        }

        Registry m_reg;
        void InitNozzle(string id)
        {
            m_reg = new Registry(id);
            p_nNozzle = m_reg.Read("nNozzle", 1);
        }
        #endregion

        #region Nozzle Run
        public string RunNozzle(List<bool> aOpen)
        {
            for (int n = 0; n < p_nNozzle; n++) m_doNozzle.Write(n, aOpen[n]); 
            return "OK";
        }

        public string RunNozzle(int nNozzle)
        {
            for (int n = 0; n < p_nNozzle; n++) m_doNozzle.Write(n, n == nNozzle); 
            return "OK";
        }

        public string RunCloseAllNozzle()
        {
            for (int n = 0; n < p_nNozzle; n++) m_doNozzle.Write(n, false);
            return "OK";
        }
        #endregion

        #region Tree
        public void RunTreeSetup(Tree tree)
        {
            p_nNozzle = tree.Set(p_nNozzle, p_nNozzle, "Count", "Nozzle Count");
        }
        #endregion

        ModuleBase m_module;
        public NozzleSet(ModuleBase module)
        {
            m_module = module;
            InitNozzle(module.p_id);
        }
    }
}
