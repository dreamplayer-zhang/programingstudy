using ATI;
using Root_Vega.Controls;
using Root_Vega.Models.InspectionReview;
using RootTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Root_Vega
{
	class Dialog_InspResultViewModel : InspResultViewModel
	{
		public Dialog_InspResultViewModel(string dataPath, DateTime InspTime) : base(dataPath, InspTime)
		{
		}

		public delegate void ModelessEventHandelr();
		public event ModelessEventHandelr CloseWindow;

		public void OnCancelButton()
		{
			//CloseRequested(this, new DialogCloseRequestedEventArgs(false));
			if (CloseWindow != null)
				CloseWindow();

		}

		#region Commands
		public RelayCommand CancelCommand
		{
			get
			{
				return new RelayCommand(OnCancelButton);
			}
		}
		#endregion
	}
}
