using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RootTools_Vision
{
    public class CloneImageViewer_ViewModel : RootViewer_ViewModel
    {

        private string memoryFrontPool = "Vision.Memory";
        private string memoryFrontGroup = "Vision";
        private string memoryFront = "Main";

        

        public CloneImageViewer_ViewModel()
        {
            MemoryPool pool = new MemoryPool(memoryFrontPool);
            ImageData imageData = new ImageData(pool.GetMemory(memoryFrontGroup, memoryFront));

            base.init(imageData);
        }

        public override void MouseMove(object sender, MouseEventArgs e)
        {
            base.MouseMove(sender, e);
        }

        public override void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            base.MouseWheel(sender, e);
        }

        public override void PreviewMouseDown(object sender, MouseEventArgs e)
        {
            base.PreviewMouseDown(sender, e);
        }

        public override void PreviewMouseUp(object sender, MouseEventArgs e)
        {
            base.PreviewMouseUp(sender, e);
        }
    }
}
