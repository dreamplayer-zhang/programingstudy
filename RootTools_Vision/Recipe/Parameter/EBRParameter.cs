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
		private double stepDegree;
		private int xRange;
		private int diffEdge;
		private int diffBevel;
		private int diffEBR;
		private int offsetBevel;
		private int offsetEBR;
		#endregion

		#region [Getter/Setter]
		[Category("Parameter")]
		public int ROIWidth
		{
			get => this.roiWidth;
			set
			{
				SetProperty(ref this.roiWidth, value);
			}
		}
		[Category("Parameter")]
		public int ROIHeight
		{
			get => this.roiHeight;
			set
			{
				SetProperty(ref this.roiHeight, value);
			}
		}
		[Category("Parameter")]
		public int NotchY
		{
			get => this.notchY;
			set
			{
				SetProperty(ref this.notchY, value);
			}
		}
		[Category("Parameter")]
		public double StepDegree
		{
			get => this.stepDegree;
			set
			{
				SetProperty(ref this.stepDegree, value);
			}
		}
		[Category("Parameter")]
		public int XRange
		{
			get => this.xRange;
			set
			{
				SetProperty(ref this.xRange, value);
			}
		}
		[Category("Parameter")]
		public int DiffEdge
		{
			get => this.diffEdge;
			set
			{
				SetProperty(ref this.diffEdge, value);
			}
		}
		[Category("Parameter")]
		public int DiffBevel
		{
			get => this.diffBevel;
			set
			{
				SetProperty(ref this.diffBevel, value);
			}
		}
		[Category("Parameter")]
		public int DiffEBR
		{
			get => this.diffEBR;
			set
			{
				SetProperty(ref this.diffEBR, value);
			}
		}
		[Category("Parameter")]
		public int OffsetBevel
		{
			get => this.offsetBevel;
			set
			{
				SetProperty(ref this.offsetBevel, value);
			}
		}
		[Category("Parameter")]
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
