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
    }
}
