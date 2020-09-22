using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools_Vision;

namespace Root_WIND2
{
    public class WIND2_InspectionManager : WorkFactory
    {

        private Recipe m_Recipe;

        public Recipe Recipe { get => m_Recipe; set => m_Recipe = value; }

        protected override void InitWorkManager()
        {
            this.Add(new WorkManager("Position", RootTools_Vision.UserTypes.WORK_TYPE.PREPARISON, WORKPLACE_STATE.READY, WORKPLACE_STATE.NONE));
            this.Add(new WorkManager("Inspection", RootTools_Vision.UserTypes.WORK_TYPE.MAINWORK, WORKPLACE_STATE.INSPECTION, WORKPLACE_STATE.READY, 8));
            this.Add(new WorkManager("ProcessDefect", RootTools_Vision.UserTypes.WORK_TYPE.FINISHINGWORK, WORKPLACE_STATE.DEFECTPROCESS, WORKPLACE_STATE.INSPECTION));
        }
    }
}
