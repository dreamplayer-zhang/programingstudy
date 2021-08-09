using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager4
{
    interface IWorkPipe
    {
        void DoWork();

        void Enqueue(Workplace workplace);

        //Workplace Dequeue();

        void AddStep(WorkPipe nextPipe);
    }
}
