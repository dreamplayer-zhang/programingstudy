using RootTools;
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
			edgeParamBaseTop = new EdgeSurfaceParameterBase();
			edgeParamBaseSide = new EdgeSurfaceParameterBase();
			edgeParamBaseBtm = new EdgeSurfaceParameterBase();
		}

		private EdgeSurfaceParameterBase edgeParamBaseTop;
		private EdgeSurfaceParameterBase edgeParamBaseSide;
		private EdgeSurfaceParameterBase edgeParamBaseBtm;

		#region [Getter/Setter]
		[Category("Parameter")]
		public EdgeSurfaceParameterBase EdgeParamBaseTop
		{
			get => this.edgeParamBaseTop;
			set
			{
				SetProperty(ref edgeParamBaseTop, value);
			}
		}
		[Category("Parameter")]
		public EdgeSurfaceParameterBase EdgeParamBaseSide
		{
			get => this.edgeParamBaseSide;
			set
			{
				SetProperty(ref edgeParamBaseSide, value);
			}
		}
		[Category("Parameter")]
		public EdgeSurfaceParameterBase EdgeParamBaseBtm
		{
			get => this.edgeParamBaseBtm;
			set
			{
				SetProperty(ref edgeParamBaseBtm, value);
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
	
	public class EdgeSurfaceParameterBase : ObservableObject
	{
		#region [Parameter]
		private int roiHeight = 0;
		private int roiWidth = 0;
		private int threshold = 0;
		private int defectSizeMin = 0;
		private int mergeDist = 0;
		private int illumWhite = 0;
		private int illumSide = 0;
		private int edgeSearchLevel = 20;
		#endregion

		#region [Getter/Setter]
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
		public int ROIWidth
		{
			get => this.roiWidth;
			set
			{
				SetProperty<int>(ref this.roiWidth, value);
			}
		}
		[Category("Parameter")]
		public int Threshold
		{
			get => this.threshold;
			set
			{
				SetProperty<int>(ref this.threshold, value);
			}
		}
		[Category("Parameter")]
		public int DefectSizeMin
		{
			get => this.defectSizeMin;
			set
			{
				SetProperty<int>(ref this.defectSizeMin, value);
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
		[Category("Parameter")]
		public int IllumWhite
		{
			get => this.illumWhite;
			set
			{
				SetProperty<int>(ref this.illumWhite, value);
			}
		}
		[Category("Parameter")]
		public int IllumSide
		{
			get => this.illumSide;
			set
			{
				SetProperty<int>(ref this.illumSide, value);
			}
		}
		[Category("Parameter")]
		public int EdgeSearchLevel
		{
			get => this.edgeSearchLevel;
			set
			{
				SetProperty<int>(ref this.edgeSearchLevel, value);
			}
		}
		#endregion
	}
}
