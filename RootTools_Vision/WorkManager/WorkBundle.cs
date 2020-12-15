using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WorkBundle : Collection<WorkBase>
    {
        private Workplace workplace;
        private WorkplaceBundle workplaceBundle;

        public Workplace Workplace 
        { 
            get => workplace; 
            set
            { 
                this.workplace = value;
                foreach (WorkBase work in this)
                {
                    work.SetWorkplace(this.workplace);
                }

            } 
        }

        public WorkplaceBundle WorkplaceBundle
        { 
            get => workplaceBundle;
            set
            {
                this.workplaceBundle = value;
                foreach (WorkBase work in this)
                {
                    work.SetWorkplaceBundle(this.workplaceBundle);
                }
            }
        }

        public WorkBundle Clone()
        {
            WorkBundle bundle = new WorkBundle();
            foreach(WorkBase work in this)
            {
                bundle.Add(work.Clone());
            }
            bundle.Workplace = this.Workplace;
            bundle.workplaceBundle = this.workplaceBundle;

            return bundle;
        }

        public static WorkBundle CreateWorkBundle(Recipe _recipe, WorkplaceBundle _workplaceBundle)
        {
            List<ParameterBase> paramList = _recipe.ParameterItemList;
            WorkBundle bundle = new WorkBundle();

            foreach(ParameterBase param in paramList)
            {
                WorkBase work = (WorkBase)Tools.CreateInstance(param.InspectionType);
                work.SetRecipe(_recipe);
                work.SetWorkplaceBundle(_workplaceBundle);

                bundle.Add(work);
            }

            ProcessDefect processDefect = new ProcessDefect();
            ProcessDefect_Wafer processDefect_Wafer = new ProcessDefect_Wafer();
            processDefect_Wafer.SetRecipe(_recipe);

            bundle.Add(processDefect);
            bundle.Add(processDefect_Wafer);

            return bundle;
        }
    }
}
