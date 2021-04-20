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

        #endregion

        #region [Properties]
        public RecipeVision Recipe
        {
            get => recipe;
        }
        public List<SharedBufferInfo> SharedBufferInfoList
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
            if (recipe.WaferMap == null)
            {
                MessageBox.Show("Map 정보가 없습니다.");
                return null;
            }
            if (recipe.WaferMap.MapSizeX == 0 || recipe.WaferMap.MapSizeY == 0)
            {
                MessageBox.Show("Map 정보가 없습니다.");
                return null;
            }

            return CreateWorkplaceBundle_WaferMap();
        }

        protected override void Initialize()
        {
            CreateWorkManager(WORK_TYPE.SNAP);
            CreateWorkManager(WORK_TYPE.ALIGNMENT);
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

        public WorkplaceBundle CreateWorkplaceBundle_WaferMap()
        {
            return null;
        }
        public void SnapDone_Callback(object obj, SnapDoneArgs args)
        {
        }
    }
}