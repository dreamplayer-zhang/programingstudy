using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class MaskRecipe : RecipeBase
    {
        CPoint originPoint;
        CRect boundingBox;
        List<RecipeType_Mask> maskList;
        
        public List<RecipeType_Mask> MaskList 
        { 
            get => maskList; 
            set => maskList = value; 
        }
        public CPoint OriginPoint 
        { 
            get => originPoint; 
            set => originPoint = value; 
        }

        [XmlIgnore]
        public CRect BoundingBox 
        { 
            get => boundingBox; 
            set => boundingBox = value; 
        }

        public MaskRecipe()
        {
            this.originPoint = new CPoint();
            this.boundingBox = new CRect();
            this.maskList = new List<RecipeType_Mask>();
            
        }

        public MaskRecipe(CPoint _originPoint)
        {
            this.originPoint = _originPoint;
            this.boundingBox = new CRect();
            this.maskList = new List<RecipeType_Mask>();
        }

        public void AddMask(RecipeType_Mask _mask)
        {
            this.maskList.Add(_mask);

            CalcBoundingBox();
        }

        public void SetOriginPoint(int x, int y)
        {
            this.OriginPoint.X = x;
            this.OriginPoint.Y = y;
        }

        public void CalcBoundingBox()
        {
            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;

            foreach (RecipeType_Mask mask in maskList)
            {
                minX = minX > mask.BoundingBox.Left ? mask.BoundingBox.Left : minX;
                minY = minY > mask.BoundingBox.Top ? mask.BoundingBox.Top : minY;
                maxX = maxX < mask.BoundingBox.Right ? mask.BoundingBox.Right : maxX;
                maxY = maxY < mask.BoundingBox.Bottom ? mask.BoundingBox.Bottom : maxY;
            }

            boundingBox.Left = minX;
            boundingBox.Top = minY;
            boundingBox.Right = maxX;
            boundingBox.Bottom = maxY;
        }

        public override bool Read(string recipePath)
        {
            return true;
        }

        public override bool Save(string recipePath)
        {
            return true;
        }

        public override void Clear()
        {
            OriginPoint = new CPoint();
            BoundingBox = new CRect();
            MaskList.Clear();
        }
    }
}
