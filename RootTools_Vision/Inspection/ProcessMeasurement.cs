using RootTools.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class ProcessMeasurement : WorkBase
	{
		public override WORK_TYPE Type => WORK_TYPE.DEFECTPROCESS_ALL;

		

		protected override bool Preparation()
		{
			return true;
		}

		protected override bool Execution()
		{
			DoProcessMeasurement();
			return true;
		}

		public void DoProcessMeasurement()
		{
			if (this.currentWorkplace.Index != 0)
				return;

			List<Defect> DefectList = new List<Defect>();

			foreach (Workplace workplace in workplaceBundle)
				foreach (Defect defect in workplace.DefectList)
					DefectList.Add(defect);

			DatabaseManager.Instance.AddDefectDataList(DefectList);

			WorkEventManager.OnProcessMeasurementDone(this.currentWorkplace, new ProcessMeasurementDoneEventArgs());
		}
	}
}
