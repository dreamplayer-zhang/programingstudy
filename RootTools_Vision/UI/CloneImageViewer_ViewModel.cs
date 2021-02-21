using RootTools;
using RootTools.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RootTools_Vision
{
    public class CloneImageViewer_ViewModel : RootViewer_ViewModel
    {
        public CloneImageViewer_ViewModel()
        {
            WorkEventManager.ReceivedMemoryID += ReceivedMemoryID_Callback;
        }

        public void ReceivedMemoryID_Callback(object obj, MemoryIDArgs args)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {

                MemoryID memoryID = args.MemoryID;

                MemoryPool pool = new MemoryPool(memoryID.Pool);
                ImageData imageData = new ImageData(pool.GetMemory(memoryID.Group, memoryID.Data));

                base.SetImageData(imageData);

            });

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
