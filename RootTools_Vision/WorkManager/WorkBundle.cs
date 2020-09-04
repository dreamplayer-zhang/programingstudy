using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WorkBundle : Collection<IWork>
    {
        public WorkBundle()
        {

        }

        public WorkBundle(WorkBundle _workbundle)
        {
            foreach (IWork work in _workbundle)
                this.Add(work);
        }
    }
}
