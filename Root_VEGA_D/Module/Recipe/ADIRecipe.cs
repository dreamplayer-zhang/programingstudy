using RootTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_D.Module.Recipe
{
    public class ADIRecipe
    {
        public string RecipeFilePath { get; private set; }

        public Size MainImageSize { get; set; }
        public int nLeft { get; set; }
        public int nTop { get; set; }
        public int nRight { get; set; }
        public int nBottom { get; set; }
        public CPoint nAlignPos { get; set; }

        public CPoint nDebug_Input_AlignPos { get; set; }

        public int nFirstDieLeft { get; set; }
        public int nFirstDieRight { get; set; }
        public int nSecondDieLeft { get; set; }
        public int nLastDieRight { get; set; }
        public int nFirstDieBottom { get; set; }
        public int nFirstDieTop { get; set; }
        public int nSecondDieBottom { get; set; }
        public int nLastDieTop { get; set; }
        public Size TriggerSize { get; set; }
        public Size RectSize { get; set; }
        public int nPadding { get; set; }

        public bool bAfterEfectMorpology { get; set; }
        public bool bAfterEfectBright { get; set; }
        public bool bAfterEfectHistogram { get; set; }
        public bool bConsiderLinear_posY { get; set; }
        public bool bLoadScore { get; set; }
        public string sDefectImagePath { get; set; }
        public int nD2DThresholdGV { get; set; }
        public int nSurfaceGV_Min { get; set; }
        public int nSurfaceGV_Max { get; set; }
        public int nDefectImagePadding { get; set; }
        public Size DefectSize { get; set; }
        public int nMergeLength { get; set; }
        public int nSwatheLength { get; set; }
        public int nSwatheIgnore { get; set; }
        public int nSwathePadding { get; set; }
        public int nStartSwathe { get; set; }
        public int nSwatheMaxCount { get; set; }

        public ADIRecipe()
        {
        }
        public bool Save(string savePath = "")
        {
#if !DEBUG
			try
			{
#endif
            StringBuilder stbr = new StringBuilder();

            stbr.Append("MainImageSize.x,");
            stbr.AppendLine(MainImageSize.Width.ToString());
            stbr.Append("MainImageSize.y,");
            stbr.AppendLine(MainImageSize.Height.ToString());
            stbr.Append("nLeft,");
            stbr.AppendLine(nLeft.ToString());
            stbr.Append("nTop,");
            stbr.AppendLine(nTop.ToString());
            stbr.Append("nRight,");
            stbr.AppendLine(nRight.ToString());
            stbr.Append("nBottom,");
            stbr.AppendLine(nBottom.ToString());
            stbr.Append("nAlignPosX,");
            stbr.AppendLine(nAlignPos.X.ToString());
            stbr.Append("nAlignPosY,");
            stbr.AppendLine(nAlignPos.Y.ToString());

            stbr.AppendLine(",");

            stbr.Append("nDebug_Input_AlignPosX,");
            stbr.AppendLine(nDebug_Input_AlignPos.X.ToString());
            stbr.Append("nDebug_Input_AlignPosY,");
            stbr.AppendLine(nDebug_Input_AlignPos.Y.ToString());

            stbr.AppendLine(",");

            stbr.Append("nFirstDieLeft,");
            stbr.AppendLine(nFirstDieLeft.ToString());
            stbr.Append("nFirstDieRight,");
            stbr.AppendLine(nFirstDieRight.ToString());
            stbr.Append("nSecondDieLeft,");
            stbr.AppendLine(nSecondDieLeft.ToString());
            stbr.Append("nLastDieRight,");
            stbr.AppendLine(nLastDieRight.ToString());
            stbr.Append("nFirstDieBottom,");
            stbr.AppendLine(nFirstDieBottom.ToString());
            stbr.Append("nFirstDieTop,");
            stbr.AppendLine(nFirstDieTop.ToString());
            stbr.Append("nSecondDieBottom,");
            stbr.AppendLine(nSecondDieBottom.ToString());
            stbr.Append("nLastDieTop,");
            stbr.AppendLine(nLastDieTop.ToString());
            stbr.Append("TriggerSize.x,");
            stbr.AppendLine(TriggerSize.Width.ToString());
            stbr.Append("TriggerSize.y,");
            stbr.AppendLine(TriggerSize.Height.ToString());
            stbr.Append("RectSize.x,");
            stbr.AppendLine(RectSize.Width.ToString());
            stbr.Append("RectSize.y,");
            stbr.AppendLine(RectSize.Height.ToString());
            stbr.Append("nPadding,");
            stbr.AppendLine(nPadding.ToString());
            stbr.Append("bAfterEfectMorpology,");
            stbr.AppendLine(bAfterEfectMorpology.ToString());
            stbr.Append("bAfterEfectBright,");
            stbr.AppendLine(bAfterEfectBright.ToString());
            stbr.Append("bAfterEfectHistogram,");
            stbr.AppendLine(bAfterEfectHistogram.ToString());
            stbr.Append("bConsiderLinear_posY,");
            stbr.AppendLine(bConsiderLinear_posY.ToString());
            stbr.Append("bLoadScore,");
            stbr.AppendLine(bLoadScore.ToString());
            stbr.Append("sDefectImagePath,");
            stbr.AppendLine(sDefectImagePath.ToString());
            stbr.Append("nD2DThresholdGV,");
            stbr.AppendLine(nD2DThresholdGV.ToString());
            stbr.Append("nSurfaceGV_Min,");
            stbr.AppendLine(nSurfaceGV_Min.ToString());
            stbr.Append("nSurfaceGV_Max,");
            stbr.AppendLine(nSurfaceGV_Max.ToString());
            stbr.Append("nDefectImagePadding,");
            stbr.AppendLine(nDefectImagePadding.ToString());
            stbr.Append("DefectSize.x,");
            stbr.AppendLine(DefectSize.Width.ToString());
            stbr.Append("DefectSize.y,");
            stbr.AppendLine(DefectSize.Height.ToString());
            stbr.Append("nMergeLength,");
            stbr.AppendLine(nMergeLength.ToString());
            stbr.Append("nSwatheLength,");
            stbr.AppendLine(nSwatheLength.ToString());
            stbr.Append("nSwatheIgnore,");
            stbr.AppendLine(nSwatheIgnore.ToString());
            stbr.Append("nSwathePadding,");
            stbr.AppendLine(nSwathePadding.ToString());
            stbr.Append("nStartSwathe,");
            stbr.AppendLine(nStartSwathe.ToString());
            stbr.Append("nSwatheMaxCount,");
            stbr.Append(nSwatheMaxCount.ToString());

            if (savePath == "")
            {
                File.WriteAllText(RecipeFilePath, stbr.ToString());
            }
            else
            {
                File.WriteAllText(savePath, stbr.ToString());
            }

#if !DEBUG
			}
			catch (Exception ex)
			{
				return false;
			}

#endif
            return true;
        }
        public static ADIRecipe Load(string filePath)
        {
            ADIRecipe recipe = new ADIRecipe();
            recipe.RecipeFilePath = filePath;

            var lines = File.ReadAllLines(filePath).Where(x => x != "");

            recipe.MainImageSize = new Size(Convert.ToDouble(lines.Where(x => x.Contains("MainImageSize.x")).First().Split(',')[1]), Convert.ToDouble(lines.Where(x => x.Contains("MainImageSize.y")).First().Split(',')[1]));
            recipe.nLeft = Convert.ToInt32(lines.Where(x => x.Contains("nLeft")).First().Split(',')[1]);
            recipe.nTop = Convert.ToInt32(lines.Where(x => x.Contains("nTop")).First().Split(',')[1]);
            recipe.nRight = Convert.ToInt32(lines.Where(x => x.Contains("nRight")).First().Split(',')[1]);
            recipe.nBottom = Convert.ToInt32(lines.Where(x => x.Contains("nBottom")).First().Split(',')[1]);
            recipe.nAlignPos = new CPoint(Convert.ToInt32(lines.Where(x => x.Contains("nAlignPosX")).First().Split(',')[1]), Convert.ToInt32(lines.Where(x => x.Contains("nAlignPosY")).First().Split(',')[1]));

            recipe.nDebug_Input_AlignPos = new CPoint(Convert.ToInt32(lines.Where(x => x.Contains("nDebug_Input_AlignPosX")).First().Split(',')[1]), Convert.ToInt32(lines.Where(x => x.Contains("nDebug_Input_AlignPosY")).First().Split(',')[1]));

            recipe.nFirstDieLeft = Convert.ToInt32(lines.Where(x => x.Contains("nFirstDieLeft")).First().Split(',')[1]);
            recipe.nFirstDieRight = Convert.ToInt32(lines.Where(x => x.Contains("nFirstDieRight")).First().Split(',')[1]);
            recipe.nSecondDieLeft = Convert.ToInt32(lines.Where(x => x.Contains("nSecondDieLeft")).First().Split(',')[1]);
            recipe.nLastDieRight = Convert.ToInt32(lines.Where(x => x.Contains("nLastDieRight")).First().Split(',')[1]);
            recipe.nFirstDieBottom = Convert.ToInt32(lines.Where(x => x.Contains("nFirstDieBottom")).First().Split(',')[1]);
            recipe.nFirstDieTop = Convert.ToInt32(lines.Where(x => x.Contains("nFirstDieTop")).First().Split(',')[1]);
            recipe.nSecondDieBottom = Convert.ToInt32(lines.Where(x => x.Contains("nSecondDieBottom")).First().Split(',')[1]);
            recipe.nLastDieTop = Convert.ToInt32(lines.Where(x => x.Contains("nLastDieTop")).First().Split(',')[1]);
            recipe.TriggerSize = new Size(Convert.ToDouble(lines.Where(x => x.Contains("TriggerSize.x")).First().Split(',')[1]), Convert.ToDouble(lines.Where(x => x.Contains("TriggerSize.y")).First().Split(',')[1]));
            recipe.RectSize = new Size(Convert.ToDouble(lines.Where(x => x.Contains("RectSize.x")).First().Split(',')[1]), Convert.ToDouble(lines.Where(x => x.Contains("RectSize.y")).First().Split(',')[1]));
            recipe.nPadding = Convert.ToInt32(lines.Where(x => x.Contains("nPadding")).First().Split(',')[1]);
            recipe.bAfterEfectMorpology = Convert.ToBoolean(lines.Where(x => x.Contains("bAfterEfectMorpology")).First().Split(',')[1]);
            recipe.bAfterEfectBright = Convert.ToBoolean(lines.Where(x => x.Contains("bAfterEfectBright")).First().Split(',')[1]);
            recipe.bAfterEfectHistogram = Convert.ToBoolean(lines.Where(x => x.Contains("bAfterEfectHistogram")).First().Split(',')[1]);
            recipe.bConsiderLinear_posY = Convert.ToBoolean(lines.Where(x => x.Contains("bConsiderLinear_posY")).First().Split(',')[1]);
            recipe.bLoadScore = Convert.ToBoolean(lines.Where(x => x.Contains("bLoadScore")).First().Split(',')[1]);
            recipe.sDefectImagePath = lines.Where(x => x.Contains("sDefectImagePath")).First().Split(',')[1];
            recipe.nD2DThresholdGV = Convert.ToInt32(lines.Where(x => x.Contains("nD2DThresholdGV")).First().Split(',')[1]);
            recipe.nSurfaceGV_Min = Convert.ToInt32(lines.Where(x => x.Contains("nSurfaceGV_Min")).First().Split(',')[1]);
            recipe.nSurfaceGV_Max = Convert.ToInt32(lines.Where(x => x.Contains("nSurfaceGV_Max")).First().Split(',')[1]);
            recipe.nDefectImagePadding = Convert.ToInt32(lines.Where(x => x.Contains("nDefectImagePadding")).First().Split(',')[1]);
            recipe.DefectSize = new Size(Convert.ToDouble(lines.Where(x => x.Contains("DefectSize.x")).First().Split(',')[1]), Convert.ToDouble(lines.Where(x => x.Contains("DefectSize.y")).First().Split(',')[1]));
            recipe.nMergeLength = Convert.ToInt32(lines.Where(x => x.Contains("nMergeLength")).First().Split(',')[1]);
            recipe.nSwatheLength = Convert.ToInt32(lines.Where(x => x.Contains("nSwatheLength")).First().Split(',')[1]);
            recipe.nSwatheIgnore = Convert.ToInt32(lines.Where(x => x.Contains("nSwatheIgnore")).First().Split(',')[1]);
            recipe.nSwathePadding = Convert.ToInt32(lines.Where(x => x.Contains("nSwathePadding")).First().Split(',')[1]);
            recipe.nStartSwathe = Convert.ToInt32(lines.Where(x => x.Contains("nStartSwathe")).First().Split(',')[1]);
            recipe.nSwatheMaxCount = Convert.ToInt32(lines.Where(x => x.Contains("nSwatheMaxCount")).First().Split(',')[1]);

            return recipe;
        }
    }
}