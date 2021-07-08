using System;
using System.Collections.Generic;
using System.Windows;
using Root_VEGA_P_Vision.Module;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    class InspectionManagerVegaP:WorkFactory
    {
        #region [Members]
        private readonly RecipeVision recipe;
        private readonly Vision vision;

        WorkplaceBundle workplaceBundle;

        #endregion

        #region [Properties]
        public RecipeVision Recipe
        {
            get => recipe;
        }
        public List<SharedBufferInfo> SharedBufferInfoList //stain, side,tdi, stacking 
        {
            get => sharedBufferInfoList;
        }
        #endregion
        public InspectionManagerVegaP(Vision vision, RecipeVision recipe, SharedBufferInfo bufferInfo) : base(REMOTE_MODE.Master)
        {
            this.vision = vision;
            this.recipe = recipe;

            sharedBufferInfoList.Add(bufferInfo);
        }

        #region [Override]
        protected override WorkBundle CreateWorkBundle()
        {
            List<ParameterBase> paramList = recipe.ParameterItemList;
            WorkBundle bundle = new WorkBundle();

            bundle.Add(new Snap());
            foreach (ParameterBase param in paramList)
            {
                WorkBase work = (WorkBase)Tools.CreateInstance(param.InspectionType);
                work.SetRecipe(recipe);
                work.SetParameter(param);

                bundle.Add(work);
            }

            ProcessDefect processDefect = new ProcessDefect();
            ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer("defect");

            bundle.Add(processDefect);
            bundle.Add(processDefect_Wafer);

            return bundle;
        }

        protected override WorkplaceBundle CreateWorkplaceBundle()
        {
            return CreateWorkplace_Pod();
        }

        protected override void Initialize()
        {
            //CreateWorkManager(WORK_TYPE.SNAP);
            //CreateWorkManager(WORK_TYPE.ALIGNMENT);
            CreateWorkManager(WORK_TYPE.INSPECTION, 6);
            CreateWorkManager(WORK_TYPE.DEFECTPROCESS, 6);
            CreateWorkManager(WORK_TYPE.DEFECTPROCESS_ALL, 1, true);

            VegaPEventManager.SnapDone += SnapDone_Callback;
        }

        protected override bool Ready(WorkplaceBundle workplaces, WorkBundle works)
        {
            works.SetRecipe(recipe);
            return true;
        }
        #endregion

        public WorkplaceBundle CreateWorkplace_Pod()
        {
            workplaceBundle = new WorkplaceBundle();

            GrabMode grabMode = null;
            foreach(GrabMode grab in vision.m_aGrabMode)
            {
                if (grab.p_id.Contains("Stain"))
                {
                    grabMode = grab;
                    break;
                }
            }
            //여기여기
            Workplace workplace = new Workplace(0, 0, 0, 0, 350/*grabMode.m_camera.p_sz.X*3*/, 180/*grabMode.m_camera.p_sz.Y*3*/, workplaceBundle.Count);
            workplace.SetSharedBuffer(sharedBufferInfoList[0]);
            workplaceBundle.Add(workplace);

            return workplaceBundle;
        }
        public new void Start()
        {
            if (this.Recipe == null)
                return;

            //DateTime inspectionStart = DateTime.Now;
            //DateTime inspectionEnd = DateTime.Now;
            //string lotId = "Lotid";
            //string partId = "Partid";
            //string setupId = "SetupID";
            //string cstId = "CSTid";
            //string waferId = "WaferID";
            ////string sRecipe = "RecipeID";
            //string recipeName = recipe.Name;

            //DatabaseManager.Instance.SetLotinfo(inspectionStart, inspectionEnd, lotId, partId, setupId, cstId, waferId, recipeName);

            base.Start();
        }
        public void SnapDone_Callback(object obj, SnapDoneArgs args)
        {
        }
    }
}