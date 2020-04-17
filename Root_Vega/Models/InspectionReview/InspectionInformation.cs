using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Vega.Models.InspectionReview
{
	class InspectionInformation
	{
		public InspectionInformation()
		{

		}
		public InspectionInformation(InspectionInformation data)
		{
			this.Name = data.Name;
			this.Context = data.Context;
		}
		public string Name { get; set; }
		public string Context { get; set; }
	}
}
