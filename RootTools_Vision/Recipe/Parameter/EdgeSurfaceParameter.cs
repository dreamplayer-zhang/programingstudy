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
		private int roiHeight = 1000;
		private int roiWidth = 3000;
		private int threshold = 0;
		private int size = 0;
		private int mergeDist = 0;
		#endregion

		#region [Getter/Setter]
		[Category("Parameter")]
		public int RoiHeight
		{
			get => this.roiHeight;
			set
			{
				SetProperty<int>(ref this.roiHeight, value);
			}
		}
		[Category("Parameter")]
		public int RoiWidth
		{
			get => this.roiWidth;
			set
			{
				SetProperty<int>(ref this.roiWidth, value);
			}
		}
		[Category("Parameter")]
		public int Theshold
		{
			get => this.threshold;
			set
			{
				SetProperty<int>(ref this.threshold, value);
			}
		}
		[Category("Parameter")]
		public int Size
		{
			get => this.size;
			set
			{
				SetProperty<int>(ref this.size, value);
			}
		}
		[Category("Parameter")]
		public int MergeDist
		{
			get => this.mergeDist;
			set
			{
				SetProperty<int>(ref this.mergeDist, value);
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

		public override object Clone()
		{
			return this.MemberwiseClone();
		}
	}
}
