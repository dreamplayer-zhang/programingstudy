using Root_CAMELLIA.ShapeDraw;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Root_CAMELLIA
{
    public static class GeneralTools
    {
        public static SolidColorBrush ActiveBrush { get; } = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
        public static SolidColorBrush GuideLineBrush { get; } = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        public static SolidColorBrush GuideCustomLineBrush { get; } = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        public static SolidColorBrush StageShadeBrush { get; } = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));
        public static SolidColorBrush SelectPointBrush { get; } = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));
        public static SolidColorBrush StageHoleBrush { get; } = new SolidColorBrush(Color.FromArgb(64, 128, 128, 128));
        public static SolidColorBrush SelectedOverBrush { get; } = new SolidColorBrush(Color.FromArgb(255, 255, 255, 128));
        public static SolidColorBrush SelectBrush { get; } = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));

        public static SolidColorBrush CustomSelectBrush { get; } = new SolidColorBrush(Color.FromArgb(255, 75, 137, 220));

        public static SolidColorBrush CustomCandidateBrush { get; } = new SolidColorBrush(Color.FromArgb(255, 246, 187, 67));

        public static RadialGradientBrush Gb { get; } = new RadialGradientBrush(
           new GradientStopCollection() { new GradientStop(new SolidColorBrush(Color.FromArgb(255, 130, 130, 130)).Color, 0.3),
                    new GradientStop(new SolidColorBrush(Color.FromArgb(255, 110, 110, 110)).Color, 0.6),
                    new GradientStop(new SolidColorBrush(Color.FromArgb(255, 90, 90, 90)).Color, 1)});

        public static RadialGradientBrush GbHole { get; } = new RadialGradientBrush(
            new GradientStopCollection() {
                 new GradientStop(new SolidColorBrush(Color.FromArgb(255, 255, 200, 200)).Color, 0.1),
                new GradientStop(new SolidColorBrush(Color.FromArgb(255, 239, 59, 54)).Color, 0.3),
                    new GradientStop(new SolidColorBrush(Color.FromArgb(255, 255, 125, 125)).Color, 1)});
        public static RadialGradientBrush GbSelect { get; set; } = new RadialGradientBrush(
          new GradientStopCollection() {
                 new GradientStop(new SolidColorBrush(Color.FromArgb(255, 0, 210, 255)).Color, 0.2),

                    new GradientStop(new SolidColorBrush(Color.FromArgb(255, 58, 167, 213)).Color, 1)});


        public static Circle DataStageField { get; } = new Circle();
        public static ShapeDraw.Line DataStageLineHole { get; } = new ShapeDraw.Line();
        public static System.Drawing.PointF[] DataStageEdgeHolePoint { get; } = new System.Drawing.PointF[64];
        public static Arc[] DataStageEdgeHoleArc { get; } = new Arc[8];
        public static Circle[] DataStageGuideLine { get; } = new Circle[4];
        public static Arc[] DataStageDoubleHoleArc { get; } = new Arc[8];
        public static Arc[] DataStageTopHoleArc { get; } = new Arc[2];
        public static Arc[] DataStageBotHoleArc { get; } = new Arc[2];

        public static Circle DataStageGuideField { get; } = new Circle();
        public static ShapeDraw.Line DataStageGuideLineHole { get; } = new ShapeDraw.Line();
        public static System.Drawing.PointF[] DataStageGuideEdgeHolePoint { get; } = new System.Drawing.PointF[64];
        public static Arc[] DataStageGuideEdgeHoleArc { get; } = new Arc[8];
        public static Arc[] DataStageGuideDoubleHoleArc { get; } = new Arc[8];
        public static Arc[] DataStageGuideTopHoleArc { get; } = new Arc[2];
        public static Arc[] DataStageGuideBotHoleArc { get; } = new Arc[2];

        public static int ArcPointNum { get; } = 63;
        public static int EdgeNum { get; } = 4;
        public static int DoubleHoleNum { get; } = 4;
        public static int GuideLineNum { get; } = 4;

        public static void MakeDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        static GeneralTools()
        {
            DataStageField.Set(0, 0, BaseDefine.ViewSize, BaseDefine.ViewSize);
            DataStageLineHole.Set(0, 0, BaseDefine.ViewSize, 7);

            double dRadiusIn = 130;
            double dRadiusOut = 155;
            DataStageEdgeHolePoint[0] = new System.Drawing.PointF((float)15, (float)Math.Sqrt(16675));
            DataStageEdgeHolePoint[1] = new System.Drawing.PointF((float)Math.Sqrt(16347.75), (float)23.5);
            DataStageEdgeHolePoint[2] = new System.Drawing.PointF((float)Math.Sqrt(23472.75), (float)23.5);
            DataStageEdgeHolePoint[3] = new System.Drawing.PointF((float)15, (float)Math.Sqrt(23800));
            DataStageEdgeHolePoint[4] = new System.Drawing.PointF((float)Math.Sqrt(16347.75), (float)-23.5);
            DataStageEdgeHolePoint[5] = new System.Drawing.PointF((float)15, (float)-Math.Sqrt(16675));
            DataStageEdgeHolePoint[6] = new System.Drawing.PointF((float)15, (float)-Math.Sqrt(23800));
            DataStageEdgeHolePoint[7] = new System.Drawing.PointF((float)Math.Sqrt(23472.75), (float)-23.5);
            DataStageEdgeHolePoint[8] = new System.Drawing.PointF((float)-15, (float)-Math.Sqrt(16675));
            DataStageEdgeHolePoint[9] = new System.Drawing.PointF((float)-Math.Sqrt(16347.75), (float)-23.5);
            DataStageEdgeHolePoint[10] = new System.Drawing.PointF((float)-Math.Sqrt(23472.75), (float)-23.5);
            DataStageEdgeHolePoint[11] = new System.Drawing.PointF((float)-15, (float)-Math.Sqrt(23800));
            DataStageEdgeHolePoint[12] = new System.Drawing.PointF((float)-Math.Sqrt(16347.75), (float)23.5);
            DataStageEdgeHolePoint[13] = new System.Drawing.PointF((float)-15, (float)Math.Sqrt(16675));
            DataStageEdgeHolePoint[14] = new System.Drawing.PointF((float)-15, (float)Math.Sqrt(23800));
            DataStageEdgeHolePoint[15] = new System.Drawing.PointF((float)-Math.Sqrt(23472.75), (float)23.5);

            for (int i = 0; i < EdgeNum; i++)
            {
                DataStageEdgeHoleArc[2 * i + 0] = new Arc(0, 0, dRadiusIn, Math.Atan2(DataStageEdgeHolePoint[4 * i + 0].Y, DataStageEdgeHolePoint[4 * i + 0].X), Math.Atan2(DataStageEdgeHolePoint[4 * i + 1].Y, DataStageEdgeHolePoint[4 * i + 1].X), ArcPointNum, false);
                DataStageEdgeHoleArc[2 * i + 1] = new Arc(0, 0, dRadiusOut, Math.Atan2(DataStageEdgeHolePoint[4 * i + 2].Y, DataStageEdgeHolePoint[4 * i + 2].X), Math.Atan2(DataStageEdgeHolePoint[4 * i + 3].Y, DataStageEdgeHolePoint[4 * i + 3].X), ArcPointNum, false);
            }

            double dRadiusHole = 6;
            double dInLength = 69.3;
            double dOutLength = 77.85;
            for (int i = 0; i < 2 * DoubleHoleNum; i++)
            {
                DataStageDoubleHoleArc[i] = new Arc();
            }
            DataStageDoubleHoleArc[0].InitArc(dInLength, dInLength, dRadiusHole, (3 / (float)4) * Math.PI, (7 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageDoubleHoleArc[1].InitArc(dOutLength, dOutLength, dRadiusHole, (7 / (float)4) * Math.PI, (3 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageDoubleHoleArc[2].InitArc(dInLength, -dInLength, dRadiusHole, (1 / (float)4) * Math.PI, (5 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageDoubleHoleArc[3].InitArc(dOutLength, -dOutLength, dRadiusHole, (5 / (float)4) * Math.PI, (1 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageDoubleHoleArc[4].InitArc(-dInLength, -dInLength, dRadiusHole, (7 / (float)4) * Math.PI, (3 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageDoubleHoleArc[5].InitArc(-dOutLength, -dOutLength, dRadiusHole, (3 / (float)4) * Math.PI, (7 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageDoubleHoleArc[6].InitArc(-dInLength, dInLength, dRadiusHole, (5 / (float)4) * Math.PI, (1 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageDoubleHoleArc[7].InitArc(-dOutLength, dOutLength, dRadiusHole, (1 / (float)4) * Math.PI, (5 / (float)4) * Math.PI, ArcPointNum, true);


            DataStageTopHoleArc[0] = new Arc(0, 145, dRadiusHole, Math.PI, 0, ArcPointNum, true);
            DataStageTopHoleArc[1] = new Arc(0, 0, dRadiusOut, Math.Atan2(Math.Sqrt(23989), 6), Math.Atan2(Math.Sqrt(23989), -6), ArcPointNum, true);
            DataStageBotHoleArc[0] = new Arc(0, -145, dRadiusHole, 0, Math.PI, ArcPointNum, true);
            DataStageBotHoleArc[1] = new Arc(0, 0, dRadiusOut, Math.Atan2(-Math.Sqrt(23989), -6), Math.Atan2(-Math.Sqrt(23989), 6), ArcPointNum, true);


            for (int i = 0; i < GuideLineNum; i++)
            {
                DataStageGuideLine[i] = new Circle();

            }
            DataStageGuideLine[0].Set(0, 0, 49, 49);
            DataStageGuideLine[1].Set(0, 0, 98, 98);
            DataStageGuideLine[2].Set(0, 0, 150, 150);
            DataStageGuideLine[3].Set(0, 0, 196, 196);



            DataStageGuideLineHole.Set(0, 0, BaseDefine.ViewSize - 2, 5);

            dRadiusIn = 131;
            dRadiusOut = 154;

            DataStageGuideEdgeHolePoint[0] = new System.Drawing.PointF((float)16, (float)Math.Sqrt(16905));
            DataStageGuideEdgeHolePoint[1] = new System.Drawing.PointF((float)Math.Sqrt(16560.75), (float)24.5);
            DataStageGuideEdgeHolePoint[2] = new System.Drawing.PointF((float)Math.Sqrt(23115.75), (float)24.5);
            DataStageGuideEdgeHolePoint[3] = new System.Drawing.PointF((float)16, (float)Math.Sqrt(23460));
            DataStageGuideEdgeHolePoint[4] = new System.Drawing.PointF((float)Math.Sqrt(16560.75), (float)-24.5);
            DataStageGuideEdgeHolePoint[5] = new System.Drawing.PointF((float)16, (float)-Math.Sqrt(16905));
            DataStageGuideEdgeHolePoint[6] = new System.Drawing.PointF((float)16, (float)-Math.Sqrt(23460));
            DataStageGuideEdgeHolePoint[7] = new System.Drawing.PointF((float)Math.Sqrt(23115.75), (float)-24.5);
            DataStageGuideEdgeHolePoint[8] = new System.Drawing.PointF((float)-16, (float)-Math.Sqrt(16905));
            DataStageGuideEdgeHolePoint[9] = new System.Drawing.PointF((float)-Math.Sqrt(16560.75), (float)-24.5);
            DataStageGuideEdgeHolePoint[10] = new System.Drawing.PointF((float)-Math.Sqrt(23115.75), (float)-24.5);
            DataStageGuideEdgeHolePoint[11] = new System.Drawing.PointF((float)-16, (float)-Math.Sqrt(23460));
            DataStageGuideEdgeHolePoint[12] = new System.Drawing.PointF((float)-Math.Sqrt(16560.75), (float)24.5);
            DataStageGuideEdgeHolePoint[13] = new System.Drawing.PointF((float)-16, (float)Math.Sqrt(16905));
            DataStageGuideEdgeHolePoint[14] = new System.Drawing.PointF((float)-16, (float)Math.Sqrt(23460));
            DataStageGuideEdgeHolePoint[15] = new System.Drawing.PointF((float)-Math.Sqrt(23115.75), (float)24.5);

            for (int i = 0; i < EdgeNum; i++)
            {
                DataStageGuideEdgeHoleArc[2 * i + 0] = new Arc(0, 0, dRadiusIn, Math.Atan2(DataStageGuideEdgeHolePoint[4 * i + 0].Y, DataStageGuideEdgeHolePoint[4 * i + 0].X), Math.Atan2(DataStageGuideEdgeHolePoint[4 * i + 1].Y, DataStageGuideEdgeHolePoint[4 * i + 1].X), ArcPointNum, false);
                DataStageGuideEdgeHoleArc[2 * i + 1] = new Arc(0, 0, dRadiusOut, Math.Atan2(DataStageGuideEdgeHolePoint[4 * i + 2].Y, DataStageGuideEdgeHolePoint[4 * i + 2].X), Math.Atan2(DataStageGuideEdgeHolePoint[4 * i + 3].Y, DataStageGuideEdgeHolePoint[4 * i + 3].X), ArcPointNum, false);
            }


            dRadiusHole = 5;

            for (int i = 0; i < 2 * DoubleHoleNum; i++)
            {
                DataStageGuideDoubleHoleArc[i] = new Arc();
            }
            DataStageGuideDoubleHoleArc[0].InitArc(dInLength, dInLength, dRadiusHole, (3 / (float)4) * Math.PI, (7 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageGuideDoubleHoleArc[1].InitArc(dOutLength, dOutLength, dRadiusHole, (7 / (float)4) * Math.PI, (3 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageGuideDoubleHoleArc[2].InitArc(dInLength, -dInLength, dRadiusHole, (1 / (float)4) * Math.PI, (5 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageGuideDoubleHoleArc[3].InitArc(dOutLength, -dOutLength, dRadiusHole, (5 / (float)4) * Math.PI, (1 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageGuideDoubleHoleArc[4].InitArc(-dInLength, -dInLength, dRadiusHole, (7 / (float)4) * Math.PI, (3 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageGuideDoubleHoleArc[5].InitArc(-dOutLength, -dOutLength, dRadiusHole, (3 / (float)4) * Math.PI, (7 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageGuideDoubleHoleArc[6].InitArc(-dInLength, dInLength, dRadiusHole, (5 / (float)4) * Math.PI, (1 / (float)4) * Math.PI, ArcPointNum, true);
            DataStageGuideDoubleHoleArc[7].InitArc(-dOutLength, dOutLength, dRadiusHole, (1 / (float)4) * Math.PI, (5 / (float)4) * Math.PI, ArcPointNum, true);

            DataStageGuideTopHoleArc[0] = new Arc(0, 145, dRadiusHole, Math.PI, 0, ArcPointNum, true);
            DataStageGuideTopHoleArc[1] = new Arc(0, 0, dRadiusOut, Math.Atan2(Math.Sqrt(23989), 5), Math.Atan2(Math.Sqrt(23989), -5), ArcPointNum, true);
            DataStageGuideBotHoleArc[0] = new Arc(0, -145, dRadiusHole, 0, Math.PI, ArcPointNum, true);
            DataStageGuideBotHoleArc[1] = new Arc(0, 0, dRadiusOut, Math.Atan2(-Math.Sqrt(23989), -5), Math.Atan2(-Math.Sqrt(23989), 5), ArcPointNum, true);
        }

        public static string SHA256Hash(string data)
        {
            SHA256 sha = new SHA256Managed();
            byte[] hash = sha.ComputeHash(Encoding.ASCII.GetBytes(data));
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in hash)
            {
                stringBuilder.AppendFormat("{0:x2}", b);
            }
            return stringBuilder.ToString();
        }
    }
}
