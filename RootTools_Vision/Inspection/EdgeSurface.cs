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
			if (this.workplace.Index == 0)
				return base.DoPrework();

			return true;
		}

		public override void DoWork()
		{
			DoInspection();
			base.DoWork();
		}

		public void DoInspection()
		{
			//if (this.workplace.Index == 0)
			//	return;
						
			int roiHeight = parameter.RoiHeight;
			int roiWidth = parameter.RoiWidth; //this.workplace.SharedBufferWidth;
			int threshold = parameter.Theshold; // 12;
			int defectSize = parameter.Size; // 5;

			int roiSize = roiWidth * roiHeight;

			int left = this.workplace.PositionX;
			int top = this.workplace.PositionY;
			int right = this.workplace.PositionX + this.workplace.SharedBufferWidth; 
			int bottom = this.workplace.PositionY + roiHeight;

			byte[] arrSrc = new byte[roiSize];
			for (int cnt = top; cnt < bottom; cnt++)
			{
				Marshal.Copy(new IntPtr(this.workplace.SharedBufferR_GRAY.ToInt64() + (cnt * (Int64)roiWidth))
							, arrSrc
							, roiWidth * (cnt - top)
							, roiWidth);
			}

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
