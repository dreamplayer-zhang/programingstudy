using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class OriginRecipe : RecipeItemBase
    {
        #region [Parameter]
        private int originX;
        private int originY;
        private int diePitchX;
        private int diePitchY;
        private int inspectionBufferOffsetX;
        private int inspectionBufferOffsetY;

        private RecipeType_ImageData masterImage;
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

        //[XmlIgnore]
        public RecipeType_ImageData MasterImage
        { 
            get => masterImage; 
            set => masterImage = value;
        }

        #endregion

        public OriginRecipe()
        {

        }

        public override bool Read(string recipePath)
        {
            //return masterImage.Read(recipePath);

            return true;
        }

        public override bool Save(string recipePath)
        {

            return true;
            //masterImage.FileName = "MasterImage.bmp";
            //return masterImage.Save(recipePath);
        }

        public override void Clear()
        {
            this.OriginX = 0;
            this.OriginY = 0;
            this.DiePitchX = 0;
            this.DiePitchY = 0;
            this.InspectionBufferOffsetX = 0;
            this.InspectionBufferOffsetY = 0;

        }
    }
}
