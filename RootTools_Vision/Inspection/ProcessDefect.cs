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
        Workplace workplace;

        public ProcessDefect()
        {
        }

        public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS;

        public override void SetRecipe(Recipe _recipe)
        {
            m_sName = this.GetType().Name;
        }

        public override void DoWork()
        {
            DoProcessDefect();
        }

        public override void SetWorkplace(Workplace _workplace)
        {
            this.workplace = _workplace;
        }

        public void DoProcessDefect()
        {
            // 
            // Add
            //if (workplace.DefectList.Count > 0)
            //{

            //    if (workplace.DefectList.Count == 1)
            //    {
            //        foreach (Defect defect in workplace.DefectList)
            //        {
            //            DatabaseManager.Instance.AddDefectData(defect);
            //        }
            //    }
            //    else
            //    {
            //        DatabaseManager.Instance.AddDefectDataList(workplace.DefectList);
            //    }
            //}
            WorkEventManager.OnProcessDefectDone(this.workplace, new ProcessDefectDoneEventArgs());
        }

        public override WorkBase Clone()
        {
            return (WorkBase)this.MemberwiseClone();
        }

        public override void SetWorkplaceBundle(WorkplaceBundle workplace)
        {
            return;
        }


    }
}
