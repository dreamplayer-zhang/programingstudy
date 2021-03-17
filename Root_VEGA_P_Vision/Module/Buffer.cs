using RootTools;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision.Module
{
    public class Buffer : ModuleBase, IRTRChild
    {
        #region InfoPod
        InfoPod _infoPod = null;
        public InfoPod p_infoPod
        {
            get { return _infoPod; }
            set
            {
                int nPod = (value != null) ? (int)value.p_ePod : -1;
                _infoPod = value;
                m_reg.Write("InfoPod", nPod);
                value.WriteReg();
                OnPropertyChanged();
            }
        }

        Registry m_reg = null;
        public void ReadPod_Registry()
        {
            int nPod = m_reg.Read("InfoPod", -1);
            p_infoPod = new InfoPod((InfoPod.ePod)nPod);
            p_infoPod.ReadReg();
        }
        #endregion

        #region IRTRChild
        public string IsGetOK()
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            return (p_infoPod != null) ? "OK" : p_id + " IsGetOK - Pod not Exist";
        }

        public string IsPutOK(InfoPod infoPod)
        {
            if (p_eState != eState.Ready) return p_id + " eState not Ready";
            switch (infoPod.p_ePod)
            {
                case InfoPod.ePod.EOP_Dome:
                case InfoPod.ePod.EOP_Door:
                    return p_id + " Invalid Pod Type";
            }
            return (p_infoPod == null) ? "OK" : p_id + " IsPutOK - Pod Exist";
        }

        public string BeforeGet()
        {
            return "OK";
        }

        public string BeforePut(InfoPod infoPod)
        {
            return "OK";
        }

        public string AfterGet()
        {
            return "OK";
        }

        public string AfterPut()
        {
            return "OK";
        }

        public bool IsPodExist()
        {
            return (p_infoPod != null);
        }
        #endregion

        #region Teach RTR
        public class TeachRTR
        {
            int[] m_teachPlate = new int[2] { 0, 0 };
            int[] m_teachCover = new int[2] { 0, 0 };

            public int GetTeach(InfoPod infoPod)
            {
                int nTurn = infoPod.p_bTurn ? 1 : 0;
                switch (infoPod.p_ePod)
                {
                    case InfoPod.ePod.EIP_Cover: return m_teachCover[nTurn];
                    case InfoPod.ePod.EIP_Plate: return m_teachPlate[nTurn]; 
                }
                return -1; 
            }

            public void RunTree(Tree tree)
            {
                RunTree(tree.GetTree("EIP Cover"), m_teachCover);
                RunTree(tree.GetTree("EIP Plate"), m_teachPlate);
            }

            void RunTree(Tree tree, int[] teach)
            {
                teach[0] = tree.Set(teach[0], teach[0], "Top", "RND RTR Teach");
                teach[1] = tree.Set(teach[1], teach[1], "Bottom", "RND RTR Teach");
            }
        }
        TeachRTR m_teach; 

        public int GetTeachRTR(InfoPod infoPod)
        {
            return m_teach.GetTeach(infoPod); 
        }

        public void RunTreeTeach(Tree tree)
        {
            m_teach.RunTree(tree.GetTree(p_id));
        }
        #endregion

        public Buffer()
        {
            m_teach = new TeachRTR(); 
        }
    }
}
