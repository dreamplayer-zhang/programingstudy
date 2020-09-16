using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision
{
    public class WaferMapInfo
    {
        byte[] pWaferMap;
        int nMapSizeX;
        int nMapSizeY;
        int nDieSizeX;
        int nDieSizeY;

        int nMasterDieX;
        int nMasterDieY;

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

        public int DieSizeX
        {
            get { return this.nDieSizeX; }
        }

        public int DieSizeY
        {
            get { return this.nDieSizeY; }
        }

        public int MasterDieX
        {
            get { return this.nMasterDieX; }
        }

        public int MasterDieY
        {
            get { return this.nMasterDieY; }
        }

        public WaferMapInfo(int mapSizeX, int mapSizeY, byte[] waferMap, int dieSizeX, int dieSizeY)
        {
            this.nMapSizeX = mapSizeX;
            this.nMapSizeY = mapSizeY;
            this.nDieSizeX = dieSizeX;
            this.nDieSizeY = dieSizeY;

            this.pWaferMap = new byte[this.nMapSizeX * this.nMapSizeY];

            this.pWaferMap = (byte[])waferMap.Clone();

            bool bDone = false;
            for (int x = 0; x < this.nMapSizeX; x++)
            {
                for (int y = 0; y < this.nMapSizeY; y++)
                {
                    if (this.pWaferMap[x + y * this.nMapSizeX] == 1) ;
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
