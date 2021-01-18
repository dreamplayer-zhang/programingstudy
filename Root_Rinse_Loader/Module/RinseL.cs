using RootTools;
using RootTools.Comm;
using RootTools.Module;
using System;

namespace Root_Rinse_Loader.Module
{
    public class RinseL : ModuleBase
    {
        #region eRunMode
        public enum eRunMode
        {
            Magazine,
            Stack
        }
        string[] m_asRunMode = Enum.GetNames(typeof(eRunMode));

        eRunMode _eMode = eRunMode.Magazine;
        public eRunMode p_eMode
        {
            get { return _eMode; }
            set
            {
                if (_eMode == value) return;
                _eMode = value;
                OnPropertyChanged();
            }
        }

        double _widthStrip = 77;
        public double p_widthStrip
        {
            get { return _widthStrip; }
            set
            {
                if (_widthStrip == value) return;
                _widthStrip = value;
                OnPropertyChanged();
            }
        }

        int _iMagazin = 0;
        public int p_iMagazine
        {
            get { return _iMagazin; }
            set
            {
                if (_iMagazin == value) return;
                _iMagazin = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Unloader State
        EQ.eState _eStateUnloader = EQ.eState.Init; 
        EQ.eState p_eStateUnloader
        {
            get { return _eStateUnloader; }
            set
            {
                if (_eStateUnloader == value) return;
                _eStateUnloader = value;
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region ToolBox
        TCPIPClient m_tcpip; 
        public override void GetTools(bool bInit)
        {
            p_sInfo = m_toolBox.Get(ref m_tcpip, this, "TCPIP"); 
            if (bInit) 
            {
                EQ.m_EQ.OnChanged += M_EQ_OnChanged;
                m_tcpip.EventReciveData += M_tcpip_EventReciveData;
            }
        }

        private void M_EQ_OnChanged(_EQ.eEQ eEQ, dynamic value)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        #region TCPIP
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, System.Net.Sockets.Socket socket)
        {
            throw new System.NotImplementedException();
        }
        #endregion


        public RinseL(string id, IEngineer engineer)
        {
            p_id = id;
            InitBase(id, engineer); 
        }
    }
}
