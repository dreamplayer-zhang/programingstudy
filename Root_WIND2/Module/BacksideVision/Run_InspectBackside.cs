using RootTools;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2.Module
{
    public class Run_InspectBackside : ModuleRunBase
    {
        BackSideVision m_module;

        string m_sRecipeName = string.Empty;

        // Grab 관련 파라매터 (이거 나중에 구조 변경 필요할듯)
        //bool m_bInvDir = false;
        public GrabModeBack m_grabMode = null;
        string m_sGrabMode = "";

        #region [Getter Setter]
        public string RecipeName
        {
            get => m_sRecipeName;
            set => m_sRecipeName = value;
        }

        public string p_sGrabMode
        {
            get { return m_sGrabMode; }
            set
            {
                m_sGrabMode = value;
                m_grabMode = m_module.GetGrabMode(value);
            }
        }
        #endregion

        public Run_InspectBackside(BackSideVision module)
        {
            m_module = module;
            InitModuleRun(module);
        }

        public override ModuleRunBase Clone()
        {
            Run_InspectBackside run = new Run_InspectBackside(m_module);
            run.p_sGrabMode = p_sGrabMode;
            run.RecipeName = this.RecipeName;
            return run;
        }

        public override void RunTree(Tree tree, bool bVisible, bool bRecipe = false)
        {
            m_sRecipeName = tree.SetFile(m_sRecipeName, m_sRecipeName, "rcp", "Recipe", "Recipe Name", bVisible);
            // 이거 다 셋팅 되어 있는거 가져와야함
            p_sGrabMode = tree.Set(p_sGrabMode, p_sGrabMode, m_module.p_asGrabMode, "Grab Mode", "Select GrabMode", bVisible);
        }

        public override string Run()
        {
            //레시피에 GrabMode 저장하고 있어야함
            InspectionManagerBackside inspectionBackside = GlobalObjects.Instance.Get<InspectionManagerBackside>();
            inspectionBackside.Stop();

            if (m_grabMode == null) return "Grab Mode == null";

            if (EQ.IsStop() == false)
            {
                if (inspectionBackside.Recipe.Read(m_sRecipeName) == false)
                    return "Recipe Open Fail";

                inspectionBackside.Start();

            }
            else
            {
                inspectionBackside.Stop();
            }

            while (inspectionBackside.CheckAllWorkDone() == false)
            {
                if (EQ.IsStop())
                    return "OK";

                Task.Delay(1000);
            }

            m_grabMode.SetLight(false);
            return "OK";
        }
    }
}
