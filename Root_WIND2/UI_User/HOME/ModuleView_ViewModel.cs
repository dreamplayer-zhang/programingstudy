using RootTools.Module;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.UI_User
{
    public class ModuleView_ViewModel : ObservableObject
    {
        public string ModuleName
        {
            get;
            set;
        }

        public bool IsUseChecked
        {
            get;
            set;
        }
        public ModuleBase m_Module;


        bool bSettingView = false;
        public bool p_bSetting
        {
            get
            {
                return bSettingView;
            }
            set
            {
                SetProperty(ref bSettingView, value);
            }
        }

        public ModuleView_ViewModel(ModuleBase module)
        {
            if (module == null) return;
            ModuleName = module.p_id;
            m_Module = module;
            
            p_bSetting = false;
        }

        public void AddMode(string sName, ObservableCollection<string> modulruns)
        {
            Dictionary<string, bool> runs = new Dictionary<string, bool>();
            for (int i = 0; i < modulruns.Count; i++)
            {
                runs.Add(modulruns[i], false);
            }
            modeList.Add(new RunMode(sName, runs, ModuleName));
        }

        public void ChengeSettingPage()
        {
            p_bSetting = !p_bSetting;
        }

        ObservableCollection<RunMode> modeList = new ObservableCollection<RunMode>();
        public ObservableCollection<RunMode> ModeList
        {
            get
            {
                return modeList;
            }
            set
            {
                SetProperty(ref modeList, value);
            }
        }
        public RelayCommand CommandSettingClick
        {
            get
            {
                return new RelayCommand(ChengeSettingPage);
            }
        }

    }

    public class RunMode : ObservableObject
    {
        public string sName
        {
            get; set;
        }
        public string sGroupName { get; set; }
        bool _bChecked = false;
        public bool bChecked
        {
            get
            {
                return _bChecked;
            }
            set
            {
                SetProperty(ref _bChecked, value);
            }
        }


        ObservableCollection<Dictionary<string,bool>> moduleRunList = new ObservableCollection<Dictionary<string, bool>>();
        public ObservableCollection<Dictionary<string, bool>> ModuleRuns
        {
            get{
                return moduleRunList;
            }
            set
            {
                SetProperty(ref moduleRunList, value);
            }
        }

        private ModuleRunList m_moduleTempList;
        public ModuleRunList p_moduleTempList
        {
            get
            {
                return m_moduleTempList;
            }
            set
            {
                SetProperty(ref m_moduleTempList, value);
            }
        }

        //void SetModuleRun()
        //{
        //    if(sName == "OnlySnap")
        //    {
        //        m_moduleTempList.Add(sGroupName,  );
        //    }
        //}

        public RunMode(string Name, Dictionary<string,bool> runs, string ModuleName)
        {
            sName = Name;
            sGroupName = ModuleName;
            //modulrruns = runs;
        }
    }
}
