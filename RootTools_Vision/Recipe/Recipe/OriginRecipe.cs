using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class OriginRecipe : RecipeBase
    {
        #region [Parameter]
        private int originX;
        private int originY;
        private int diePitchX;
        private int diePitchY;
        private int inspectionBufferOffsetX;
        private int inspectionBufferOffsetY;
        #endregion

        #region [Getter Setter]
        public int OriginX
        {
            get => this.originX;
            set
            {
                SetProperty<int>(ref this.originX, value);
            }
        }

        public int OriginY
        {
            get => this.originY;
            set
            {
                SetProperty<int>(ref this.originY, value);
            }
        }

        public int DiePitchX
        {
            get => this.diePitchX;
            set
            {
                SetProperty<int>(ref this.diePitchX, value);
            }
        }

        public int DiePitchY
        {
            get => this.diePitchY;
            set
            {
                SetProperty<int>(ref this.diePitchY, value);
            }
        }

        public int InspectionBufferOffsetX
        {
            get => this.inspectionBufferOffsetX;
            set
            {
                SetProperty<int>(ref this.inspectionBufferOffsetX, value);
            }
        }

        public int InspectionBufferOffsetY
        {
            get => this.inspectionBufferOffsetY;
            set
            {
                SetProperty<int>(ref this.inspectionBufferOffsetY, value);
            }
        }
        #endregion 

        public OriginRecipe()
        {

        }

        public override bool Read(string recipePath)
        {
            return true;
        }

        public override bool Save(string recipePath)
        {
            return true;
        }
    }
}
