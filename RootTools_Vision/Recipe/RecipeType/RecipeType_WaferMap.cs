using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace RootTools_Vision
{
    public enum CHIP_TYPE
    {
        NO_CHIP = 0,
        NORMAL = 1,
        EXTRA = 3,
        FLAT_ZONE = 4
    }

    public class RecipeType_WaferMap
    {
        private string m_strDeviceID;
        private string m_strWaferID;
        private int m_nLotNum;
        private int m_nFlatZone;
        private int m_nBadDieNum;
        private int m_nGoodDieNum;
        private int m_nDieNum;

        private int[] mapdata = new int[0];
        private int mapSizeX = 0;
        private int mapSizeY = 0;
        private int masterDieX = 0;
        private int masterDieY = 0;

        private int originDieX = 0;
        private int originDieY = 0;
        private double chipWidth = 0;
        private double chipHeight = 0;
        private double diePitchX = 0;
        private double diePitchY = 0;
        private double scribeLaneX = 0;
        private double scribeLaneY = 0;
        private double sampleCenterLocationX = 0;
        private double sampleCenterLocationY = 0;


        private bool useExtraMap = false;
        private int[] extraMapdata = new int[0];
        private int extraMapSizeX = 0;
        private int extraMapSizeY = 0;
        private int extraMasterDieX = 0;
        private int extraMasterDieY = 0;
        private int extraDieOffsetX = 0;
        private int extraDieOffsetY = 0;

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

        [XmlIgnore]
        public int[] ExtraMapdata 
        { 
            get => extraMapdata; 
        }

        [XmlArray("ExtraMapdata")]
        [XmlArrayItem("row")]
        public string[] ExtraMapDataToString
        {
            get
            {
                string[] map = new string[extraMapSizeY];
                for (int i = 0; i < extraMapSizeY; i++)
                {
                    for (int j = 0; j < extraMapSizeX; j++)
                        map[i] += string.Format("{0}", extraMapdata[i * extraMapSizeX + j]);
                }

                return map;
            }
            set
            {
                string[] map = value;
                if (map.Length == 0)
                {
                    extraMapSizeX = 0;
                    extraMapSizeY = 0;
                }
                else
                {
                    extraMapSizeX = map[0].Length;
                    extraMapSizeY = map.Length;
                    extraMapdata = new int[extraMapSizeX * extraMapSizeY];

                    for (int i = 0; i < extraMapSizeY; i++)
                    {
                        for (int j = 0; j < extraMapSizeX; j++)
                        {
                            extraMapdata[i * extraMapSizeX + j] = int.Parse(map[i][j].ToString());
                        }
                    }
                }
            }
        }


        public double ChipWidth { get => chipWidth; set => chipWidth = value; }
        public double ChipHeight { get => chipHeight; set => chipHeight = value; }
        public double DiePitchX { get => diePitchX; set => diePitchX = value; }
        public double DiePitchY { get => diePitchY; set => diePitchY = value; }
        public double ScribeLaneX { get => scribeLaneX; set => scribeLaneX = value; }
        public double ScribeLaneY { get => scribeLaneY; set => scribeLaneY = value; }
        public double SampleCenterLocationX { get => sampleCenterLocationX; set => sampleCenterLocationX = value; }
        public double SampleCenterLocationY { get => sampleCenterLocationY; set => sampleCenterLocationY = value; }
        public int OriginDieX { get => originDieX; set => originDieX = value; }
        public int OriginDieY { get => originDieY; set => originDieY = value; }

        // Extra Map
        public bool UseExtraMap { get => useExtraMap; set => useExtraMap = value; }
        
        public int ExtraMapSizeX { get => extraMapSizeX; set => extraMapSizeX = value; }
        public int ExtraMapSizeY { get => extraMapSizeY; set => extraMapSizeY = value; }
        public int ExtraMasterDieX { get => extraMasterDieX; set => extraMasterDieX = value; }
        public int ExtraMasterDieY { get => extraMasterDieY; set => extraMasterDieY = value; }
        public int ExtraDieOffsetX { get => extraDieOffsetX; set => extraDieOffsetX = value; }
        public int ExtraDieOffsetY { get => extraDieOffsetY; set => extraDieOffsetY = value; }


        public int GrossDie
        {
            get
            {
                int grossDie = 0;
                if (mapdata != null && mapdata.Length != 0)
                {
                    for(int i = 0;  i < mapdata.Length; i++)
                    {
                        if(mapdata[i] == (int)CHIP_TYPE.NORMAL)
                        {
                            grossDie++;
                        }
                    }
                }
                return grossDie;
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

        public void CreateExtraMap(int mapSizeX, int mapSizeY, int[] _mapdata, int offsetX, int offsetY)
        {
            this.extraMapSizeX = mapSizeX;
            this.extraMapSizeY = mapSizeY;
            this.extraDieOffsetX = offsetX;
            this.extraDieOffsetY = offsetY;

            this.extraMapdata = new int[this.extraMapSizeX * this.extraMapSizeY];
            this.extraMapdata = (int[])_mapdata.Clone();

            bool bDone = false;
            for (int x = 0; x < this.extraMapSizeX; x++)
            {
                for (int y = 0; y < this.extraMapSizeY; y++)
                {
                    if (this.extraMapdata[x + y * this.extraMapSizeX] == (byte)CHIP_TYPE.NORMAL ||
                        this.extraMapdata[x + y * this.extraMapSizeX] == (byte)CHIP_TYPE.EXTRA)
                    {
                        this.extraMasterDieX = x;
                        this.extraMasterDieY = y;
                        bDone = true;
                        break;
                    }
                }
                if (bDone) break;
            }
        }

        public void Clear()
        {
            mapdata = new int[0];
            mapSizeX = 0;
            mapSizeY = 0;
            masterDieX = 0;
            masterDieY = 0;

            UseExtraMap = false;
            extraMapdata = new int[0];
            ExtraMapSizeX = 0;
            ExtraMapSizeY = 0;
            ExtraMasterDieX = 0;
            ExtraMasterDieY = 0;
        }

        public void CreateWaferMap(int mapSizeX, int mapSizeY, CHIP_TYPE type)
        {
            this.mapSizeX = mapSizeX;
            this.mapSizeY = mapSizeY;
            this.mapdata = new int[this.mapSizeX * this.mapSizeY];

            this.masterDieX = 0;
            this.masterDieY = 0;
            for (int x = 0; x < this.MapSizeX; x++)
            {
                for (int y = 0; y < this.MapSizeY; y++)
                {
                    this.mapdata[x + y * this.MapSizeX] = (int)type;
                }
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

        public void Invert()
        {
            for (int x = 0; x < this.MapSizeX; x++)
            {
                for (int y = 0; y < this.MapSizeY; y++)
                {
                    if (this.mapdata[x + y * this.MapSizeX] == (byte)CHIP_TYPE.NORMAL)
                    {
                        this.mapdata[x + y * this.MapSizeX] = (byte)CHIP_TYPE.NO_CHIP;
                    }
                    else
                    {
                        this.mapdata[x + y * this.MapSizeX] = (byte)CHIP_TYPE.NORMAL;
                    }
                }
            }
        }

        public void HorizontalFlip() // LR
        {
            int[] flipmapdata = new int[MapSizeX * MapSizeY];

            for (int x = 0; x < this.MapSizeX; x++)
            {
                for (int y = 0; y < this.MapSizeY; y++)
                {
                    flipmapdata[(this.MapSizeX - x - 1) + y * this.MapSizeX] = this.mapdata[x + y * this.MapSizeX];
                }
            }
            this.mapdata = flipmapdata;
        }

        public void VerticalFlip() // UD
        {
            int[] flipmapdata = new int[MapSizeX * MapSizeY];

            for (int y = 0; y < this.MapSizeY; y++)
            {
                for (int x = 0; x < this.MapSizeX; x++)
                {
                    flipmapdata[x + (this.MapSizeY - y - 1) * this.MapSizeX] = this.mapdata[x + y * this.MapSizeX];
                }
            }
            this.mapdata = flipmapdata;
        }

        public void OpenASCMapData(StreamReader stdFile)
        {
            // [ 2021-06-01 ] : Imported by jhan from VisionWorks2

            int nSeed;
            string tmp;

            //m_szShot = CSize(0, 0);
            //m_ptShotStartChip = CPoint(0, 0);
            m_nGoodDieNum = m_nBadDieNum = 0;

            tmp = stdFile.ReadLine();
            nSeed = tmp.IndexOf(":");
            if (tmp.IndexOf("\r") != -1)
            {
                m_strDeviceID = tmp.Substring(nSeed + 1, tmp.Length - nSeed - 3);
            }
            else
            {
                m_strDeviceID = tmp.Substring(nSeed + 1, tmp.Length - nSeed - 1);
            }

            tmp = stdFile.ReadLine();
            if (tmp == "") tmp = stdFile.ReadLine();
            nSeed = tmp.IndexOf(":");
            if (tmp.IndexOf("\r") != -1)
            {
                m_strWaferID = tmp.Substring(nSeed + 1, tmp.Length - nSeed - 3);
            }
            else
            {
                m_strWaferID = tmp.Substring(nSeed + 1, tmp.Length - nSeed - 1);
            }

            m_nLotNum = Int32.Parse(m_strWaferID.Substring(m_strWaferID.IndexOf('-') + 1, 2));

            // Wafer id에 '-' 가 존재하면 그 부분을 잘 잘라 가져오는 루틴 cmahn 2014.11.27
            int nHyphenNum = CountChar(m_strWaferID, '-');
            if (nHyphenNum > 1)
            {
                int nSecond = m_strWaferID.IndexOf('-'); ;
                for (int n = 1; n < nHyphenNum; n++)
                    nSecond = m_strWaferID.IndexOf('-', nSecond + 1);
                m_strWaferID = m_strWaferID.Substring(0, nSecond);
            }
            else
                //m_strWaferID = m_strWaferID.Left(m_strWaferID.IndexOf('-'));

            tmp = stdFile.ReadLine();
            if (tmp == "") tmp = stdFile.ReadLine();
            nSeed = tmp.IndexOf(":");
            if (tmp.IndexOf("\r") != -1) mapSizeX = Int32.Parse(tmp.Substring(nSeed + 1, tmp.Length - nSeed - 3));
            else mapSizeX = Int32.Parse(tmp.Substring(nSeed + 1, tmp.Length - nSeed - 1));

            tmp = stdFile.ReadLine();
            if (tmp == "") tmp = stdFile.ReadLine();
            nSeed = tmp.IndexOf(":");
            if (tmp.IndexOf("\r") != -1) mapSizeY = Int32.Parse(tmp.Substring(nSeed + 1, tmp.Length - nSeed - 3));
            else mapSizeY = Int32.Parse(tmp.Substring(nSeed + 1, tmp.Length - nSeed - 1));

            //if (mapdata != null) delete[] mapdata;
            mapdata = new int[mapSizeX * mapSizeY];
            int[] p = mapdata;

            tmp = stdFile.ReadLine();   // REFDIE
            if (tmp == "") tmp = stdFile.ReadLine();
            for (int i = 0; i < mapSizeY; i++)
            {
                tmp = stdFile.ReadLine();
                if (tmp == "") tmp = stdFile.ReadLine();
                for (int j = 0; j < mapSizeX; j++)
                {
                    switch (tmp[j])
                    {
                        case 'D': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_DEFECT; m_nBadDieNum++; break; // Reject
                        case '.': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_NOCHIP; break; // Empty
                        case '1': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_GOOD_1; m_nGoodDieNum++; break; // Good1
                        case '2': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_DIE_1; m_nGoodDieNum++; break; // Die1
                        case '3': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_DIE_2; m_nGoodDieNum++; break; // Die2
                        case '4': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_FLATZONE; break; // Direction
                        case '5': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_GOOD_2; m_nGoodDieNum++; break; // Good2
                        case 'A': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_ALIGN; break; // Align
                        case 'R': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_REFERENCE; break; // Reference
                        case 'P': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_PSMARK; break; // PS Mark
                        case 'I': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_INVALID; break; // Invalid
                        case 'B': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_BEF_DEFECT; break; // Before Bad 140722 천안 MapData
                        //////////////////////////////////////////////////////////////////////////
                        // Ext Map 알파벳 다 적용 시켜 봅시다.
                        // [ 2011-9-21 ] : YS-Park
                        case '0': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_0; break; // Ext 0
                        case '6': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_6; break; // Ext 6
                        case '7': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_7; break; // Ext 7
                        case '8': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_8; break; // Ext 8
                        case '9': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_9; break; // Ext 9
                        //case 'B' : p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_B; break; // Ext B
                        case 'C': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_C; break; // Ext C
                        case 'E': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_E; break; // Ext E
                        case 'F': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_F; break; // Ext F
                        case 'G': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_G; break; // Ext G
                        case 'H': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_H; break; // Ext H
                        //case 'I' : p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_I; break; // Ext I
                        case 'J': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_J; break; // Ext J
                        case 'K': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_K; break; // Ext K
                        case 'L': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_L; break; // Ext L
                        case 'M': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_M; break; // Ext M
                        case 'N': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_N; break; // Ext N
                        case 'O': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_O; break; // Ext O
                        //case 'P' : p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_P; break; // Ext P
                        case 'Q': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_Q; break; // Ext Q
                        case 'S': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_S; break; // Ext S
                        case 'T': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_T; break; // Ext T
                        case 'U': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_U; break; // Ext U
                        case 'V': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_V; break; // Ext V
                        case 'W': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_W; break; // Ext W
                        case 'X': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_X; break; // Ext X
                        case 'Y': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_Y; break; // Ext Y
                        case 'Z': p[i * mapSizeX + j] = Constants.ASCWaferMap.WAFER_MAP_CHIP_EXT_Z; break; // Ext Z
                    }
                }
            }

            m_nDieNum = m_nGoodDieNum + m_nBadDieNum;
            tmp = stdFile.ReadLine(); // check sum 계산해야함.
            if (tmp == "") tmp = stdFile.ReadLine();
            tmp = stdFile.ReadLine();
            if (tmp == "") tmp = stdFile.ReadLine();

            if (tmp.IndexOf("TOP") != -1) m_nFlatZone = Constants.ASCWaferMap.WAFER_MAP_FLATZONE_TOP;
            else if (tmp.IndexOf("RIGHT") != -1) m_nFlatZone = Constants.ASCWaferMap.WAFER_MAP_FLATZONE_RIGHT;
            else if (tmp.IndexOf("LEFT") != -1) m_nFlatZone = Constants.ASCWaferMap.WAFER_MAP_FLATZONE_LEFT;
            else if (tmp.IndexOf("BOTTOM") != -1) m_nFlatZone = Constants.ASCWaferMap.WAFER_MAP_FLATZONE_BOTTOM;

            /*//SampleInspection
            if (m_pSampleInspMapData != NULL) delete[] m_pSampleInspMapData;
            m_pSampleInspMapData = new BYTE[mapSizeX * mapSizeY];
            memset(m_pSampleInspMapData, SAMPLE_MAP_CHIP_UNSELECTED, mapSizeX * mapSizeY);

            p = m_pSampleInspMapData;

            tmp = stdFile.ReadLine();   //SampleInspection
            if (tmp == "") tmp = stdFile.ReadLine();

            if (tmp.Find("SampleInspection") != -1)
            {
                for (int i = 0; i < mapSizeY; i++)
                {
                    tmp = stdFile.ReadLine();
                    if (tmp == "") tmp = stdFile.ReadLine();
                    for (int j = 0; j < mapSizeX; j++, p++)
                    {
                        switch (tmp[j])
                        {
                            case '1': *p = SAMPLE_MAP_CHIP_SELECTED; break; // Reject
                            case '.': *p = SAMPLE_MAP_CHIP_UNSELECTED; break;   // Empty
                            case '2': *p = SAMPLE_MAP_CHIP_INVALID; break;  // Invalid
                            case '3': *p = SAMPLE_MAP_CHIP_DISCOLOR; break; // Discolor
                        }
                    }
                }
                MakeSampleChipXList();
            }
            tmp = stdFile.ReadLine();   //m_bUseSampleImage
            if (tmp == "") tmp = stdFile.ReadLine();
            tmp = stdFile.ReadLine();   //m_bSendMapG85ImageVS
            if (tmp == "") tmp = stdFile.ReadLine();
            tmp = stdFile.ReadLine();   //m_nSampleCenterX
            if (tmp == "") tmp = stdFile.ReadLine();
            m_fSampleCenterX = (float)atof(tmp);
            tmp = stdFile.ReadLine();   //m_nSampleCenterY
            if (tmp == "") tmp = stdFile.ReadLine();
            m_fSampleCenterY = (float)atof(tmp);*/
        }

        public void ConvertACSMapDataToWaferMap()
        {
            if (mapdata != null && mapSizeX > 0 && mapSizeY > 0)
            {
                for (int i = 0; i < mapSizeY; i++)
                {
                    for (int j = 0; j < mapSizeX; j++)
                        if (mapdata[i * mapSizeX + j] == 255)
                        {
                            mapdata[i * mapSizeX + j] = 0;
                        }
                }
            }
        }

        public void OpenXmlMapData(StreamReader stdFile)
        {
            // [ 2021-06-21 ] : Developed by jhan

        }

        public void OpenKlarfMapData(StreamReader stdFile)
        {
            // [ 2021-06-15 ] : Imported by jhan from VisionWorks2

            string strNumber = "";
            string buffX;
            string buffY;
            string tmp;

            int[] SampleX = new int[10000];
            int[] SampleY = new int[10000];
            int Min_x = 99, Max_x = 0, Min_y = 99, Max_y = 0;
            int num = 0;

            tmp = stdFile.ReadLine();
            tmp.Trim();
            while (tmp.IndexOf("SampleTestPlan ") == -1)
            {
                tmp = stdFile.ReadLine();
            }
            for (int i = 13; i < tmp.Length; i++)
            {
                char ch = tmp[i];
                if (ch >= '0' && ch <= '9')
                {
                    strNumber += ch;
                }
            }

            int n = Int32.Parse(strNumber);
            while (tmp.IndexOf(";") == -1)
            {
                tmp = stdFile.ReadLine();
                tmp.Trim();

                buffX = tmp.Substring(0, tmp.IndexOf(' '));
                buffY = tmp.Substring(tmp.IndexOf(' ') + 1, tmp.Length - tmp.IndexOf(' ') - 1);

                SampleX[num] = Int32.Parse(buffX);
                if (buffY.IndexOf(";") != -1)
                {
                    buffY = buffY.Substring(0, buffY.Length - 1);
                }
                SampleY[num] = Int32.Parse(buffY);
                num = num + 1;
                buffY = "";
                buffX = "";
            }

            for (int i = 0; i < n; i++)
            {
                if (SampleX[i] > Max_x)
                {
                    Max_x = SampleX[i];
                }
                if (SampleX[i] < Min_x)
                {
                    Min_x = SampleX[i];
                }
                if (SampleY[i] > Max_y)
                {
                    Max_y = SampleY[i];
                }
                if (SampleY[i] < Min_y)
                {
                    Min_y = SampleY[i];
                }
            }

            mapSizeX = (Max_x - Min_x + 1);
            mapSizeY = (Max_y - Min_y + 1);

            mapdata = new int[mapSizeX * mapSizeY];
            int[] p = mapdata;

            int cnt = 0;
            int t = 0;
            for (int i = 0; i < n; i++)
            {
                t = (SampleX[cnt] - Min_x) + (mapSizeX * (Max_y - SampleY[cnt]));
                p[t] = 1;
                cnt++;
            }

            /*//SampleInspection
            if (m_pSampleInspMapData != NULL) delete[] m_pSampleInspMapData;
            m_pSampleInspMapData = new BYTE[mapSizeX * mapSizeY];
            memset(m_pSampleInspMapData, 0, sizeof(BYTE) * mapSizeX * mapSizeY);
            p = m_pSampleInspMapData;

            cnt = 0;
            for (int i = 0; i < n; i++)
            {
                int t = (SampleX[cnt] - Min_x) + (m_nUnitXNum * (Max_y - SampleY[cnt]));
                *(p + t) = 1;
                cnt++;
            }*/
        }

        int CountChar(string str, char c)
        {
            int nCount = 0;
            int nIndex = 0;
            while (true)
            {
                nIndex = str.IndexOf(c, nIndex);
                if (nIndex >= 0)
                {
                    nCount++; nIndex++;
                }
                else
                    break;
            }

            return nCount;

        }
    }
}
