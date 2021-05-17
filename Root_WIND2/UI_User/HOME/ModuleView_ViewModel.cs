using RootTools.Module;
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


        public ModuleView_ViewModel(ModuleBase module)
        {
            ModuleName = module.p_id;
            m_Module = module;
        }

        public void AddMode(string sName)
        {
            modeList.Add(new RunMode(sName));
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
    }

    public class RunMode : ObservableObject
    {
        public string sName
        {
            get; set;
        }

        public bool bChecked
        {
            get;set;
        }

        ObservableCollection<string> moduleRunList = new ObservableCollection<string>();
        public ObservableCollection<string> ModuleRuns
        {
            get{
                return moduleRunList;
            }
            set
            {
                SetProperty(ref moduleRunList, value);
            }
        }

        public RunMode(string Name)
        {
            sName = Name;
        }
    }
}
