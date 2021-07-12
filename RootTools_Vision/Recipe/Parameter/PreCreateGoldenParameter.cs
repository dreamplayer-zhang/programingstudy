using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using RootTools;
using RootTools_Vision;

namespace RootTools_Vision
{
    public class PreCreateGoldenParameter : FrontsideOthersParameterBase
    {
        public enum SelectYIndexMethod
        {
            Whole = 0,
            Inner,
            Outer
        }

        public PreCreateGoldenParameter()
        {

        }

        // 검사 파라매터 적용 대상 셋팅
        #region [Parameters]
        
        private CreateRefImageMethod createRefImage = CreateRefImageMethod.Average;
        private SelectYIndexMethod selectYIndex = SelectYIndexMethod.Whole;

        #endregion

        #region [Getter Setter]

        [Category("Create Option")]
        [DisplayName("Create Ref Image Method")]
        public CreateRefImageMethod CreateRefImage
        {
            get => this.createRefImage;
            set
            {
                SetProperty<CreateRefImageMethod>(ref this.createRefImage, value);
            }
        }

        [Category("Create Option")]
        [DisplayName("Select Y Index Method")]
        public SelectYIndexMethod SelectYIndex
        {
            get => this.selectYIndex;
            set
            {
                SetProperty<SelectYIndexMethod>(ref this.selectYIndex, value);
            }
        }

        #endregion
    }
}
