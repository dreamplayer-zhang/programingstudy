using Root_AOP01_Inspection.Module;
using RootTools_Vision;
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
    /// RecipeEdge_Panel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class RecipeEdge_Panel : UserControl
    {
        public RecipeEdge_Panel()
        {
            InitializeComponent();
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideTopInspMgRegName).AddUIEvent += InspectionManager_AddDefectEvent;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideTopInspMgRegName).ClearDefectEvent += InspectionManager_ClearDefect;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideTopInspMgRegName).RefreshDefectEvent += InspectionManager_AOP_RefreshDefect;

			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideLeftInspMgRegName).AddUIEvent += InspectionManager_AddDefectEvent;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideLeftInspMgRegName).ClearDefectEvent += InspectionManager_ClearDefect;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideLeftInspMgRegName).RefreshDefectEvent += InspectionManager_AOP_RefreshDefect;

			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideRightInspMgRegName).AddUIEvent += InspectionManager_AddDefectEvent;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideRightInspMgRegName).ClearDefectEvent += InspectionManager_ClearDefect;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideRightInspMgRegName).RefreshDefectEvent += InspectionManager_AOP_RefreshDefect;

			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideBotInspMgRegName).AddUIEvent += InspectionManager_AddDefectEvent;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideBotInspMgRegName).ClearDefectEvent += InspectionManager_ClearDefect;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideBotInspMgRegName).RefreshDefectEvent += InspectionManager_AOP_RefreshDefect;
		}

		~RecipeEdge_Panel()
		{
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideTopInspMgRegName).AddUIEvent -= InspectionManager_AddDefectEvent;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideTopInspMgRegName).ClearDefectEvent -= InspectionManager_ClearDefect;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideTopInspMgRegName).RefreshDefectEvent -= InspectionManager_AOP_RefreshDefect;

			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideLeftInspMgRegName).AddUIEvent -= InspectionManager_AddDefectEvent;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideLeftInspMgRegName).ClearDefectEvent -= InspectionManager_ClearDefect;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideLeftInspMgRegName).RefreshDefectEvent -= InspectionManager_AOP_RefreshDefect;

			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideRightInspMgRegName).AddUIEvent -= InspectionManager_AddDefectEvent;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideRightInspMgRegName).ClearDefectEvent -= InspectionManager_ClearDefect;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideRightInspMgRegName).RefreshDefectEvent -= InspectionManager_AOP_RefreshDefect;

			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideBotInspMgRegName).AddUIEvent -= InspectionManager_AddDefectEvent;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideBotInspMgRegName).ClearDefectEvent -= InspectionManager_ClearDefect;
			GlobalObjects.Instance.GetNamed<InspectionManager_AOP>(App.SideBotInspMgRegName).RefreshDefectEvent -= InspectionManager_AOP_RefreshDefect;
		}

		private void InspectionManager_AOP_RefreshDefect()
		{
			//canvas.Dispatcher.Invoke(new Action(delegate ()
			//{
			//	canvas.RefreshDraw();
			//}));
		}

		private void InspectionManager_AddDefectEvent(List<RootTools.Database.Defect> items, Brush brush, Pen pen)
		{
			//foreach (var item in items)
			//{
			//	RootTools.Database.Defect defectInfo = item as RootTools.Database.Defect;
			//	var temp = new CRect((int)defectInfo.p_rtDefectBox.Left, (int)defectInfo.p_rtDefectBox.Top, (int)defectInfo.p_rtDefectBox.Right, (int)defectInfo.p_rtDefectBox.Bottom);
			//	canvas.AddBlock(item.m_fAbsX, item.m_fAbsY, temp.Width, temp.Height, brush, pen);//Defect fAbsX정보로
			//}
		}

		private void InspectionManager_ClearDefect()
		{
			try
			{
				//canvas.Dispatcher.Invoke(new Action(delegate ()
				//{
				//	canvas.ClearRect();
				//	canvas.RefreshDraw();
				//}));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
