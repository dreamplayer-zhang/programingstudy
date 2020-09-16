using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;

namespace RootTools
{
    public class OriginTool_ViewModel :RootViewer_ViewModel
    {
        public event getMemPoint GetMemPoint;
        public delegate void getMemPoint();

        public OriginTool_ViewModel(ImageData image = null, IDialogService dialogService = null)
        {
            base.init(image, dialogService);
        }

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
            GetMemPoint();
        }
        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);
        }
    }
}
