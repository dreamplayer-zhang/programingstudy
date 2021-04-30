using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RootTools;
using RootTools.Memory;

namespace Root_VEGA_P_Vision
{
    public class VisionReview_ViewModel: ObservableObject
    {
        VisionReview_Panel visionReview;
        Review_Panel reviewPanel;
        screwUI screwUITime;
        screwUI screwUIIllum;
        ScrewUI_ViewModel vm1, vm2;

        ScrewUI_ImageViewer_ViewModel ivvm1,ivvm2;
        public VisionReview_ViewModel(Review_Panel reviewPanel)
        {
            this.reviewPanel = reviewPanel;
            screwUITime = new screwUI();
            screwUIIllum = new screwUI();
            vm1 = (ScrewUI_ViewModel)screwUITime.DataContext;
            vm2 = (ScrewUI_ViewModel)screwUIIllum.DataContext;
            ImageData img = new ImageData(1000, 1000);
            img.LoadImageSync(@"D:\03_Projects\Root\Root_VEGA_P_Vision\Resources\screwbf.bmp",new CPoint(0,0));
            vm1.ImageDatas.Add(img);
            vm1.SetCurrentImage(0);
        }

    }
}
