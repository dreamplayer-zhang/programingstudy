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

namespace RootTools_Vision
{
	public class EdgeSurface : WorkBase
	{
		WorkplaceBundle workplaceBundle;
		Workplace workplace;

		public override WORK_TYPE Type => WORK_TYPE.INSPECTION;

		private EdgeSurfaceParameter parameter;
		private EdgeSurfaceRecipe recipe;

		public EdgeSurface() : base()
		{
			m_sName = this.GetType().Name;
		}

		public override WorkBase Clone()
		{
			return (WorkBase)this.MemberwiseClone();
		}

		public override void SetRecipe(Recipe _recipe)
		{
			this.parameter = _recipe.GetRecipe<EdgeSurfaceParameter>();
			this.recipe = _recipe.GetRecipe<EdgeSurfaceRecipe>();
		}

		public override void SetWorkplace(Workplace _workplace)
		{
			this.workplace = _workplace;
		}

		public override void SetWorkplaceBundle(WorkplaceBundle _workplace)
		{
			this.workplaceBundle = _workplace;
		}

		public override bool DoPrework()
		{
			return base.DoPrework();
		}

		public override void DoWork()
		{
			DoInspection();
			//base.DoWork();
		}

		public void DoInspection()
		{
			if (this.workplace.Index == 0)
				return;

			DoColorInspection(this.workplace.SharedBufferR_GRAY, parameter);
			DoColorInspection(this.workplace.SharedBufferG, parameter);
			DoColorInspection(this.workplace.SharedBufferB, parameter);

			WorkEventManager.OnInspectionDone(this.workplace, new InspectionDoneEventArgs(new List<CRect>())); // 나중에 ProcessDefect쪽 EVENT로...
		}

		private void DoColorInspection(IntPtr sharedBuffer, EdgeSurfaceParameter param)
		{
			int roiHeight = 2000; //parameter.RoiHeight;
			int roiWidth = this.workplace.SharedBufferWidth; //parameter.RoiWidth;
			int threshold = 40; //parameter.Theshold;
			int defectSize = 10; //parameter.Size;

			int roiSize = roiWidth * roiHeight;

			int left = this.workplace.PositionX;
			int top = this.workplace.PositionY;
			int right = this.workplace.PositionX + this.workplace.SharedBufferWidth;
			int bottom = this.workplace.PositionY + roiHeight;

			byte[] arrSrc = new byte[roiSize];
			for (int cnt = top; cnt < bottom; cnt++)
			{
				Marshal.Copy(new IntPtr(sharedBuffer.ToInt64() + (cnt * (Int64)roiWidth))
							, arrSrc
							, roiWidth * (cnt - top)
							, roiWidth);
			}
			//Emgu.CV.Mat mat = new Emgu.CV.Mat((int)roiHeight, (int)roiWidth, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
			//Marshal.Copy(arrSrc, 0, mat.DataPointer, arrSrc.Length);
			//mat.Save(@"D:/" + sharedBuffer.ToInt64().ToString() + "_" + this.workplace.Index.ToString() + ".bmp");

			// profile 생성
			List<int> temp = new List<int>();
			List<int> profile = new List<int>();
			for (long j = 0; j < roiWidth; j++)
			{
				temp.Clear();
				for (long i = 0; i < roiSize; i += roiWidth)
				{
					temp.Add(arrSrc[j + i]);
				}
				temp.Sort();
				profile.Add(temp[temp.Count / 2]);  // 중앙값
			}

			// Calculate diff image (original - profile)
			byte[] diff = new byte[roiSize];
			for (int j = 0; j < roiHeight; j++)
			{
				for (int i = 0; i < roiWidth; i++)
				{
					diff[(j * roiWidth) + i] = (byte)(Math.Abs(arrSrc[(j * roiWidth) + i] - profile[i]));
				}
			}

			// Threshold
			byte[] thresh = new byte[roiSize];
			CLR_IP.Cpp_Threshold(diff, thresh, roiWidth, roiHeight, false, threshold);
			var label = CLR_IP.Cpp_Labeling(diff, thresh, roiWidth, roiHeight, true);
			//Emgu.CV.Mat mat2 = new Emgu.CV.Mat((int)roiHeight, (int)roiWidth, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
			//Marshal.Copy(thresh, 0, mat2.DataPointer, arrSrc.Length);
			//mat2.Save(@"D:/" + sharedBuffer.ToInt64().ToString() + "_thresh_" + this.workplace.Index.ToString() + ".bmp");

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
						this.workplace.PositionY + label[i].boundTop,
						Math.Abs(label[i].boundRight - label[i].boundLeft),
						Math.Abs(label[i].boundBottom - label[i].boundTop),
						this.workplace.MapPositionX,
						this.workplace.MapPositionY
						);
				}
			}
		}
	}
}
