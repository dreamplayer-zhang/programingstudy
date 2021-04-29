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
		private int startPosition = 0;
		private int roiHeight = 1000;
		private int threshold = 10;
		private int defectSizeMin = 1;
		private int defectSizeMax = 10000;
		private int mergeDist = 1;
		private int edgeSearchLevel = 20;

		private bool chR = false;
		private bool chG = false;
		private bool chB = false;
		#endregion

		#region [Getter/Setter]
		[Category("Parameter")]
		public int StartPosition
		{
			get => this.startPosition;
			set => SetProperty(ref startPosition, value);
		}
		[Category("Parameter")]
		public int ROIHeight
		{
			get => this.roiHeight;
			set => SetProperty(ref roiHeight, value);
		}
		[Category("Parameter")]
		public int Threshold
		{
			get => this.threshold;
			set => SetProperty(ref threshold, value);
		}
		[Category("Parameter")]
		public int DefectSizeMin
		{
			get => this.defectSizeMin;
			set => SetProperty(ref defectSizeMin, value);
		}
		[Category("Parameter")]
		public int DefectSizeMax
		{
			get => this.defectSizeMax;
			set => SetProperty(ref defectSizeMax, value);
		}
		[Category("Parameter")]
		public int MergeDist
		{
			get => this.mergeDist;
			set => SetProperty(ref mergeDist, value);
		}
		[Category("Parameter")]
		public int EdgeSearchLevel
		{
			get => this.edgeSearchLevel;
			set => SetProperty(ref edgeSearchLevel, value);
		}
		[Category("Parameter")]
		public bool ChR
		{
			get => this.chR;
			set => SetProperty(ref chR, value);
		}
		[Category("Parameter")]
		public bool ChG
		{
			get => this.chG;
			set => SetProperty(ref chG, value);
		}
		[Category("Parameter")]
		public bool ChB
		{
			get => this.chB;
			set => SetProperty(ref chB, value);
		}
		#endregion
	}
}
