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
		// roi
		private int roiHeight = 500;
		private int notchY;

		// option
		private int measureCount = 30;
		private double notchOffsetDegree = 5;
		private int xRange = 20;

		// diff GV
		private int diffEdge;
		private int diffBevel;
		private int diffEBR;

		// 
		private int offsetBevel;
		private int offsetEBR;
		#endregion

		#region [Getter/Setter]
		[Category("ROI")]
		[DisplayName("Height")]
		public int ROIHeight
		{
			get => this.roiHeight;
			set
			{
				SetProperty(ref this.roiHeight, value);
			}
		}
		[Browsable(false)]
		[Category("ROI")]
		[DisplayName("Notch")]
		public int NotchY
		{
			get => this.notchY;
			set
			{
				SetProperty(ref this.notchY, value);
			}
		}

		[Category("Option")]
		[DisplayName("Measurement Count")]
		public int MeasureCount
		{
			get => this.measureCount;
			set
			{
				SetProperty(ref this.measureCount, value);
			}
		}
		[Category("Option")]
		[DisplayName("Notch Offset Degree")]
		public double NotchOffsetDegree
		{
			get => this.notchOffsetDegree;
			set
			{
				SetProperty(ref this.notchOffsetDegree, value);
			}
		}
		[Category("Option")]
		[DisplayName("X Range")]
		public int XRange
		{
			get => this.xRange;
			set
			{
				SetProperty(ref this.xRange, value);
			}
		}

		[Category("Difference GV")]
		[DisplayName("1. Wafer Edge")]
		public int DiffEdge
		{
			get => this.diffEdge;
			set
			{
				SetProperty(ref this.diffEdge, value);
			}
		}
		[Category("Difference GV")]
		[DisplayName("2. Edge-Bevel")]
		public int DiffBevel
		{
			get => this.diffBevel;
			set
			{
				SetProperty(ref this.diffBevel, value);
			}
		}
		[Category("Difference GV")]
		[DisplayName("3. Bevel-EBR")]
		public int DiffEBR
		{
			get => this.diffEBR;
			set
			{
				SetProperty(ref this.diffEBR, value);
			}
		}

		[Category("Offset")]
		[DisplayName("1. Edge-Bevel")]
		public int OffsetBevel
		{
			get => this.offsetBevel;
			set
			{
				SetProperty(ref this.offsetBevel, value);
			}
		}
		[Category("Offset")]
		[DisplayName("2. Bevel-EBR")]
		public int OffsetEBR
		{
			get => this.offsetEBR;
			set
			{
				SetProperty(ref this.offsetEBR, value);
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
