using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    [Serializable]
    public class WorkBundle : ObservableCollection<WorkBase>
    {
        public void SetRecipe(RecipeBase recipe)
        {
            foreach (WorkBase work in this)
            {
                work.SetRecipe(recipe);
            }
        }

        public void SetWorkplace(Workplace workplace)
        {
            foreach (WorkBase work in this)
            {
                work.SetWorkplace(workplace);
            }
        }

        public void SetWorkplacBundle(WorkplaceBundle workplaceBundle)
        {
            foreach (WorkBase work in this)
            {
                work.SetWorkplaceBundle(workplaceBundle);
            }
        }

        public void SetWorkplaceBuffer(byte[] bufferR_GRAY, byte[] bufferG, byte[] bufferB)
        {
            foreach (WorkBase work in this)
            {
                work.SetWorkplaceBuffer(bufferR_GRAY, bufferG, bufferB);
            }
        }

        public void SetGrabMode(RootTools.GrabModeBase grabMode)
		{
            foreach (WorkBase work in this)
			{
                work.SetGrabMode(grabMode);
			}
		}

        public void Reset()
        {
            foreach (WorkBase w in this)
                w.Reset();
        }

        public WorkBundle Clone()
        {
            WorkBundle works = new WorkBundle();
            foreach(WorkBase wb in this)
            {
                works.Add(wb.Clone());
            }
            return works;
        }

        public WorkBundle CloneForRemote()
        {
            WorkBundle wb = new WorkBundle();
            foreach(WorkBase work in this)
            {
                wb.Add(new CloneableWorkBase(work.GetType().ToString()));
            }

            return wb;
        }

        // 다시 고려

        //public WorkBundle Clone()
        //{
        //    WorkBundle bundle = new WorkBundle();
        //    foreach(WorkBase work in this)
        //    {
        //        bundle.Add(work.Clone());
        //    }
        //    bundle.Workplace = this.Workplace;
        //    bundle.workplaceBundle = this.workplaceBundle;

        //    return bundle;
        //}
    }
}
