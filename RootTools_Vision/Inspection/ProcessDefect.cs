using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class ProcessDefect : WorkBase
    {
        public ProcessDefect()
        {
        }

        public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS;


        protected override bool Preparation()
        {
            return true;
        }

        protected override bool Execution()
        {
            ProcessDefectParameter param = this.recipe.GetItem<ProcessDefectParameter>();
            if (param.Use == false)
                return true;

            DoProcessDefect();
            return true;
        }

        public void DoProcessDefect()
        {
            if (this.currentWorkplace.Index == 0) return;

            ProcessDefectParameter param = this.recipe.GetItem<ProcessDefectParameter>();

            List<Defect> MergeDefectList = Tools.MergeDefect(this.currentWorkplace.DefectList, param.MergeDefectDistnace);

            TempLogger.Write("Defect", string.Format("Merge : {0}", MergeDefectList.Count));

            foreach (Defect defect in MergeDefectList)
            {
                OriginRecipe originRecipe = this.recipe.GetItem<OriginRecipe>();
                defect.CalcAbsToRelPos(originRecipe.OriginX, originRecipe.OriginY); // Frontside
            }

            this.currentWorkplace.DefectList = MergeDefectList;

            WorkEventManager.OnProcessDefectDone(this.currentWorkplace, new ProcessDefectDoneEventArgs());
        }
    }
}
