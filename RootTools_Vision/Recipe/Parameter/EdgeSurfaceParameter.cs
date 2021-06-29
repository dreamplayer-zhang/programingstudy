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
		// inspection ROI
		private int startPosition = 0;
		private int endPosition = 10000;
		private int roiHeight = 1000;
		
		// parameter
		private int threshold = 10;
		private int defectSizeMin = 1;
		private int defectSizeMax = 100;
		
		// search edge
		private bool useEdgeSearch = true;
		private int edgeSearchLevel = 20;

		// option
		private bool chR = false;
		private bool chG = false;
		private bool chB = false;
		#endregion

		#region [Property]
		[Category("Option")]
		[DisplayName("R-Channel")]
		public bool ChR
		{
			get => this.chR;
			set => SetProperty(ref chR, value);
		}
		[Category("Option")]
		[DisplayName("G-Channel")]
		public bool ChG
		{
			get => this.chG;
			set => SetProperty(ref chG, value);
		}
		[Category("Option")]
		[DisplayName("B-Channel")]
		public bool ChB
		{
			get => this.chB;
			set => SetProperty(ref chB, value);
		}

		[Category("Parameter")]
		public int Threshold
		{
			get => this.threshold;
			set => SetProperty(ref threshold, value);
		}
		[Category("Parameter")]
		[DisplayName("Size Min")]
		public int DefectSizeMin
		{
			get => this.defectSizeMin;
			set => SetProperty(ref defectSizeMin, value);
		}
		[Category("Parameter")]
		[DisplayName("Size Max")]
		public int DefectSizeMax
		{
			get => this.defectSizeMax;
			set => SetProperty(ref defectSizeMax, value);
		}

		[Category("ROI")]
		[DisplayName("Start Y Position")]
		public int StartPosition
		{
			get => this.startPosition;
			set => SetProperty(ref startPosition, value);
		}
		[Category("ROI")]
		[DisplayName("End Y Position")]
		public int EndPosition
		{
			get => this.endPosition;
			set => SetProperty(ref endPosition, value);
		}
		[Category("ROI")]
		[DisplayName("Step Height")]
		public int ROIHeight
		{
			get => this.roiHeight;
			set => SetProperty(ref roiHeight, value);
		}

		[Browsable(false)]
		[Category("Search Edge Line")]
		[DisplayName("Use")]
		public bool UseEdgeSearch
		{
			get => this.useEdgeSearch;
			set => SetProperty(ref useEdgeSearch, value);
		}
		[Category("Search Edge Line")]
		[DisplayName("Level(%)")]
		public int EdgeSearchLevel
		{
			get => this.edgeSearchLevel;
			set => SetProperty(ref edgeSearchLevel, value);
		}
		#endregion
	}
}
