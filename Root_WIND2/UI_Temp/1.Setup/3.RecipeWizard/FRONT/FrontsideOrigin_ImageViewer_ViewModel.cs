using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2.UI_Temp
{
    public enum FRONT_ORIGIN_VIEWER_STATE
    {
        Normal,
        Origin,
        Pitch,
        Rular,
    }

    class FrontsideOrigin_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        public FrontsideOrigin_ImageViewer_ViewModel()
        {
            p_VisibleMenu = System.Windows.Visibility.Collapsed;
        }

        public FRONT_ORIGIN_VIEWER_STATE ViewerState
        {
            get;
            set;
        }

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
            //if (m_KeyEvent != null)
            //    if (m_KeyEvent.Key == Key.LeftShift && m_KeyEvent.IsDown)
            //        return;
            //CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            //CPoint MemPt = GetMemPoint(CanvasPt);
            //switch (eBoxProcess)
            //{
            //    case BoxProcess.None:
            //        //마우스가 rect밖에있으면 선택취소
            //        // 그다음 다시그리기
            //        // 안에있으면 선택
            //        if (p_Cursor != Cursors.Arrow)
            //        {
            //            PointBuffer = MemPt;
            //            string cursor = p_Cursor.ToString();
            //            eBoxProcess = BoxProcess.Modifying;
            //            break;
            //        }
            //        else
            //        {
            //            if (BOX != null)
            //            {
            //                if (p_ViewElement.Contains(BOX.UIElement))
            //                {
            //                    p_ViewElement.Remove(BOX.UIElement);
            //                    p_ViewElement.Remove(BOX.ModifyTool);
            //                }
            //            }
            //            BOX = StartDraw(BOX, MemPt);
            //            p_ViewElement.Add(BOX.UIElement);
            //            eBoxProcess = BoxProcess.Drawing;
            //        }
            //        break;
            //    case BoxProcess.Drawing:
            //        BOX = DrawDone(BOX);

            //        BoxDone(BOX);
            //        eBoxProcess = BoxProcess.None;
            //        break;
            //    case BoxProcess.Modifying:
            //        break;
            //}
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);

            //CPoint CanvasPt = new CPoint(p_MouseX, p_MouseY);
            //CPoint MemPt = GetMemPoint(CanvasPt);

            //switch (eBoxProcess)
            //{
            //    case BoxProcess.None:
            //        break;
            //    case BoxProcess.Drawing:
            //        BOX = Drawing(BOX, MemPt);
            //        break;
            //    case BoxProcess.Modifying:
            //        {
            //            if (e.LeftButton == MouseButtonState.Pressed)
            //                BOX = ModifyRect(BOX, MemPt);

            //        }
            //        break;
            //}
        }
        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            //switch (eBoxProcess)
            //{
            //    case BoxProcess.None:
            //        break;
            //    case BoxProcess.Drawing:
            //        break;
            //    case BoxProcess.Modifying:
            //        {
            //            if (BOX.isSelected)
            //            {
            //                MakeModifyTool(BOX);
            //                BOX.ModifyTool.Visibility = Visibility.Visible;
            //            }
            //            BoxDone(BOX);
            //            eBoxProcess = BoxProcess.None;
            //            eModifyType = ModifyType.None;
            //        }
            //        break;
            //}
        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
            //RedrawShapes();
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
            base.CanvasMovePoint_Ref(point, nX, nY);
            //RedrawShapes();
        }
    }
}
