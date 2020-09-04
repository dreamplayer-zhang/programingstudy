using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace RootTools_Vision
{
    public class WorkplaceBundle : Collection<Workplace>
    {
        private int nUnitSizeX;

        public int UnitSizeX
        {
            get { return nUnitSizeX; }
        }

        private int nUnitSizeY;
        public int UnitSizeY
        {
            get { return nUnitSizeY; }
        }

        private int currentWorkplaceIndex = 0;

        public WorkplaceBundle()
        {

        }

        /// <summary>
        /// 작업을 해야하는 작업장을 리턴한다.
        /// 모든 작업장이 완료되었거나 할당된 작업이 없으면 null을 리턴
        /// 인덱스가 자동으로 증가하기 때문에 리턴이 되면 반드시 작업을 하도록 해야함
        /// 아니면 인덱스를 원복시켜야함
        /// </summary>
        /// <returns></returns>
        public Workplace GetNextWorkplace()
        {
            if (this.currentWorkplaceIndex > this.Count - 1)
                return null;

            return this[currentWorkplaceIndex++];
        }

        public void IsCompleted()
        {

        }

        public void Reset()
        {
            foreach (Workplace workplace in this)
            {
                workplace.State = WORKPLACE_STATE.NONE;
            }

            this.currentWorkplaceIndex = 0;
        }

        static public WorkplaceBundle CreateWaferMap(int nSizeX, int nSizeY, byte[] wafermap)
        {
            WorkplaceBundle bundle = new WorkplaceBundle();
            bundle.nUnitSizeX = nSizeX;
            bundle.nUnitSizeY = nSizeY;

            for (int y = 0; y < nSizeY; y++)
            {
                for (int x = 0; x < nSizeX; x++)
                {
                    int index = y * nSizeX + x;
                    if(wafermap[index] == 1)
                    {
                        bundle.Add(new Workplace(index, new Point(x, y), new Point(0, 0), new Size(0, 0)));
                    }
                }
            }
            return bundle;
        }

        static public WorkplaceBundle CreateWaferMap(WaferMapInfo mapInfo)
        {
            WorkplaceBundle bundle = new WorkplaceBundle();

            
            byte[] wafermap = mapInfo.WaferMapData;
            int nSizeX = mapInfo.MapSizeX;
            int nSizeY = mapInfo.MapSizeY;
            int nMasterX = mapInfo.MasterDieX;
            int nMasterY = mapInfo.MasterDieY;
            int nDiePitchX = mapInfo.DieSizeX;
            int nDiePitchY = mapInfo.DieSizeY;

            bundle.nUnitSizeX = nSizeX;
            bundle.nUnitSizeY = nSizeY;

            int index = 0;

            // Right
            for(int x = nMasterX; x < nSizeX; x++)
            {
                // Top
                for(int y = nMasterY; y >= 0; y--)
                {
                    if(wafermap[ x + y * nSizeX] == 1)
                    {
                        bundle.Add(new Workplace(index, new Point(x, y), new Point(x * nDiePitchX, y * nDiePitchY), new Size(nDiePitchX, nDiePitchY)));
                        index++;
                    }
                }

                // Bottom
                for (int y = nMasterY + 1; y < nSizeY; y++)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        bundle.Add(new Workplace(index, new Point(x, y), new Point(x * nDiePitchX, y * nDiePitchY), new Size(nDiePitchX, nDiePitchY)));
                        index++;
                    }
                }
            }


            // Left
            for (int x = nMasterX - 1; x >= 0; x--)
            {
                // Top
                for (int y = nMasterY; y >= 0; y--)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        bundle.Add(new Workplace(index, new Point(x, y), new Point(x * nDiePitchX, y * nDiePitchY), new Size(nDiePitchX, nDiePitchY)));
                        index++;
                    }
                }

                // Bottom
                for (int y = nMasterY + 1; y < nSizeY; y++)
                {
                    if (wafermap[x + y * nSizeX] == 1)
                    {
                        bundle.Add(new Workplace(index, new Point(x, y), new Point(x * nDiePitchX, y * nDiePitchY), new Size(nDiePitchX, nDiePitchY)));
                        index++;
                    }
                }
            }

            return bundle;
        }



        static public WorkplaceBundle CreateSingle(int index, Point subIndex, Point pos)
        {
            WorkplaceBundle bundle = new WorkplaceBundle();

            bundle.Add(new Workplace(index, subIndex, pos, new Size(0, 0)));

            return bundle;
        }
    }


}
