using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WorkBundle : Collection<IWork>
    {
        private Workplace workplace;

        public Workplace Workplace 
        { 
            get => workplace; 
            set
            { 
                this.workplace = value;
                foreach (IWork work in this)
                {
                    work.SetWorkplace(this.workplace);
                }

            } 
        }

        public WorkBundle Clone()
        {
            WorkBundle bundle = new WorkBundle();
            foreach(IWork work in this)
            {
                bundle.Add(work);
            }

            return bundle;
        }
    }
}
