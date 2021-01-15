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
		private int xRange;
		private int edgeDiff;
		private int bevelDiff;
		private int ebrDiff;
		#endregion

		#region [Getter/Setter]
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
		public int EdgeDiff
		{
			get => this.edgeDiff;
			set
			{
				SetProperty<int>(ref this.edgeDiff, value);
			}
		}
		[Category("Parameter")]
		public int BevelDiff
		{
			get => this.bevelDiff;
			set
			{
				SetProperty<int>(ref this.bevelDiff, value);
			}
		}
		[Category("Parameter")]
		public int EBRDiff
		{
			get => this.ebrDiff;
			set
			{
				SetProperty<int>(ref this.ebrDiff, value);
			}
		}

		[Browsable(false)]
		public int MaskIndex 
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
