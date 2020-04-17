using RootTools.Trees;
using System;
using System.Collections.Generic;

namespace Root_Wind
{
    public class WaferSize
    {
        #region Data
        public class Data
        {
            public InfoWafer.eWaferSize m_eWaferSize = InfoWafer.eWaferSize.e300mm;
            public string m_sWaferSize;
            public bool m_bEnable = false;
            public int m_lWafer = 1;
            public int m_nTeachWTR = -1;

            public Data(InfoWafer.eWaferSize waferSize)
            {
                m_eWaferSize = waferSize;
                m_sWaferSize = waferSize.ToString();
            }

            public void RunTreeEnable(Tree tree, bool bVisible)
            {
                m_bEnable = tree.Set(m_bEnable, m_bEnable, m_sWaferSize, "Enable Wafer Size", bVisible);
            }

            public void RunTreeCount(Tree tree, bool bVisible)
            {
                m_lWafer = tree.Set(m_lWafer, m_lWafer, m_sWaferSize, "Wafer Count", m_bEnable && bVisible);
            }

            public void RunTreeTeach(Tree tree)
            {
                m_nTeachWTR = tree.Set(m_nTeachWTR, m_nTeachWTR, m_sWaferSize, "WTR Teach Index", m_bEnable);
            }
        }
        List<Data> m_aData = new List<Data>();

        void InitDatas()
        {
            int lCount = Enum.GetNames(typeof(InfoWafer.eWaferSize)).Length;
            for (int n = 0; n < lCount; n++)
            {
                Data data = new Data((InfoWafer.eWaferSize)n);
                m_aData.Add(data);
            }
        }

        public Data GetData(InfoWafer.eWaferSize size)
        {
            foreach (Data data in m_aData)
            {
                if (data.m_eWaferSize == size) return data;
            }
            return null;
        }
        #endregion

        string m_id;
        bool m_bUseEnable = false;
        bool m_bUseCount = false;
        public WaferSize(string id, bool bUseEnable, bool bUseCount)
        {
            m_id = id;
            m_bUseEnable = bUseEnable;
            m_bUseCount = bUseCount;
            InitDatas();
        }

        public void RunTree(Tree tree, bool bVisible)
        {
            foreach (Data data in m_aData)
            {
                data.RunTreeEnable(tree.GetTree("Enable"), bVisible && m_bUseEnable);
                data.RunTreeCount(tree.GetTree("Count"), bVisible && m_bUseCount);
                if (m_bUseEnable == false) data.m_bEnable = true;
            }
        }

        public void RunTeachTree(Tree tree)
        {
            foreach (Data data in m_aData)
            {
                data.RunTreeTeach(tree);
            }
        }

        public List<string> GetEnableNames()
        {
            List<string> asEnable = new List<string>();
            foreach (Data data in m_aData)
            {
                if (data.m_bEnable) asEnable.Add(data.m_sWaferSize);
            }
            return asEnable;
        }
    }
}
