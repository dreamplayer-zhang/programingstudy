using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RootTools_Vision
{

    public enum CHIP_TYPE
    {
        NO_CHIP = 0,
        NORMAL = 1,
    }

    public class RecipeType_WaferMap
    {
        private int[] mapdata = new int[0];
        private int mapSizeX = 0;
        private int mapSizeY = 0;
        private int masterDieX = 0;
        private int masterDieY = 0;

        #region [Getter Setter]
        public int MapSizeX
        {
            get 
            {
                return this.mapSizeX; 
            }
            set 
            { 
                this.mapSizeX = value; 
            }
        }

        public int MapSizeY
        {
            get 
            { 
                return this.mapSizeY;
            }
            set 
            { 
                this.mapSizeY = value; 
            }
        }

        public int MasterDieX
        {
            get 
            { 
                return this.masterDieX; 
            }
            set
            {
                this.masterDieX = value;
            }
        }

        public int MasterDieY
        {
            get 
            { 
                return this.masterDieY; 
            }
            set
            {
                this.masterDieY = value;
            }
        }


        [XmlIgnore]
        public int[] Data
        {
            get { return this.mapdata; }
        }


        //[XmlElement(ElementName = "MapData")]
        [XmlArray("Mapdata")]
        [XmlArrayItem("row")]
        public string[] DataToString
        {
            get 
            {
                string[] map = new string[mapSizeY];
                for(int i = 0; i < mapSizeY; i++)
                {
                    for (int j = 0; j < mapSizeX; j++)
                        map[i] += string.Format("{0}", mapdata[i * mapSizeX + j]);
                }

                return map; 
            }
            set
            {
                string[] map = value;
                if(map.Length == 0)
                {
                    mapSizeX = 0;
                    mapSizeY = 0;
                }
                else
                {
                    mapSizeX = map[0].Length;
                    mapSizeY = map.Length;
                    mapdata = new int[mapSizeX * mapSizeY];

                    for (int i = 0; i < mapSizeY; i++)
                    {
                        for (int j = 0; j < mapSizeX; j++)
                        {
                            mapdata[i * mapSizeX + j] = int.Parse(map[i][j].ToString());
                        }
                    }
                }

               
            }
        }

        #endregion

        public RecipeType_WaferMap()
        {

        }

        public RecipeType_WaferMap(int _mapSizeX, int _mapSizeY, int[] _mapdata)
        {
            this.mapSizeX = _mapSizeX;
            this.mapSizeY = _mapSizeY;
            this.mapdata = new int[this.MapSizeX * this.MapSizeY];
            this.mapdata = (int[])_mapdata.Clone();

            bool bDone = false;
            for (int x = 0; x < this.mapSizeX; x++)
            {
                for (int y = 0; y < this.mapSizeY; y++)
                {
                    if (this.mapdata[x + y * this.MapSizeX] == (byte)CHIP_TYPE.NORMAL)
                    {
                        this.masterDieX = x;
                        this.masterDieY = y;
                        bDone = true;
                        break;
                    }
                }
                if (bDone) break;
            }
        }

        public void CreateWaferMap(int mapSizeX, int mapSizeY, int[] waferMap)
        {
            this.mapSizeX = mapSizeX;
            this.mapSizeY = mapSizeY;
            this.mapdata = new int[this.mapSizeX * this.mapSizeY];
            this.mapdata = (int[])waferMap.Clone();

            bool bDone = false;
            for (int x = 0; x < this.MapSizeX; x++)
            {
                for (int y = 0; y < this.MapSizeY; y++)
                {
                    if (this.mapdata[x + y * this.MapSizeX] == (byte)CHIP_TYPE.NORMAL)
                    {
                        this.masterDieX = x;
                        this.masterDieY = y;
                        bDone = true;
                        break;
                    }
                }
                if (bDone) break;
            }
        }

        public void SetMasterDie(int x, int y)
        {
            this.masterDieX = x;
            this.masterDieY = y;
        }

        public CHIP_TYPE GetChipType(int x, int y)
        {
            return (CHIP_TYPE)this.Data[this.MapSizeX * y + x];
        }

        public void SetChipType(int x, int y, CHIP_TYPE type)
        {
            this.Data[this.MapSizeX * y + x] = (int)type;
        }

        public RecipeType_WaferMap Clone()
        {
            RecipeType_WaferMap map = new RecipeType_WaferMap(this.MapSizeX, this.MapSizeY, this.mapdata);
            return map;
        }

    }
}
