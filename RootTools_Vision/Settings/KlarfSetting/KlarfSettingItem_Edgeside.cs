using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using RootTools;

namespace RootTools_Vision
{
	public class KlarfSettingItem_Edgeside : KlarfSettingItem
	{
		public enum FineBinSize
		{
			X,
			Y,
			LongAxis,
		}

		[Category("Finebin Number")]
		[DisplayName("Finebin Size")]
		public FineBinSize Size
		{
			get
			{
				return size;
			}
			set
			{
				size = value;
			}
		}
		private FineBinSize size = FineBinSize.LongAxis;

		[Category("Finebin Number")]
		[DisplayName("Specification Limit")]
		[XmlElement("SLList")]
		[Browsable(false)]
		public ObservableCollection<FinebinSpecificationLimit> SLList
		{
			get
			{
				return slList;
			}
			set
			{
				slList = value;
			}
		}
		private ObservableCollection<FinebinSpecificationLimit> slList = new ObservableCollection<FinebinSpecificationLimit>();
	}

	public class FinebinSpecificationLimit
	{
		[XmlAttribute]
		public int Number
		{
			get;
			set;
		}

		public int USL
		{
			get;
			set;
		}

		public int LSL
		{
			get;
			set;
		}

		public FinebinSpecificationLimit()
		{

		}

		public FinebinSpecificationLimit(int num, int usl, int lsl)
		{
			this.Number = num;
			this.USL = usl;
			this.LSL = lsl;
		}
	}

}
