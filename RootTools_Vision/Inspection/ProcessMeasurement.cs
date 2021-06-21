using RootTools.Database;
using RootTools_CLR;
using RootTools_Vision.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static RootTools_Vision.Tools;

namespace RootTools_Vision
{
	public class ProcessMeasurement : WorkBase
	{
		string TableName = "measurement";
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

		/// <summary>
		/// EBR Measurement Data Processing
		/// </summary>
        public void DoProcessMeasurement()
        {
            if (this.currentWorkplace.Index != 0)
                return;

			List<Measurement> measureList = new List<Measurement>();

			foreach (Workplace workplace in workplaceBundle)
                foreach (Measurement measure in workplace.MeasureList)
                    measureList.Add(measure);

            foreach (Measurement measure in measureList)
                this.currentWorkplace.MeasureList.Add(measure);

            int measureItem = Enum.GetValues(typeof(Measurement.EBRMeasureItem)).Length;

            for (int i = 0, index = 1; i < measureList.Count; i += measureItem, index++)
                for (int j = 0; j < measureItem; j++)
                    measureList[i + j].SetMeasureIndex(index);

			if (measureList.Count > 0)
				DatabaseManager.Instance.AddMeasurementDataList(measureList);

			#region Klarf / Defect Image 저장

			// Klarf에만 표시되는 Data - 이거 이제 필요없는데
			List<Measurement> realKlarfData = new List<Measurement>();
			for (int i = 0; i < measureList.Count; i++)
			{
				if (measureList[i].m_strMeasureItem == Measurement.EBRMeasureItem.EBR.ToString())
					realKlarfData.Add(measureList[i]);
			}

			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			Settings settings = new Settings();
			SettingItem_SetupEBR settings_ebr = settings.GetItem<SettingItem_SetupEBR>();

            // EBR 측정 이미지 저장
			SaveDefectImageParallel(Path.Combine(settings_ebr.MeasureImagePath, sInspectionID)
										, measureList
										, this.currentWorkplace.SharedBufferInfo
										, this.currentWorkplace.SharedBufferInfo.ByteCnt
										, new System.Windows.Size(this.currentWorkplace.SharedBufferWidth, recipe.GetItem<EBRParameter>().ROIHeight));

			// EBR 원형 이미지 저장
			EBRRecipe recipeParam = this.recipe.GetItem<EBRRecipe>();
			Tools.SaveCircleImage(Path.Combine(settings_ebr.MeasureImagePath, sInspectionID, (realKlarfData.Count + 1).ToString()), settings_ebr.OutputImageSizeWidth, settings_ebr.OutputImageSizeHeight, settings_ebr.Thickness
                                  , this.currentWorkplace.SharedBufferInfo, recipeParam.FirstNotch, recipeParam.LastNotch);

            // Graph 이미지 저장
            List<float> ebrData = new List<float>();
            List<float> bevelData = new List<float>();
            for (int i = 0; i < measureList.Count; i++)
            {
                if (measureList[i].m_strMeasureItem == Measurement.EBRMeasureItem.EBR.ToString())
                    ebrData.Add(measureList[i].m_fData);
                if (measureList[i].m_strMeasureItem == Measurement.EBRMeasureItem.Bevel.ToString())
                    bevelData.Add(measureList[i].m_fData);
            }
            Tools.SaveEBRChartToImage(Path.Combine(settings_ebr.MeasureImagePath, sInspectionID, (realKlarfData.Count + 2).ToString()), ebrData, bevelData);

			#endregion
            WorkEventManager.OnProcessMeasurementDone(this.currentWorkplace, new ProcessMeasurementDoneEventArgs());
        }

        public void SaveDefectImageParallel(String path, List<Measurement> measurementList, SharedBufferInfo sharedBuffer, int nByteCnt, System.Windows.Size size = new System.Windows.Size())
        {
            path += "\\";
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.Exists)
                di.Create();

            if (measurementList.Count < 1)
                return;

            //Parallel.ForEach(measurementList, measure =>
            foreach (Measurement measure in measurementList)
            {
                double cx = (measure.p_rtDefectBox.Left + measure.p_rtDefectBox.Right) / 2;
                double cy = (measure.p_rtDefectBox.Top + measure.p_rtDefectBox.Bottom) / 2;

                int startX = (int)cx - 320;
                int startY = (int)cy - 240;
                int width = 640;
                int height = 480;

                System.Windows.Rect imageRect = new System.Windows.Rect(startX, startY, width, height);

                if (size != System.Windows.Size.Empty)
                {
                    startX = (int)(cx - (size.Width / 2));
                    if (startX < 0)
                        startX = 0;

                    startY = (int)(cy - (size.Height / 2));
                    width = (int)size.Width;
                    height = (int)size.Height;
                }

                System.Drawing.Bitmap bitmap;
                if (System.IO.File.Exists(path + measure.m_nMeasurementIndex + ".bmp"))
                    bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(path + measure.m_nMeasurementIndex + ".bmp", true);
                else
                    bitmap = Tools.CovertBufferToBitmap(sharedBuffer, new System.Windows.Rect(startX, startY, width, height));

                System.Drawing.Bitmap temp = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height);
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(temp);
                graphics.DrawImage(bitmap, 0, 0);
                bitmap.Dispose();

                System.Drawing.Pen pen;
                int offset = 0;
                if (measure.m_strMeasureItem == Measurement.EBRMeasureItem.EBR.ToString())
                {
                    pen = new System.Drawing.Pen(System.Drawing.Color.Red, 3);
                    offset = 10;
                }
                else
                    pen = new System.Drawing.Pen(System.Drawing.Color.Green, 3);

                //double resolution = this.currentWorkplace.CameraInfo.TargetResX;
                int rectStartX = (int)(measure.m_fRelX - (measure.m_fData /*/ resolution*/));
                int rectWidth = (int)measure.m_fRelX - rectStartX;
				int rectStartY = (height / 2);
				graphics.DrawLine(pen, rectStartX, rectStartY + offset, rectStartX + rectWidth, rectStartY + offset);

                System.Drawing.Font myFont = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 100, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
                graphics.DrawString("Angle : " + measure.m_fAngle.ToString("F3"), myFont, new System.Drawing.SolidBrush(System.Drawing.Color.Red), 100, 100);

                if (System.IO.File.Exists(path + measure.m_nMeasurementIndex + ".bmp"))
                    System.IO.File.Delete(path + measure.m_nMeasurementIndex + ".bmp");

                temp.Save(path + measure.m_nMeasurementIndex + ".bmp");
            }
            //);
        }
        private static object lockObj = new object();
	}
}
