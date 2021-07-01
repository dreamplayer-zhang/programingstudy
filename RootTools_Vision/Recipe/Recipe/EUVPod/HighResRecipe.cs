using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class HighResRecipe : RecipeItemBase
    {
        #region Property
        double radius, power;//mask 관련
        long startZ, endZ;
        int stepCnt,dome,coax;
        List<CPoint> scanPosList;

        #region [Get/Set]
        public double Radius
        {
            get => radius;
            set => SetProperty(ref radius, value);
        }
        public double Power
        {
            get => power;
            set => SetProperty(ref power, value);
        }
        public long StartZ
        {
            get => startZ;
            set => SetProperty(ref startZ, value);
        }
        public long EndZ
        {
            get => endZ;
            set => SetProperty(ref endZ, value);
        }
        public int StepCnt
        {
            get => stepCnt;
            set => SetProperty(ref stepCnt, value);
        }
        public List<CPoint> ScanPosList
        {
            get => scanPosList;
            set => SetProperty(ref scanPosList, value);
        }
        public int Dome
        {
            get => dome;
            set => SetProperty(ref dome, value);
        }
        public int Coax
        {
            get => coax;
            set => SetProperty(ref coax, value);
        }
        #endregion
        #endregion
        public HighResRecipe() { }
        public override void Clear()
        {
            radius = power = startZ = endZ = stepCnt = dome = coax = 0;
            scanPosList.Clear();
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
