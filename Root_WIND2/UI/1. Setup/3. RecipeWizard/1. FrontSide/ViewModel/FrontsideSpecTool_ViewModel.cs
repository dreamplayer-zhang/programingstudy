using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2
{
    class FrontsideSpecTool_ViewModel: RootViewer_ViewModel
    {
        public FrontsideSpecTool_ViewModel()
        {
        }

        public void init(Setup_ViewModel setup, RecipeBase recipe)
        {
            base.init(GlobalObjects.Instance.GetNamed<ImageData>("FrontImage"), GlobalObjects.Instance.Get<DialogService>());
            p_VisibleMenu = System.Windows.Visibility.Visible;
        }

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
        }
        public override void SetRoiRect()
        {
            base.SetRoiRect();
        }
        public override void CanvasMovePoint_Ref(CPoint point, int nX, int nY)
        {
        }
    }
}
