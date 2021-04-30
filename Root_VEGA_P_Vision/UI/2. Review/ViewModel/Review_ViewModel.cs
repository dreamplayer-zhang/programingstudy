using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    public class Review_ViewModel
    {
        Review_Panel reviewPanel;
        VisionReview_Panel visionReviewPanel;
        ParticleReview_Panel particleReviewPanel;
        ResultSummary_Panel resultSummaryPanel;

        public Review_ViewModel(Review_Panel reviewPanel)
        {
            this.reviewPanel = reviewPanel;
            visionReviewPanel = new VisionReview_Panel();
            particleReviewPanel = new ParticleReview_Panel();
            resultSummaryPanel = new ResultSummary_Panel();
            visionReviewPanel.DataContext = new VisionReview_ViewModel(reviewPanel);
        }
    }
}
