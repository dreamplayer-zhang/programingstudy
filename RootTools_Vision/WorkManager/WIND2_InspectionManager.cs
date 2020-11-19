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
            this.Add(new WorkManager("Position", WORK_TYPE.PREPARISON, WORKPLACE_STATE.READY, WORKPLACE_STATE.NONE, STATE_CHECK_TYPE.CHIP));
            this.Add(new WorkManager("Inspection", WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, STATE_CHECK_TYPE.CHIP, 8));
            this.Add(new WorkManager("ProcessDefect", WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION, STATE_CHECK_TYPE.CHIP));
        }


        // Inspection Data를 WorkBundle 형태로 변환
        public void SetInspectionData()
        {
            //this.SetBundles();
        }
    }
}
