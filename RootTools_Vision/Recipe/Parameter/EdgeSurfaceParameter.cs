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
		private int defectSizeMinX = 1;
		private int defectSizeMaxX = 100;
		private int defectSizeMinY = 1;
		private int defectSizeMaxY = 100;

		// search edge
		private bool useEdgeSearch = true;
		private int edgeSearchLevel = 20;

		// option
		private bool chR = false;
		private bool chG = false;
		private bool chB = false;
		private double notchOffsetDegree = 5;

		// probability method
		private bool useProbabilityMethod = false;
		private double proportionThreshold = 20;
		private double proportionLimit = 20;
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
		[DisplayName("1. Threshold")]
		public int Threshold
		{
			get => this.threshold;
			set => SetProperty(ref threshold, value);
		}
		[Category("Parameter")]
		[DisplayName("2. Proportion Threshold")]
		public double ProportionThreshold
		{
			get => this.proportionThreshold;
			set => SetProperty(ref proportionThreshold, value);
		}
		[Category("Parameter")]
		[DisplayName("3. Proportion Limit")]
		public double ProportionLimit
		{
			get => this.proportionLimit;
			set => SetProperty(ref proportionLimit, value);
		}
		[Category("Parameter")]
		[DisplayName("4. Width Size Min")]
		public int DefectSizeMinX
		{
			get => this.defectSizeMinX;
			set => SetProperty(ref defectSizeMinX, value);
		}
		[Category("Parameter")]
		[DisplayName("5. Width Size Max")]
		public int DefectSizeMaxX
		{
			get => this.defectSizeMaxX;
			set => SetProperty(ref defectSizeMaxX, value);
		}

		[Category("Parameter")]
		[DisplayName("6. Height Size Min")]
		public int DefectSizeMinY
		{
			get => this.defectSizeMinY;
			set => SetProperty(ref defectSizeMinY, value);
		}
		[Category("Parameter")]
		[DisplayName("7. Height Size Max")]
		public int DefectSizeMaxY
		{
			get => this.defectSizeMaxY;
			set => SetProperty(ref defectSizeMaxY, value);
		}

		[Category("ROI")]
		[DisplayName("1. Start Y Position")]
		public int StartPosition
		{
			get => this.startPosition;
			set => SetProperty(ref startPosition, value);
		}
		[Category("ROI")]
		[DisplayName("2. End Y Position")]
		public int EndPosition
		{
			get => this.endPosition;
			set => SetProperty(ref endPosition, value);
		}
		[Category("ROI")]
		[DisplayName("3. Step Height")]
		public int ROIHeight
		{
			get => this.roiHeight;
			set => SetProperty(ref roiHeight, value);
		}
		[Category("ROI")]
		[DisplayName("4. Notch Offset Degree")]
		public double NotchOffsetDegree
		{
			get => this.notchOffsetDegree;
			set
			{
				SetProperty(ref this.notchOffsetDegree, value);
			}
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

		[Category("Method")]
		[DisplayName("Use Probability Method")]
		public bool UseProbabilityMethod
		{
			get => this.useProbabilityMethod;
			set => SetProperty(ref useProbabilityMethod, value);
		}
		#endregion
	}
}
