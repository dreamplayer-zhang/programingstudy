using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RootTools_Vision
{
    public class ChipData
    {
        public ChipInfo chipinfo;
        public bool bSampling;
        public Point DiePoint;
        public Point MapIndex;

        public void SetChip(ChipInfo newchipinfo)
        {
            this.chipinfo = newchipinfo;
        }
    }
    public enum ChipInfo
    {
        Normal_Chip = 1,
        Origin_Chip = 2,
        Partial_Chip = 3,
        No_Chip = 4,
    }

    public class RecipeInfo_MapData : IRecipeInfo
    {
        public WaferMapInfo m_WaferMap;
        public RecipeInfo_MapData()
        {
        }

        public RecipeInfo_MapData(WaferMapInfo waferMapInfo)
        {
            m_WaferMap = new WaferMapInfo(waferMapInfo.MapSizeX, waferMapInfo.MapSizeY, waferMapInfo.WaferMapData, waferMapInfo.ListWaferMap);
        }

        public void SetWaferMapData(WaferMapInfo waferMapInfo)
        {
            m_WaferMap = waferMapInfo;
        }

    }
}
