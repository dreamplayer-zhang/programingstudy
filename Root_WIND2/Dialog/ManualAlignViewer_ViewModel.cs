using Root_EFEM;
using Root_WIND2.Module;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_WIND2
{
    public class ManualAlignViewer_ViewModel : ObservableObject, IDialogRequestClose
    {
        private ManualAlignViewer_ImageViewer_ViewModel imageViewerVM;

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        public ManualAlignViewer_ImageViewer_ViewModel ImageViewerVM
        {
            get => this.imageViewerVM;
            set
            {
                SetProperty(ref this.imageViewerVM, value);
            }
        }

        public ManualAlignViewer_ViewModel(ImageData imageData)
        {
            Vision vision = GlobalObjects.Instance.Get<WIND2_Engineer>().m_handler.p_Vision;
            this.ImageViewerVM = new ManualAlignViewer_ImageViewer_ViewModel(imageData, vision.AxisRotate);
        }
    }
}
