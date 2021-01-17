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
		Workplace workplace;
		WorkplaceBundle workplaceBundle;

		public override WORK_TYPE Type => WORK_TYPE.MEASUREMENTPROCESS;

		public override WorkBase Clone()
		{
			return (WorkBase)this.MemberwiseClone();
		}

		public override void SetRecipe(Recipe _recipe)
		{
			m_sName = this.GetType().Name;
		}

		public override void SetWorkplace(Workplace workplace)
		{
			this.workplace = workplace;
		}

		public override void SetWorkplaceBundle(WorkplaceBundle workplace)
		{
			this.workplaceBundle = workplace;
		}

		public override void DoWork()
		{
			DoProcessMeasurement();
		}

		List<Defect> DefectList = new List<Defect>();
		public void DoProcessMeasurement()
		{
			if (!(this.workplace.MapPositionX == -1 && this.workplace.MapPositionY == -1))
				return;

			foreach (Workplace workplace in workplaceBundle)
				foreach (Defect defect in workplace.DefectList)
					DefectList.Add(defect);

			DatabaseManager.Instance.AddDefectDataList(DefectList);

			WorkEventManager.OnProcessMeasurementDone(this.workplace, new ProcessMeasurementDoneEventArgs());
		}
	}
}
