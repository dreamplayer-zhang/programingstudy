using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    public class SideRecipe : RecipeItemBase
    {
        #region [Parameter]
        int[] scanDir;
        int coax, df, ring;
        long tbPos, lrPos;
        #endregion

        #region[Getter/Setter]
        [XmlArray("ScanDir")]
        public int[] ScanDir
        {
            get => scanDir;
            set => SetProperty(ref scanDir, value);
        }
        public int Coax
        {
            get => coax;
            set => SetProperty(ref coax, value);
        }
        public int DF
        {
            get => df;
            set => SetProperty(ref df, value);
        }
        public long TBPos
        {
            get => tbPos;
            set => SetProperty(ref tbPos, value);
        }
        public long LRPos
        {
            get => lrPos;
            set => SetProperty(ref lrPos, value);
        }
        public int Ring
        {
            get => ring;
            set => SetProperty(ref ring, value);
        }
        #endregion
        public SideRecipe() 
        {
            ScanDir = new int[4] { -1,-1,-1,-1 };
        }
        public override void Clear()
        {
            for (int i = 0; i < 4; i++)
                ScanDir[i] = -1;
            coax = 0;
            df = 0;
            ring = 0;
            tbPos = 0;
            lrPos = 0;
            ring = 0;
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
