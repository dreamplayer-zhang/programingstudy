using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager4
{
    interface IWorkPipeLine
    {
        void Start();
        void Stop();

        void SetWork(WorkBundle works);
    }
}
