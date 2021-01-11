using RootTools;
using RootTools.Comm;
using RootTools.Module;
using System;
using System.IO;
using System.Threading;

namespace Root_Rinse_Unloader.Module
{
    public class RinseU : ModuleBase
    {
        #region eRunMode
        public enum eRunMode
        {
            Magazine,
            Stack
        }

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

        #region ToolBox
        public override void GetTools(bool bInit)
        {
            if (bInit) 
            {
                EQ.m_EQ.OnChanged += M_EQ_OnChanged; //forget
            }
        }

        private void M_EQ_OnChanged(_EQ.eEQ eEQ, dynamic value)
        {
            m_remote.RemoteSend(Remote.eProtocol.EQ, eEQ.ToString(), value.ToString());
        }
        #endregion

        #region TCPIP
        private void M_tcpip_EventReciveData(byte[] aBuf, int nSize, System.Net.Sockets.Socket socket)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        public RinseU(string id, IEngineer engineer)
        {
            p_id = id;
            p_eRemote = eRemote.Server; 
            InitBase(id, engineer);
        }

    }
}
