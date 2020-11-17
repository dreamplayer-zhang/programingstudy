﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WaferMapInfo
    {
        public byte[] pWaferMap;
        public int nMapSizeX = 1;
        public int nMapSizeY = 1;
        public int nMasterDieX = 1;
        public int nMasterDieY = 1;
        public List<ChipData> ListWaferMap;

        public byte[] WaferMapData
        {
            get { return this.pWaferMap; }
        }

        public int MapSizeX
        {
            get { return this.nMapSizeX; }
        }

        public int MapSizeY
        {
            get { return this.nMapSizeY; }
        }

        public int MasterDieX
        {
            get { return this.nMasterDieX; }
        }

        public int MasterDieY
        {
            get { return this.nMasterDieY; }
        }

        public WaferMapInfo()
        {
        }

        public WaferMapInfo(int mapSizeX, int mapSizeY, byte[] waferMap)
        {
            this.nMapSizeX = mapSizeX;
            this.nMapSizeY = mapSizeY;
            this.pWaferMap = new byte[this.nMapSizeX * this.nMapSizeY];
            this.pWaferMap = (byte[])waferMap.Clone();
            this.ListWaferMap = new List<ChipData>();

            bool bDone = false;
            for (int x = 0; x < this.nMapSizeX; x++)
            {
                for (int y = 0; y < this.nMapSizeY; y++)
                {
                    if (this.pWaferMap[x + y * this.nMapSizeX] == 1)
                    {
                        this.nMasterDieX = x;
                        this.nMasterDieY = y;
                        bDone = true;
                        break;
                    }
                }
                if (bDone) break;
            }
        }

        public WaferMapInfo(int mapSizeX, int mapSizeY, byte[] waferMap, List<ChipData> chipDatas)
        {
            this.nMapSizeX = mapSizeX;
            this.nMapSizeY = mapSizeY;
            this.pWaferMap = new byte[this.nMapSizeX * this.nMapSizeY];
            this.pWaferMap = (byte[])waferMap.Clone();

            this.ListWaferMap = new List<ChipData>();
            this.ListWaferMap = chipDatas;

            bool bDone = false;
            for (int x = 0; x < this.nMapSizeX; x++)
            {
                for (int y = 0; y < this.nMapSizeY; y++)
                {
                    if (this.pWaferMap[x + y * this.nMapSizeX] == 1)
                    {
                        this.nMasterDieX = x;
                        this.nMasterDieY = y;
                        bDone = true;
                        break;
                    }
                }
                if (bDone) break;
            }
        }

        public void SetMasterDie(int x, int y)
        {
            this.nMasterDieX = x;
            this.nMasterDieY = y;
        }
    }
}
