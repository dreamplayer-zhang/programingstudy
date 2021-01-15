using Root_AOP01_Inspection.Module;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Root_AOP01_Inspection
{
	/// <summary>
	/// Recipe45D_Panel.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class Recipe45D_Panel : UserControl
	{
		public Recipe45D_Panel()
		{
			InitializeComponent();
			//AddDefectEvent
			ProgramManager.Instance.InspectionManager.AddDefectEvent += InspectionManager_AddDefectEvent;
			ProgramManager.Instance.InspectionManager.ClearDefectEvent += InspectionManager_ClearDefect;
			ProgramManager.Instance.InspectionManager.RefreshDefectEvent += InspectionManager_AOP_RefreshDefect;
		}
		~Recipe45D_Panel()
		{
			ProgramManager.Instance.InspectionManager.AddDefectEvent -= InspectionManager_AddDefectEvent;
			ProgramManager.Instance.InspectionManager.ClearDefectEvent -= InspectionManager_ClearDefect;
			ProgramManager.Instance.InspectionManager.RefreshDefectEvent -= InspectionManager_AOP_RefreshDefect;
		}

		private void InspectionManager_AOP_RefreshDefect()
		{
			canvas.Dispatcher.Invoke(new Action(delegate ()
			{
				canvas.RefreshDraw();
			}));
		}

		private void InspectionManager_AddDefectEvent(List<RootTools.Database.Defect> items)
		{
			foreach (var item in items)
			{
				RootTools.Database.Defect defectInfo = item as RootTools.Database.Defect;
				var temp = new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom);
				canvas.AddBlock(temp.Left, temp.Top, temp.Width, temp.Height, null, new Pen(Brushes.Red, 2));
			}
		}

		private void InspectionManager_ClearDefect()
		{
			try
			{
				canvas.Dispatcher.Invoke(new Action(delegate ()
				{
					canvas.ClearRect();
					canvas.RefreshDraw();
				}));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
