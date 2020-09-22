using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RootTools_Vision;

namespace RootTools_Vision.Test
{
    class WIND2_InspectionManager_ : WorkFactory
    {
        protected override void InitWorkManager()
        {
            this.Add(new WorkManager("Position", UserTypes.WORK_TYPE.PREPARISON, WORKPLACE_STATE.READY, WORKPLACE_STATE.NONE));
            this.Add(new WorkManager("Inspection", UserTypes.WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, 8));
            this.Add(new WorkManager("ProcessDefect", UserTypes.WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION));
        }


        // Inspection Data를 WorkBundle 형태로 변환
        public void SetInspectionData()
        {
            //this.SetBundles();
        }
    }
}
