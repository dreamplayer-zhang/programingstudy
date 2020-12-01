using RootTools;
using RootTools.Database;
using RootTools_CLR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RootTools_Vision.Inspection
{
	public class EdgeSurface : WorkBase
	{
		WorkplaceBundle workplaceBundle;
		Workplace workplace;


		public override WORK_TYPE Type => WORK_TYPE.MAINWORK;

		public override WorkBase Clone()
		{
			return (WorkBase)this.MemberwiseClone();
		}

		public override void DoWork()
		{
			DoInspection();
			//base.DoWork();
		}

		public override void SetRecipe(Recipe _recipe)
		{
			
		}

		public override void SetWorkplace(Workplace _workplace)
		{
			this.workplace = _workplace;
		}

		public override void SetWorkplaceBundle(WorkplaceBundle _workplace)
		{
			this.workplaceBundle = _workplace;
		}

		public void DoInspection()
		{
			//if (this.workplace.Index == 0)
			//	return;
						
			int roiHeight = 1000; // recipe
			int threshold = 12; // recipe
			int defectSize = 5; // recipe

			//int memH = this.workplace.SharedBufferHeight;
			int memW = this.workplace.SharedBufferWidth;
			int memSize = memW * roiHeight;

			int left = this.workplace.PositionX;
			int top = this.workplace.PositionY;
			int right = this.workplace.PositionX + this.workplace.SharedBufferWidth; 
			int bottom = this.workplace.PositionY + roiHeight;

			byte[] arrSrc = new byte[memSize];
			for (int cnt = top; cnt < bottom; cnt++)
			{
				Marshal.Copy(new IntPtr(this.workplace.SharedBuffer.ToInt64() + (cnt * (Int64)memW))
							, arrSrc
							, memW * (cnt - top)
							, memW);
			}

			//Emgu.CV.Mat mat = new Emgu.CV.Mat((int)roiHeight, (int)memW, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
			//Marshal.Copy(arrSrc, 0, mat.DataPointer, arrSrc.Length);
			//mat.Save(@"D:\mat_" + this.workplace.Index.ToString() + ".bmp");

			// profile 생성
			List<int> temp = new List<int>();
			List<int> profile = new List<int>();
			for (long j = 0; j < memW; j++)
			{
				temp.Clear();
				for (long i = 0; i < memSize; i += memW)
				{
					temp.Add(arrSrc[j + i]);
				}
				temp.Sort();
				profile.Add(temp[temp.Count / 2]);  // 중앙값
			}

			// Calculate diff image (original - profile)
			byte[] diff = new byte[memSize];
			for (int j = 0; j < roiHeight; j++)
			{
				for (int i = 0; i < memW; i++)
				{
					diff[(j * memW) + i] = (byte)(Math.Abs(arrSrc[(j * memW) + i] - profile[i]));
				}
			}

			// Threshold
			byte[] thresh = new byte[memSize];
			CLR_IP.Cpp_Threshold(diff, thresh, memW, roiHeight, false, threshold);
			var label = CLR_IP.Cpp_Labeling(diff, thresh, memW, roiHeight, true);

			// Add defect
			string sInspectionID = DatabaseManager.Instance.GetInspectionID();
			for (int i = 0; i < label.Length; i++)
			{
				if (label[i].area > defectSize)
				{
					this.workplace.AddDefect(sInspectionID,
						10001,
						label[i].area,
						label[i].value,
						this.workplace.PositionX + label[i].boundLeft,
						this.workplace.PositionY - label[i].boundTop,
						Math.Abs(label[i].boundRight - label[i].boundLeft),
						Math.Abs(label[i].boundBottom - label[i].boundTop),
						this.workplace.MapPositionX,
						this.workplace.MapPositionY
						);
				}
			}

			WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}
	}
}
