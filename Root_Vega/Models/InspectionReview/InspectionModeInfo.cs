using RootTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_Vega.Models.InspectionReview
{
	class InspectionModeInfo : ObservableObject
	{

		public InspectionModeInfo(string name, int mode, bool isTarget)
		{
			this._Name = name;
			this._Mode = mode;
			this._IsTarget = isTarget;
		}

		#region Name
		string _Name;
		public string Name
		{
			get { return this._Name; }
			set
			{
				Debug.WriteLine(string.Format("Name : {0} => {1}", this._Name, value));
				this._Name = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region Mode
		int _Mode;
		public int Mode
		{
			get { return this._Mode; }
			set
			{
				Debug.WriteLine(string.Format("Mode : {0} => {1}", this._Mode, value));
				this._Mode = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion

		#region IsTarget
		bool _IsTarget;
		public bool IsTarget
		{
			get { return this._IsTarget; }
			set
			{
				Debug.WriteLine(string.Format("IsTarget : {0} => {1}", this._IsTarget, value));
				this._IsTarget = value;
				this.RaisePropertyChanged();
			}
		}
		#endregion
	}
}
