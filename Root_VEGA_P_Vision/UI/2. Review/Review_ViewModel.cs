using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class Review_ViewModel : ObservableObject
    {
        VisionReview_ViewModel visionReviewVM;
        ParticleReview_ViewModel particleReviewVM;
        ResultSummary_ViewModel resultSummaryVM;

        #region Property
        public VisionReview_ViewModel VisionReview
        {
            get => visionReviewVM;
            set => SetProperty(ref visionReviewVM, value);
        }
        public ParticleReview_ViewModel ParticleReview
        {
            get => particleReviewVM;
            set => SetProperty(ref particleReviewVM, value);
        }
        public ResultSummary_ViewModel ResultSummary
        {
            get => resultSummaryVM;
            set => SetProperty(ref resultSummaryVM, value);
        }
        #endregion
        public Review_ViewModel()
        {
            visionReviewVM = new VisionReview_ViewModel();
            particleReviewVM = new ParticleReview_ViewModel();
            resultSummaryVM = new ResultSummary_ViewModel();
        }
    }
}
