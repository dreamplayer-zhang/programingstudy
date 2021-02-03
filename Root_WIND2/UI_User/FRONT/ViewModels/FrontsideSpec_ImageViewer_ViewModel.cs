using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    public class FrontsideSpec_ImageViewer_ViewModel : RootViewer_ViewModel
    {
        public FrontsideSpec_ImageViewer_ViewModel()
        {
            this.p_VisibleMenu = System.Windows.Visibility.Collapsed;
        }

        public void SetViewRect()
        {
            OriginRecipe originRecipe = GlobalObjects.Instance.Get<RecipeFront>().GetItem<OriginRecipe>();

            int left = originRecipe.OriginX;
            int bottom = originRecipe.OriginY;
            int right = originRecipe.OriginX + originRecipe.OriginWidth;
            int top = originRecipe.OriginY - originRecipe.OriginHeight;

            int width = originRecipe.OriginWidth;
            int height = originRecipe.OriginHeight;

            double full_ratio = 1;
            double ratio = 1;

            if (this.p_CanvasHeight > this.p_CanvasWidth)
            {
                full_ratio = full_ratio = (double)this.p_ImageData.p_Size.Y / (double)this.p_CanvasHeight;
            }
            else
            {
                full_ratio = full_ratio = (double)this.p_ImageData.p_Size.X / (double)this.p_CanvasWidth;
            }


            double canvas_w_h_ratio = (double)(this.p_CanvasHeight) / (double)(p_CanvasWidth); // 가로가 더 길 경우 1 이하
            double box_w_h_ratio = (double)height / (double)width;

            if (box_w_h_ratio > canvas_w_h_ratio) // Canvas보다 가로 비율이 더 높을 경우,  box의 세로에 맞춰야함.
            {
                ratio = (double)height / (double)this.p_CanvasHeight;
            }
            else
            {
                ratio = (double)width / (double)this.p_CanvasWidth;
            }

            this.p_Zoom = ratio / full_ratio;

            this.p_View_Rect = new System.Drawing.Rectangle(new System.Drawing.Point(left, top), new System.Drawing.Size(width, height));

            this.SetImageSource();
        }


        #region [Overrides]
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            e.Handled = true;
        }

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }

        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            e.Handled = true;
        }
        #endregion
    }
}
