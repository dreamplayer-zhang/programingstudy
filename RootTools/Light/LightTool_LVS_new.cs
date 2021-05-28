using RootTools.Comm;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace RootTools.Light
{
    public class LightTool_LVS_new : ObservableObject, ILightTool
    {
        const int c_lLight = 4;

        public delegate void dgOnChangeTool();
        public event dgOnChangeTool OnChangeTool;

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
                RaisePropertyChanged();
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
                LightTool_LVS_new_UI ui = new LightTool_LVS_new_UI();
                ui.Init(this);
                return ui;
            }
        }
        #endregion

        #region Light
        public class Light : LightBase
        {
            protected override void GetPower() {}

            public override void SetPower()
            {
                double fPower = p_fSetPower;
                fPower *= p_fScalePower;
                int nPower = (int)Math.Round(fPower);
                SetChannel();
                Thread.Sleep(10);
                SetPower(nPower);
            }

            const int c_lMaxPower = 255;
            const int nByteCnt = 6;

            private void M_rs232_OnReceive(byte[] aRead, int nRead)
            {
                p_fGetPower = p_fSetPower;
            }
            void SetChannel()
            {
                byte[] aSend = new byte[nByteCnt];
                int i = 0;
                aSend[i++] = 0x01; //Start
                aSend[i++] = 0x00; //Op Write
                aSend[i++] = 0x01; //DL
                aSend[i++] = 0x20; //CSR(Addr)
                aSend[i++] = (byte)(1 << m_nCh);
                aSend[i++] = 0x04; //end

                m_rs232.Send(aSend, nByteCnt);
            }
            void SetPower(int nPower)
            {
                byte[] aSend = new byte[nByteCnt];
                int i = 0;
                aSend[i++] = 0x01; //Start
                aSend[i++] = 0x00; //Op Write
                aSend[i++] = 0x01; //DL
                aSend[i++] = 0x28; //SVR(Addr)
                aSend[i++] = BitConverter.GetBytes(nPower)[0];
                aSend[i++] = 0x04; //end

                m_rs232.Send(aSend, nByteCnt);
            }

            int m_nCh = 0;
            RS232byte m_rs232;
            public RS232byte p_rs232
            {
                get { return m_rs232; }
                set { SetProperty(ref m_rs232, value); }
            }
            public Light(string id, int nCh,RS232byte rs232)
            {
                m_nCh = nCh;
                p_rs232 = rs232;
                Init(id + "." + nCh.ToString("00"), nCh);
                p_rs232.OnReceive += M_rs232_OnReceive;
            }
        }

        public List<LightBase> p_aLight { get; set; }
        void InitLight()
        {
            m_rs232.p_bConnect = true;
            for (int n = 0; n < c_lLight; n++)
            {
                Light light = new Light(p_id, n, p_rs232);
                p_aLight.Add(light);
            }
        }

        public LightBase GetLight(int nCh, string sNewID)
        {
            if (nCh < 0) return null;
            if (nCh >= c_lLight) return null;
            if (p_aLight[nCh].p_sID != p_aLight[nCh].p_id) return null;
            p_aLight[nCh].p_sID = sNewID;
            if (OnChangeTool != null) OnChangeTool();
            return p_aLight[nCh];
        }

        public void Deselect(LightBase light)
        {
            light.Deselect();
            if (OnChangeTool != null) OnChangeTool();
        }
        #endregion

        IEngineer m_engineer;
        Log m_log;
        public RS232byte m_rs232;
        public RS232byte p_rs232
        {
            get { return m_rs232; }
            set { SetProperty(ref m_rs232, value); }
        }
        public LightTool_LVS_new(string id, IEngineer engineer)
        {
            p_aLight = new List<LightBase>();
            p_id = id;
            m_engineer = engineer;
            m_log = LogView.GetLog(id);
            p_rs232 = new RS232byte(id, m_log);
            InitLight();
        }

        public void ThreadStop()
        {
            p_rs232.ThreadStop();
        }
    }
}
