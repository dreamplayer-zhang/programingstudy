using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Data
{
    public class SaveMeasureData
    {
        Task m_task;
        bool m_bThread = true;
        public bool m_bStartSave = false;

        public LibSR_Met.DataManager m_metDM;
        //System.Collections.Concurrent.ConcurrentQueue queue = new System.Collections.Concurrent.ConcurrentQueue();
        public SaveMeasureData()
        {
            m_metDM = LibSR_Met.DataManager.GetInstance();
            m_task = new Task(RunThread);
            m_task.Start();
        }

        void RunThread()
        {
            //while (m_bThread)
            //{
            //    if (m_bStartSave)
            //    {
            //        //m_metDM.SaveResultFileSlot(,);
            //    }
            //}
        }

        public void ThreadStop()
        {
            m_bThread = false;
        }
    }

    public class SaveData
    {
        public string p_foupID { get; set; } = "";
        public string p_lotID { get; set; } = "";
        public string p_toolsID { get; set; } = "TEST";
        public string p_waferID { get; set; } = "";
        public string p_slotID { get; set; } = "";
        public string p_swVersion { get; set; } = "1.0.0.0";
        public string p_recipeName { get; set; } = "";

    }
}
