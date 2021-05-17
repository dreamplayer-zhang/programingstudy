using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class EUVOriginRecipe:RecipeItemBase
    {
        #region [Parameter]
        OriginInfo tdiInfo, stainInfo,sideTBInfo,sideLRInfo;
        #endregion
        #region [Getter Setter]
        public OriginInfo TDIOrigin 
        { 
            get => tdiInfo; 
            set => SetProperty(ref tdiInfo, value); 
        }
        public OriginInfo StainOrigin { get => stainInfo; set => SetProperty(ref stainInfo, value); }
        public OriginInfo SideTBOrigin { get => sideTBInfo; set => SetProperty(ref sideTBInfo, value); }
        public OriginInfo SideLROrigin { get => sideLRInfo; set => SetProperty(ref sideLRInfo, value); }

        public EUVOriginRecipe()
        {
            TDIOrigin = new OriginInfo(new CPoint(0, 0), new CPoint(0, 0));
            StainOrigin = new OriginInfo(new CPoint(0, 0), new CPoint(0, 0));
            SideTBOrigin = new OriginInfo(new CPoint(0, 0), new CPoint(0, 0));
            SideLROrigin = new OriginInfo(new CPoint(0, 0), new CPoint(0, 0));
        }
        public override void Clear()
        {
            TDIOrigin = new OriginInfo(new CPoint(0, 0), new CPoint(0, 0));
            StainOrigin = new OriginInfo(new CPoint(0, 0), new CPoint(0, 0));
            SideTBOrigin = new OriginInfo(new CPoint(0, 0), new CPoint(0, 0));
            SideLROrigin = new OriginInfo(new CPoint(0, 0), new CPoint(0, 0));
        }

        public override bool Read(string recipePath)
        {
            return true;
        }

        public override bool Save(string recipePath)
        {
            return true;
        }
        #endregion
    }
    public class OriginInfo: ObservableObject
    {
        CPoint origin, originSize;
        #region [Property]
        public CPoint Origin {
            get => origin;
            set => SetProperty(ref origin, value);      
        }
        public CPoint OriginSize { get => originSize; set => SetProperty(ref originSize, value); }
        #endregion
        public OriginInfo(CPoint Origin, CPoint OriginSize)
        {
            this.Origin = Origin;
            this.OriginSize = OriginSize;
        }
    }
}
