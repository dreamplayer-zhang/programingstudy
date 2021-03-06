using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace RootTools_Vision
{
    /// <summary>
    /// BoundingBox는 생성할 때 자동으로 계산하기 때문에 RecipeType_Mask를 수정하는 경우는 고려하지않고 무조건 새로 생성해야함
    /// 수정을 불가능하게 하여 데이터의 무결성을 유지
    /// </summary>
    public class RecipeType_Mask
    {
        private long area;
        private CRect boundingBox;
        private List<RecipeType_PointLine> pointLines;
        private Color colorIndex = Colors.AliceBlue;

        [XmlIgnore]
        public CRect BoundingBox
        {
            get => boundingBox;
            private set => boundingBox = value;
        }

        [XmlArray("PointLines")]
        [XmlArrayItem("PointLine")]
        public List<RecipeType_PointLine> PointLines
        {
            get => pointLines;
            set
            {
                Area = 0;
                pointLines = new List<RecipeType_PointLine>();
                boundingBox = new CRect();

                int minX = int.MaxValue;
                int minY = int.MaxValue;
                int maxX = int.MinValue;
                int maxY = int.MinValue;

                foreach (RecipeType_PointLine pl in value)
                {
                    pointLines.Add(new RecipeType_PointLine(pl.StartPoint, pl.Length));

                    minX = minX > pl.StartPoint.X ? pl.StartPoint.X : minX;
                    minY = minY > pl.StartPoint.Y ? pl.StartPoint.Y : minY;
                    maxX = maxX < pl.StartPoint.X + pl.Length ? pl.StartPoint.X + pl.Length : maxX;
                    maxY = maxY < pl.StartPoint.Y ? pl.StartPoint.Y : maxY;

                    Area += pl.Length;
                }

                boundingBox.Left = minX;
                boundingBox.Top = minY;
                boundingBox.Right = maxX;
                boundingBox.Bottom = maxY;
            }
        }

        public Color ColorIndex
        {
            get => this.colorIndex;
            set
            {
                this.colorIndex = value;
            }
        }

        [XmlIgnore]
        public long Area
        {
            get
            {
                return area;
            }

            set
            {
                area = value;
            }
        }

        public RecipeType_Mask()
        {
            area = 0;
            pointLines = new List<RecipeType_PointLine>();
            boundingBox = new CRect();
        }

        public RecipeType_Mask(List<RecipeType_PointLine> _pointLines)
        {
            Area = 0;
            pointLines = new List<RecipeType_PointLine>();
            boundingBox = new CRect();

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            foreach(RecipeType_PointLine pl in _pointLines)
            {
                pointLines.Add(new RecipeType_PointLine(pl.StartPoint, pl.Length));

                minX = minX > pl.StartPoint.X ? pl.StartPoint.X : minX;
                minY = minY > pl.StartPoint.Y ? pl.StartPoint.Y : minY;
                maxX = maxX < pl.StartPoint.X + pl.Length ? pl.StartPoint.X + pl.Length : maxX;
                maxY = maxY < pl.StartPoint.Y ? pl.StartPoint.Y : maxY;

                Area += pl.Length;
            }

            boundingBox.Left = minX;
            boundingBox.Top = minY;
            boundingBox.Right = maxX;
            boundingBox.Bottom = maxY;
        }

        public RecipeType_Mask(List<PointLine> _pointLines, Color _colorIndex)
        {
            this.colorIndex = _colorIndex;

            Area = 0;
            pointLines = new List<RecipeType_PointLine>();
            boundingBox = new CRect();

            int minX = int.MaxValue;
            int minY = int.MaxValue;
            int maxX = int.MinValue;
            int maxY = int.MinValue;
            foreach (PointLine pl in _pointLines)
            {
                pointLines.Add(new RecipeType_PointLine(pl.StartPt, pl.Width));

                minX = minX > pl.StartPt.X ? pl.StartPt.X : minX;
                minY = minY > pl.StartPt.Y ? pl.StartPt.Y : minY;
                maxX = maxX < pl.StartPt.X + pl.Width ? pl.StartPt.X + pl.Width : maxX;
                maxY = maxY < pl.StartPt.Y ? pl.StartPt.Y : maxY;

                Area += pl.Width;
            }

            boundingBox.Left = minX;
            boundingBox.Top = minY;
            boundingBox.Right = maxX;
            boundingBox.Bottom = maxY;
        }

        public void CopyPointLinesTo(ref List<PointLine> _pointLines)
        {
            _pointLines.Clear();
            foreach(RecipeType_PointLine pl in this.pointLines)
            {
                _pointLines.Add(new PointLine(pl.StartPoint, pl.Length));
            }
        }

        public List<PointLine> ToPointLineList()
        {
            List<PointLine> _pointLines = new List<PointLine>();
            foreach (RecipeType_PointLine pl in this.pointLines)
            {
                _pointLines.Add(new PointLine(pl.StartPoint, pl.Length));
            }

            return _pointLines;
        }
    }

    public class RecipeType_PointLine
    {
        private CPoint startPoint;
        private int length;

        public CPoint StartPoint
        {
            get => startPoint;
            set => startPoint = value;
        }
        public int Length
        {
            get => length;
            set => length = value;
        }

        public RecipeType_PointLine()
        {

        }

        public RecipeType_PointLine(CPoint _startPoint, int _length)
        {
            this.startPoint = _startPoint;
            this.length = _length;
        }

        public RecipeType_PointLine(RecipeType_PointLine _pointLine)
        {
            this.startPoint = _pointLine.StartPoint;
            this.length = _pointLine.Length;
        }

        public RecipeType_PointLine(PointLine _pointLine)
        {
            this.startPoint = _pointLine.StartPt;
            this.length = _pointLine.Width;
        }

        public PointLine ToPointLine()
        {
            return new PointLine(this.startPoint, this.Length);
        }
    }
}



