using RootTools;
using RootTools.Comm;
using RootTools.Module;

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
            if (bInit) { }
        }
        #endregion


        public RinseL(string id, IEngineer engineer)
        {
            p_id = id;
            p_eRemote = eRemote.Client; 
            InitBase(id, engineer); 
        }
    }
}
