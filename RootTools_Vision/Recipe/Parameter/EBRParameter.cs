using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class EBRParameter : ParameterBase, IMaskInspection
	{
		public EBRParameter() : base(typeof(EBR))
		{

		}

		#region [Parameter]
		private int roiWidth;
		private int roiHeight;
		private int notchY;
		private int stepDegree;
		private int xRange;
		private int diffEdge;
		private int diffBevel;
		private int diffEBR;
		private int offsetBevel;
		private int offsetEBR;

		// 카메라 정보
		private double camResolution = 0;
		#endregion

		#region [Getter/Setter]
		[Category("Parameter")]
		public int ROIWidth
		{
			get => this.roiWidth;
			set
			{
				SetProperty<int>(ref this.roiWidth, value);
			}
		}
		[Category("Parameter")]
		public int ROIHeight
		{
			get => this.roiHeight;
			set
			{
				SetProperty<int>(ref this.roiHeight, value);
			}
		}
		[Category("Parameter")]
		public int NotchY
		{
			get => this.notchY;
			set
			{
				SetProperty<int>(ref this.notchY, value);
			}
		}
		[Category("Parameter")]
		public int StepDegree
		{
			get => this.stepDegree;
			set
			{
				SetProperty<int>(ref this.stepDegree, value);
			}
		}
		[Category("Parameter")]
		public int XRange
		{
			get => this.xRange;
			set
			{
				SetProperty<int>(ref this.xRange, value);
			}
		}
		[Category("Parameter")]
		public int DiffEdge
		{
			get => this.diffEdge;
			set
			{
				SetProperty<int>(ref this.diffEdge, value);
			}
		}
		[Category("Parameter")]
		public int DiffBevel
		{
			get => this.diffBevel;
			set
			{
				SetProperty<int>(ref this.diffBevel, value);
			}
		}
		[Category("Parameter")]
		public int DiffEBR
		{
			get => this.diffEBR;
			set
			{
				SetProperty<int>(ref this.diffEBR, value);
			}
		}
		[Category("Parameter")]
		public int OffsetBevel
		{
			get => this.offsetBevel;
			set
			{
				SetProperty<int>(ref this.offsetBevel, value);
			}
		}
		[Category("Parameter")]
		public int OffsetEBR
		{
			get => this.offsetEBR;
			set
			{
				SetProperty<int>(ref this.offsetEBR, value);
			}
		}
		[Category("Parameter")]
		public double CamResolution
		{
			get => this.camResolution;
			set
			{
				SetProperty<double>(ref this.camResolution, value);
			}
		}

		[Browsable(false)]
		public int MaskIndex 
		{ 
			get; 
			set; 
		}
		#endregion


		//public override object Clone()
		//{
		//	return this.MemberwiseClone();
		//}
	}
}
