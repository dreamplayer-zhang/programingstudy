using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WindII
{
    enum eXMLData
    {
        RECIPE_NAME,
        DESCRIPTION,
        DEVICE,
        DIE_PITCH_X,
        DIE_PITCH_Y,
        SCRIBE_LINE_X,
        SCRIBE_LINE_Y,
        SHOT_X,
        SHOT_Y,
        MAP_OFFSET_X,
        MAP_OFFSET_Y,
        SHOT_OFFSET_X,
        SHOT_OFFSET_Y,
        SMI_OFFSET_X,
        SMI_OFFSET_Y,
        ROTATION,
        ORIGIN_DIE_X,
        ORIGIN_DIE_Y,
        EVEN_ODD,
        DIE_LIST
    }

    public class XMLData
    {
        #region [Variables]
        // Xml data
        public string RecipeName { get; set; }
        public string Description { get; set; }
        public int UnitX { get; set; }
        public int UnitY { get; set; }
        public string Device { get; set; }
        public double DiePitchX { get; set; }
        public double DiePitchY { get; set; }
        public double ScribeLineX { get; set; }
        public double ScribeLineY { get; set; }
        public int ShotX { get; set; }
        public int ShotY { get; set; }
        public double MapOffsetX { get; set; }
        public double MapOffsetY { get; set; }
        public double ShotOffsetX { get; set; }
        public double ShotOffsetY { get; set; }
        public double SMIOffsetX { get; set; }
        public double SMIOffsetY { get; set; }
        public int Rotation { get; set; }
        public int OriginDieX { get; set; }
        public int OriginDieY { get; set; }
        public string EvenOdd { get; set; }
        public List<Point> DieList { get; set; }
        public Point OriginDieUnit { get; set; }
        public Point DieMinXY { get; set; }
        public int[] MapData
        {
            get;
            set;
        }
        public int MapSizeX
        {
            get;
            set;
        }

        public int MapSizeY
        {
            get;
            set;
        }


        //Backside
        public double MapOffsetX_Backside { get; set; }
        public double ShotOffsetX_Backside { get; set; }
        public double SMIOffsetX_Backside { get; set; }

        #endregion

        public XMLData()
        {
            RecipeName = string.Empty;
            Description = string.Empty;
            Device = string.Empty;  // PartID
            DiePitchX = 0;
            DiePitchY = 0;
            ScribeLineX = 0;
            ScribeLineY = 0;
            ShotX = 0;
            ShotY = 0;
            MapOffsetX = 0f;
            MapOffsetY = 0f;
            ShotOffsetX = 0f;
            ShotOffsetY = 0f;
            SMIOffsetX = 0f;
            SMIOffsetY = 0f;
            Rotation = 0;
            OriginDieX = 0;
            OriginDieY = 0;
            EvenOdd = string.Empty;
            DieList = new List<Point>();

            MapOffsetX_Backside = 0f;
            ShotOffsetX_Backside = 0f;
            SMIOffsetX_Backside = 0f;
        }

        public Size GetUnitSize()
        {
            int minX = 0, maxX = 0;
            int minY = 0, maxY = 0;

            foreach (Point pt in DieList)
            {
                int valX = (int)(pt.X - OriginDieX);
                int valY = (int)(pt.Y - OriginDieY);

                if (minX > valX) minX = valX;
                if (maxX < valX) maxX = valX;

                if (minY > valY) minY = valY;
                if (maxY < valY) maxY = valY;
            }

            UnitX = maxX - minX + 1;
            UnitY = maxY - minY + 1;

            return new Size(maxX - minX + 1, maxY - minY + 1);
        }

        //public List<Point> GetUnitDieListCenter() // 중심 기준 좌표
        //{
        //    List<Point> dieList = new List<Point>();
        //    foreach(Point pt in DieList)
        //    {
        //        dieList.Add(new Point(pt.X - OriginDieX, pt.Y - OriginDieY));
        //    }

        //    return dieList;
        //}

        public List<Point> GetUnitDieList() // 좌상단 기준 좌표
        {
            List<Point> dieList = new List<Point>();

            int minX = OriginDieX, maxX = OriginDieX;
            int minY = OriginDieY, maxY = OriginDieY;

            foreach (Point pt in DieList)
            {
                int valX = (int)(pt.X);
                int valY = (int)(pt.Y);

                if (minX > valX) minX = valX;
                if (minY > valY) minY = valY;

                if (maxX < valX) maxX = valX;
                if (maxY < valY) maxY = valY;
            }

            foreach (Point pt in DieList)
            {
                dieList.Add(new Point(pt.X - minX, pt.Y - minY));
                if (pt.X == OriginDieX && pt.Y == OriginDieY)
                    OriginDieUnit = new Point(pt.X - minX, pt.Y - minY);
            }
            DieMinXY = new Point(minX, minY);
            return dieList;
        }


        // 좌상단 기준 OriginX/Y
        // ExclusionEdge
        public void GetOriginDieWithExclusionEdge(double edgeLen, ref int orgX, ref int orgY, bool IsBackside = false)
        {
            int minX = OriginDieX, minY = OriginDieY;
            int maxX = OriginDieX, maxY = OriginDieY;

            int minValidX = OriginDieX, minValidY = OriginDieY;
            int maxValidX = OriginDieX, maxValidY = OriginDieY;

            foreach (Point pt in DieList)
            {
                int valX = (int)(pt.X);
                int valY = (int)(pt.Y);

                if (minX > valX) minX = valX;
                if (minY > valY) minY = valY;

                if (maxX < valX) maxX = valX;
                if (maxY < valY) maxY = valY;
            }

            foreach (Point pt in DieList)
            {
                int valX = (int)(pt.X);
                int valY = (int)(pt.Y);

                double OffsetX = valX - OriginDieX < 0 ? -MapOffsetX : -(DiePitchX - MapOffsetX);
                double OffsetY = valY - OriginDieY < 0 ? -MapOffsetY : -(DiePitchY - MapOffsetY);

                double dX = Math.Abs(valX - OriginDieX) * DiePitchX + OffsetX;
                double dY = Math.Abs(pt.Y - OriginDieY) * DiePitchY + OffsetY;

                double r = Math.Sqrt(dX * dX + dY * dY);

                if (r <= (double)(150 - edgeLen) * 1000.0)
                {
                    if (minValidX > valX) minValidX = valX;
                    if (minValidY > valY) minValidY = valY;

                    if (maxValidX < valX) maxValidX = valX;
                    if (maxValidY < valY) maxValidY = valY;
                }
            }

            if (IsBackside == false)
            {
                orgX = OriginDieX - minValidX;
                orgY = OriginDieY - minValidY;
            }
            else
            {
                orgX = maxValidX - OriginDieX;
                orgY = maxValidX - OriginDieY;
            }
        }

        // 좌상단 기준 좌표
        // ExclusionEdge
        public void GetUnitDieListWithExclusionEdge(double edgeLen, ref List<Point> dieList, ref Size mapSize, ref List<Point> dieValidList, ref Size mapValidSize, ref List<Point> edgeList, bool bBackside = false)
        {
            int minX = OriginDieX, minY = OriginDieY;
            int maxX = OriginDieX, maxY = OriginDieY;

            int minValidX = OriginDieX, minValidY = OriginDieY;
            int maxValidX = OriginDieX, maxValidY = OriginDieY;

            foreach (Point pt in DieList)
            {
                int valX = (int)(pt.X);
                int valY = (int)(pt.Y);

                if (minX > valX) minX = valX;
                if (minY > valY) minY = valY;

                if (maxX < valX) maxX = valX;
                if (maxY < valY) maxY = valY;
            }

            foreach (Point pt in DieList)
            {
                int valX = (int)(pt.X);
                int valY = (int)(pt.Y);

                double OffsetX = valX - OriginDieX < 0 ? -MapOffsetX : -(DiePitchX - MapOffsetX);
                double OffsetY = valY - OriginDieY < 0 ? -MapOffsetY : -(DiePitchY - MapOffsetY);

                double dX = Math.Abs(valX - OriginDieX) * DiePitchX + OffsetX;
                double dY = Math.Abs(pt.Y - OriginDieY) * DiePitchY + OffsetY;

                double r = Math.Sqrt(dX * dX + dY * dY);

                dieList.Add(new Point(valX - minX, valY - minY));

                if (r > (double)(150 - edgeLen) * 1000.0)
                {
                    edgeList.Add(new Point(valX - minX, valY - minY));
                }
                else
                {
                    if (minValidX > valX) minValidX = valX;
                    if (minValidY > valY) minValidY = valY;

                    if (maxValidX < valX) maxValidX = valX;
                    if (maxValidY < valY) maxValidY = valY;

                    dieValidList.Add(new Point(valX, valY));
                }
            }

            for (int i = 0; i < dieValidList.Count; i++)
            {
                dieValidList[i] = new Point(dieValidList[i].X - minValidX, dieValidList[i].Y - minValidY);
            }

            mapSize.Width = maxX - minX + 1;
            mapSize.Height = maxY - minY + 1;

            mapValidSize.Width = maxValidX - minValidX + 1;
            mapValidSize.Height = maxValidY - minValidY + 1;

            if (bBackside == true)
            {
                for (int i = 0; i < edgeList.Count; i++)
                    edgeList[i] = new Point(maxX - minX - edgeList[i].X, edgeList[i].Y);

                for (int i = 0; i < dieList.Count; i++)
                    dieList[i] = new Point(maxX - minX - dieList[i].X, dieList[i].Y);

                for (int i = 0; i < dieValidList.Count; i++)
                    dieValidList[i] = new Point(maxValidX - minValidX - dieValidList[i].X, dieValidList[i].Y);
            }
        }

        public static int[] MakeWaferMap(List<Point> dieList, Size mapSize)
        {
            if (mapSize.Width * mapSize.Height > 1000000) return null;

            int[] map = new int[(int)(mapSize.Width * mapSize.Height)];

            foreach (Point pt in dieList)
            {
                map[(int)(pt.Y * mapSize.Width + pt.X)] = 1;
            }

            //if(bBackside == false)
            //{
            //    foreach (Point pt in dieList)
            //    {
            //        map[(int)(pt.Y * mapSize.Width + pt.X)] = 1;
            //    }
            //}
            //else
            //{
            //    foreach (Point pt in dieList)
            //    {
            //        map[(int)(pt.Y * mapSize.Width + (mapSize.Width - pt.X - 1))] = 1;
            //    }
            //}

            return map;
        }
        //public static int[] MakeReversWaferMap(List<Point> dieList, Size mapSize)
        //{
        //    int[] map = new int[(int)(mapSize.Width * mapSize.Height)];

        //    foreach (Point Pt in dieList)
        //    {
        //        map[(int)(Pt.Y * mapSize.Width - Pt.X - 1)] = 1;
        //    }
        //    return map;
        //}

        public double[] GetWaferMap()
        {
            Size unitSize = GetUnitSize();

            double[] map = new double[(int)(unitSize.Width * unitSize.Height)];
            List<Point> dieList = GetUnitDieList();

            foreach (Point pt in dieList)
            {
                map[(int)(pt.Y * unitSize.Width + pt.X)] = 1;
            }
            return map;
        }
    }
}

