using RootTools.Database;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.IO;
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

			List<Defect> defectList = new List<Defect>();

			foreach (Workplace workplace in workplaceBundle)
				foreach (Defect defect in workplace.DefectList)
					defectList.Add(defect);

            for (int i = 0; i < defectList.Count; i++)
            {
                defectList[i].SetDefectIndex(i);
            }

            //string sDefectimagePath = @"D:\DefectImage";
			//string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			//SaveDefectImage(Path.Combine(sDefectimagePath, sInspectionID), defectList, this.currentWorkplace.SharedBufferByteCnt);

            DatabaseManager.Instance.AddDefectDataList(defectList);

			WorkEventManager.OnProcessMeasurementDone(this.currentWorkplace, new ProcessMeasurementDoneEventArgs());
		}

		private void SaveDefectImage(String path, List<Defect> defectList, int byteCnt)
		{
            path += "\\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                di.Create();

            if (defectList.Count < 1)
                return;

            unsafe
            {
                Cpp_Rect[] defectArray = new Cpp_Rect[defectList.Count];

                for (int i = 0; i < defectList.Count; i++)
                {
                    Cpp_Rect rect = new Cpp_Rect();
					rect.x = (int)defectList[i].p_rtDefectBox.Left;
					rect.y = (int)defectList[i].p_rtDefectBox.Top;
					rect.w = (int)defectList[i].m_fWidth;
					rect.h = (int)defectList[i].m_fHeight;

					defectArray[i] = rect;
                }

                if (byteCnt == 1)
                {
                    CLR_IP.Cpp_SaveDefectListBMP(
                       path,
                       (byte*)currentWorkplace.SharedBufferR_GRAY.ToPointer(),
                       currentWorkplace.SharedBufferWidth,
                       currentWorkplace.SharedBufferHeight,
                       defectArray);
                }

                else if (byteCnt == 3)
                {
                    CLR_IP.Cpp_SaveDefectListBMP_Color(
                        path,
                       (byte*)currentWorkplace.SharedBufferR_GRAY.ToPointer(),
                       (byte*)currentWorkplace.SharedBufferG.ToPointer(),
                       (byte*)currentWorkplace.SharedBufferB.ToPointer(),
                       currentWorkplace.SharedBufferWidth,
                       currentWorkplace.SharedBufferHeight,
                       defectArray);
                }
            }
        }
	}
}
