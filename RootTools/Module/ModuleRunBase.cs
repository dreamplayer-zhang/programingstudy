using RootTools.GAFs;
using RootTools.Gem;
using RootTools.Trees;
using System.Windows.Media;

namespace RootTools.Module
{
    public class ModuleRunBase : NotifyProperty
    {
        #region eRunState
        public enum eRunState
        {
            Ready,
            Run,
            Done
        }
        eRunState _eRunState = eRunState.Ready;
        public eRunState p_eRunState
        { 
            get { return _eRunState; }
            set
            {
                if (_eRunState == value) return;
                _eRunState = value;
                OnPropertyChanged();
                OnPropertyChanged("p_brushState");
            }
        }

        public Brush p_brushState
        { 
            get
            {
                switch (p_eRunState)
                {
                    case eRunState.Ready: return Brushes.LightGreen;
                    case eRunState.Run: return Brushes.Yellow;
                    case eRunState.Done: return Brushes.LightBlue; 
                }
                return Brushes.White; 
            }
        }
        #endregion

        string _id = "";
        public string p_id { get { return _id; } }

        public string m_sModuleRun;
        protected Log m_log;
        public ModuleBase m_moduleBase = null;
        public dynamic m_infoObject;
        public GemPJ m_pj = null;

        protected string p_sInfo
        {
            get { return m_moduleBase.p_sInfo; }
        }

        #region virtual 
        protected virtual void InitModuleRun(ModuleBase module)
        {
            m_moduleBase = module;
            string[] asName = this.GetType().Name.Split('_');
            m_sModuleRun = asName[asName.Length - 1];
            _id = module.p_id + "." + m_sModuleRun;
            m_log = module.m_log;
        }

        public virtual ModuleRunBase Clone() { return null; }

        public virtual void RunTree(Tree tree, bool bVisible, bool bRecipe = false) { }

        public virtual string IsRunOK()
        {
            if (m_moduleBase.p_eState != ModuleBase.eState.Ready) return "Module State not Ready : " + m_moduleBase.p_eState.ToString();
            return "OK";
        }

        public virtual string Run() { return "OK"; }
        #endregion

        public string StartRun()
        {
            return m_moduleBase.StartRun(this);
        }

        public ALID m_alid;
        public void SetALID(string sMsg)
        {
            if (m_alid == null) return;
            m_alid.p_sMsg = sMsg;
            m_alid.p_bSet = true;
        }
    }
}
