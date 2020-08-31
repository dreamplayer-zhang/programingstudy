using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RootTools
{
    public class crosstest : ImageToolViewer_VM
    {
        public crosstest(ImageData image = null, IDialogService dialogService = null) : base(image, dialogService)
        {
        }
        public override void CanvasPreviewMouseDown(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Console.WriteLine("test");
            base.CanvasPreviewMouseDown(sender, e);
        }
    }
}
