using RootTools.Comm;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class LightTool_Kwangwoo : NotifyProperty, ILightTool
    {
        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

        #region Type
        public enum eType
        {
            Power3,
            Power4,
            Halogen
        };
        public eType m_eType = eType.Power3;
        #endregion

        #region Property
        public string p_id { get; set; }

        string _sInfo = "OK";
        public string p_sInfo
        {
            get { return _sInfo; }
            set
            {
                if (value == _sInfo) return;
                _sInfo = value;
                OnPropertyChanged();
                if (value == "OK") return;
                m_log.Error(value);
            }
        }
        #endregion

        #region UI
        public UserControl p_ui
        {
            get
            {
                LightTool_Kwangwoo_UI ui = new LightTool_Kwangwoo_UI();
                ui.Init(this);
                return (UserControl)ui;
            }
        }
        #endregion

        #region Light
        public class Light : LightBase
        {
            protected override void GetPower() { }

            string m_sSend = "";
            public override void SetPower()
            {
                double fPower = p_bOn ? p_fSetPower : 0;
                fPower *= p_fScalePower;
                m_sSend = GetCommand(fPower); 
                m_rs232.Send(m_sSend); 
            }

            const int c_lMaxPower = 999;
            string GetCommand(double fPower)
            {
                int nPower = (int)Math.Round(c_lMaxPower * fPower / 100);
                switch (m_lightTool.m_eType)
                {
                    case eType.Power3: return "S" + m_sAddress + "W" + nPower.ToString("000") + "000000001100000E";
                    case eType.Power4: return "S" + m_sAddress + "W" + nPower.ToString("0000") + "00000001100000E";
                    case eType.Halogen: return "s" + m_sAddress + "DAC=>" + nPower.ToString("000") + "e";
                }
                return "";
            }

            private void M_rs232_OnRecieve(string sRead)
            {
                m_lightTool.p_sInfo = Recieve(sRead);
                if (m_lightTool.p_sInfo == "OK") p_fGetPower = p_fSetPower;
                else m_lightTool.m_log.Warn(m_sSend + " -> " + sRead);
            }

            string Recieve(string sRead)
            {
                switch (m_lightTool.m_eType)
                {
                    case eType.Power3:
                    case eType.Power4:
                        if (sRead.Length != m_sSend.Length) return "Invalid Protocol Length";
                        sRead = sRead.Replace('R', 'W');
                        if (sRead != m_sSend) return "Invalid Protocol Recieve";
                        break; 
                }
                return "OK"; 
            }

            public int m_nAddress = 0;
            LightTool_Kwangwoo m_lightTool; 
            RS232 m_rs232;
            public Light(string id, int nCh, LightTool_Kwangwoo lightTool)
            {
                m_nAddress = nCh;
                m_lightTool = lightTool; 
                m_rs232 = lightTool.m_rs232;
                Init(id + "." + nCh.ToString("00"), nCh);
                m_rs232.OnRecieve += M_rs232_OnRecieve;
            }

            string m_sAddress = "00";
            public void RunTree(Tree tree)
            {
                m_nAddress = tree.Set(m_nAddress, m_nAddress, p_id, "Light kwangwoo Address");
                m_sAddress = m_nAddress.ToString("00"); 
            }
        }

        List<LightBase> m_aLight = new List<LightBase>();
        public List<LightBase> p_aLight { get { return m_aLight; } }
        public int p_lLight
        {
            get { return m_aLight.Count; }
            set
            {
                if (m_aLight.Count == value) return;
                while (m_aLight.Count > value) m_aLight.RemoveAt(m_aLight.Count - 1); 
                while (m_aLight.Count < value)
                {
                    Light light = new Light(p_id, m_aLight.Count, this);
                    m_aLight.Add(light); 
                }
                if (OnChangeTool != null) OnChangeTool();
            }
        }

        public LightBase GetLight(int nCh, string sNewID)
        {
            if (nCh < 0) return null;
            if (nCh >= p_lLight) return null;
            if (m_aLight[nCh].p_sID != m_aLight[nCh].p_id) return null;
            m_aLight[nCh].p_sID = sNewID;
            if (OnChangeTool != null) OnChangeTool();
            return m_aLight[nCh];
        }

        public void Deselect(LightBase light)
        {
            light.Deselect();
            if (OnChangeTool != null) OnChangeTool();
        }
        #endregion

        #region Tree
        private void M_treeRoot_UpdateTree()
        {
            RunTree(Tree.eMode.Update);
            RunTree(Tree.eMode.Init);
            RunTree(Tree.eMode.RegWrite);
        }

        public void RunTree(Tree.eMode mode)
        {
            m_treeRoot.p_eMode = mode;
            RunSetupTree(m_treeRoot.GetTree("Setup"));
            foreach (Light light in m_aLight) light.RunTree(m_treeRoot.GetTree("Address"));
        }

        void RunSetupTree(Tree tree)
        {
            m_eType = (eType)tree.Set(m_eType, m_eType, "Type", "Kwangwoo Type");
            int lLight = p_lLight; 
            p_lLight = tree.Set(p_lLight, 1, "Count", "Kwangwoo Channel Count");
            if (lLight != p_lLight)
            {
                //m_treeRoot.GetTree("Address").m_aItem.Clear(); 
                if (OnChangeTool != null) OnChangeTool();
            }
        }
        #endregion

        IEngineer m_engineer;
        Log m_log;
        public RS232 m_rs232;
        public TreeRoot m_treeRoot; 
        public LightTool_Kwangwoo(string id, IEngineer engineer)
        {
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            m_rs232 = new RS232(id, m_log);
            m_treeRoot = new TreeRoot(id, m_log);
            m_treeRoot.UpdateTree += M_treeRoot_UpdateTree;
            RunTree(Tree.eMode.RegRead); 
        }

        public void ThreadStop()
        {
            m_rs232.ThreadStop();
        }
    }
}
