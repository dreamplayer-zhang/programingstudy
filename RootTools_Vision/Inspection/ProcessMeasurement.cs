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

            List<Measurement> measureList = new List<Measurement>();

            foreach (Workplace workplace in workplaceBundle)
                foreach (Measurement measure in workplace.MeasureList)
                    measureList.Add(measure);

            // TO DO 이거 측정마다 다를건데 어떻게 할거임
            int measureItem = Enum.GetValues(typeof(EBRMeasurement.EBRMeasureItem)).Length;
            for (int i = 0; i < measureList.Count; i += measureItem)
                for (int j = 0; j < measureItem; j++)
                    measureList[i+j].SetMeasureIndex(i);

			string sMeasurementImagePath = @"D:\MeasurementImage";
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			SaveMeasurementImage(Path.Combine(sMeasurementImagePath, sInspectionID), measureList, this.currentWorkplace.SharedBufferByteCnt);
			DatabaseManager.Instance.AddMeasurementDataList(measureList);

			WorkEventManager.OnProcessMeasurementDone(this.currentWorkplace, new ProcessMeasurementDoneEventArgs());
		}

		private void SaveMeasurementImage(String path, List<Measurement> measureList, int byteCnt)
		{
            path += "\\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                di.Create();

            if (measureList.Count < 1)
                return;

            unsafe
            {
                Cpp_Rect[] defectArray = new Cpp_Rect[measureList.Count];

                for (int i = 0; i < measureList.Count; i++)
                {
                    Cpp_Rect rect = new Cpp_Rect();
					rect.x = (int)measureList[i].m_rtDefectBox.Left;
					rect.y = (int)measureList[i].m_rtDefectBox.Top;
					rect.w = (int)measureList[i].m_fWidth;
					rect.h = (int)measureList[i].m_fHeight;

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
