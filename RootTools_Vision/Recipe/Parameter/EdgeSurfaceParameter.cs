using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
	public class EdgeSurfaceParameter : ParameterBase, IMaskInspection, IColorInspection
	{
		public EdgeSurfaceParameter() : base(typeof(EdgeSurface))
		{

		}

		#region [Parameter]
		private int roiHeightTop = 0;
		private int roiWidthTop = 0;
		private int thresholdTop = 0;
		private int sizeMinTop = 0;
		private int mergeDistTop = 0;
		private int illumWhiteTop = 0;
		private int illumSideTop = 0;

		private int roiHeightSide = 0;
		private int roiWidthSide = 0;
		private int thresholdSide = 0;
		private int sizeMinSide = 0;
		private int mergeDistSide = 0;
		private int illumWhiteSide = 0;
		private int illumSideSide = 0;

		private int roiHeightBtm = 0;
		private int roiWidthBtm = 0;
		private int thresholdBtm = 0;
		private int sizeMinBtm = 0;
		private int mergeDistBtm = 0;
		private int illumWhiteBtm = 0;
		private int illumSideBtm = 0;
		#endregion

		#region [Getter/Setter]
		[Category("Parameter")]
		public int RoiHeightTop
		{
			get => this.roiHeightTop;
			set
			{
				SetProperty<int>(ref this.roiHeightTop, value);
			}
		}
		[Category("Parameter")]
		public int RoiWidthTop
		{
			get => this.roiWidthTop;
			set
			{
				SetProperty<int>(ref this.roiWidthTop, value);
			}
		}
		[Category("Parameter")]
		public int ThesholdTop
		{
			get => this.thresholdTop;
			set
			{
				SetProperty<int>(ref this.thresholdTop, value);
			}
		}
		[Category("Parameter")]
		public int SizeMinTop
		{
			get => this.sizeMinTop;
			set
			{
				SetProperty<int>(ref this.sizeMinTop, value);
			}
		}
		[Category("Parameter")]
		public int MergeDistTop
		{
			get => this.mergeDistTop;
			set
			{
				SetProperty<int>(ref this.mergeDistTop, value);
			}
		}
		[Category("Parameter")]
		public int IllumWhiteTop
		{
			get => this.illumWhiteTop;
			set
			{
				SetProperty<int>(ref this.illumWhiteTop, value);
			}
		}
		[Category("Parameter")]
		public int IllumSideTop
		{
			get => this.illumSideTop;
			set
			{
				SetProperty<int>(ref this.illumSideTop, value);
			}
		}

		[Category("Parameter")]
		public int RoiHeightSide
		{
			get => this.roiHeightSide;
			set
			{
				SetProperty<int>(ref this.roiHeightSide, value);
			}
		}
		[Category("Parameter")]
		public int RoiWidthSide
		{
			get => this.roiWidthSide;
			set
			{
				SetProperty<int>(ref this.roiWidthSide, value);
			}
		}
		[Category("Parameter")]
		public int ThesholdSide
		{
			get => this.thresholdSide;
			set
			{
				SetProperty<int>(ref this.thresholdSide, value);
			}
		}
		[Category("Parameter")]
		public int SizeMinSide
		{
			get => this.sizeMinSide;
			set
			{
				SetProperty<int>(ref this.sizeMinSide, value);
			}
		}
		[Category("Parameter")]
		public int MergeDistSide
		{
			get => this.mergeDistSide;
			set
			{
				SetProperty<int>(ref this.mergeDistSide, value);
			}
		}
		[Category("Parameter")]
		public int IllumWhiteSide
		{
			get => this.illumWhiteSide;
			set
			{
				SetProperty<int>(ref this.illumWhiteSide, value);
			}
		}
		[Category("Parameter")]
		public int IllumSideSide
		{
			get => this.illumSideSide;
			set
			{
				SetProperty<int>(ref this.illumSideSide, value);
			}
		}

		[Category("Parameter")]
		public int RoiHeightBtm
		{
			get => this.roiHeightBtm;
			set
			{
				SetProperty<int>(ref this.roiHeightBtm, value);
			}
		}
		[Category("Parameter")]
		public int RoiWidthBtm
		{
			get => this.roiWidthBtm;
			set
			{
				SetProperty<int>(ref this.roiWidthBtm, value);
			}
		}
		[Category("Parameter")]
		public int ThesholdBtm
		{
			get => this.thresholdBtm;
			set
			{
				SetProperty<int>(ref this.thresholdBtm, value);
			}
		}
		[Category("Parameter")]
		public int SizeMinBtm
		{
			get => this.sizeMinBtm;
			set
			{
				SetProperty<int>(ref this.sizeMinBtm, value);
			}
		}
		[Category("Parameter")]
		public int MergeDistBtm
		{
			get => this.mergeDistBtm;
			set
			{
				SetProperty<int>(ref this.mergeDistBtm, value);
			}
		}
		[Category("Parameter")]
		public int IllumWhiteBtm
		{
			get => this.illumWhiteBtm;
			set
			{
				SetProperty<int>(ref this.illumWhiteBtm, value);
			}
		}
		[Category("Parameter")]
		public int IllumSideBtm
		{
			get => this.illumSideBtm;
			set
			{
				SetProperty<int>(ref this.illumSideBtm, value);
			}
		}

		[Browsable(false)]
		public int MaskIndex
		{
			get;
			set;
		}

		[Browsable(false)]
		public IMAGE_CHANNEL IndexChannel
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
