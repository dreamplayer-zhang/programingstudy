using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RootTools_Vision.WorkManager3
{
    public class RecipeToWorkplaceConverter
    {
        public static ConcurrentQueue<Workplace> ConvertToQueue(RecipeBase recipe, SharedBufferInfo bufferInfo, CameraInfo cameraInfo = new CameraInfo())
        {
            RecipeType_WaferMap waferMap = recipe.WaferMap;
            OriginRecipe originRecipe = recipe.GetItem<OriginRecipe>();
            bool useExclusiveRegion = recipe.UseExclusiveRegion;
            PathGeometry exRegion = null;
            if(useExclusiveRegion == true)
            {
                exRegion = PolygonController.CreatePolygonGeometry(PolygonController.ReadPolygonFile(recipe.ExclusiveRegionFilePath));
            }

            ConcurrentQueue<Workplace> queue = new ConcurrentQueue<Workplace>();
            WorkplaceBundle bundle = new WorkplaceBundle();

            try
            {
                var mapData = waferMap.Data;
                int mapSizeX = waferMap.MapSizeX;
                int mapSizeY = waferMap.MapSizeY;
                bundle.SizeX = mapSizeX;
                bundle.SizeY = mapSizeY;

                int masterX = waferMap.MasterDieX;
                int masterY = waferMap.MasterDieY;
                int diePitchX = originRecipe.DiePitchX;
                int diePitchY = originRecipe.DiePitchY;

                int originAbsX = originRecipe.OriginX;
                int originAbsY = originRecipe.OriginY - originRecipe.OriginHeight; // 좌상단 기준

                int originWidth = originRecipe.OriginWidth;
                int originHeight = originRecipe.OriginHeight;

                int startOffsetX = 0;
                int startOffsetY = 0;

                if (waferMap.UseExtraMap == true)
                {
                    mapData = waferMap.ExtraMapdata;
                    mapSizeX = waferMap.ExtraMapSizeX;
                    mapSizeY = waferMap.ExtraMapSizeY;
                    bundle.SizeX = mapSizeX;
                    bundle.SizeY = mapSizeY;

                    masterX = waferMap.ExtraMasterDieX;
                    masterY = waferMap.ExtraMasterDieY;

                    originAbsX -= waferMap.ExtraDieOffsetX * originWidth;
                    originAbsY -= waferMap.ExtraDieOffsetY * originHeight;

                    if (originAbsX < 0)
                    {
                        startOffsetX = -originAbsX;
                        originAbsX = 0;
                    }
                    if (originAbsY < 0)
                    {
                        startOffsetY = -originAbsY;
                        originAbsY = 0;
                    }

                }


                Workplace wp = new Workplace(-1, -1, originAbsX, originAbsY, originWidth, originHeight, queue.Count);

                bundle.Add(wp);
                queue.Enqueue(wp);

                // Right
                for (int x = masterX; x < mapSizeX; x++)
                {
                    // Top
                    for (int y = masterY; y >= 0; y--)
                    {
                        if ((mapData[x + y * mapSizeX] == (int)CHIP_TYPE.NORMAL) ||
                            (mapData[x + y * mapSizeX] == (int)CHIP_TYPE.EXTRA))
                        {
                            int dx = x - masterX;
                            int dy = y - masterY;
                            int dieAbsX = originAbsX + dx * diePitchX;
                            int dieAbsY = originAbsY + dy * diePitchY;

                            if(useExclusiveRegion == true)
                            {
                                if(PolygonController.HitTest(exRegion, 
                                    new Rect(new Point(dieAbsX, dieAbsY), 
                                    new Point(dieAbsX + originWidth, dieAbsY + originHeight))) == true)
                                {
                                    continue;
                                }
                            }

                            Workplace workplace = new Workplace(x, y, dieAbsX, dieAbsY, originWidth, originHeight, queue.Count);
                            if (y == masterY)
                            {
                                workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
                            }

                            bundle.Add(workplace);
                            queue.Enqueue(workplace);
                        }
                    }

                    // Bottom
                    for (int y = masterY + 1; y < mapSizeY; y++)
                    {
                        if ((mapData[x + y * mapSizeX] == (int)CHIP_TYPE.NORMAL) ||
                            (mapData[x + y * mapSizeX] == (int)CHIP_TYPE.EXTRA))
                        {
                            int dx = x - masterX;
                            int dy = y - masterY;
                            int dieAbsX = originAbsX + dx * diePitchX;
                            int dieAbsY = originAbsY + dy * diePitchY;

                            if (useExclusiveRegion == true)
                            {
                                if (PolygonController.HitTest(exRegion,
                                    new Rect(new Point(dieAbsX, dieAbsY),
                                    new Point(dieAbsX + originWidth, dieAbsY + originHeight))) == true)
                                {
                                    continue;
                                }
                            }

                            Workplace workplace = new Workplace(x, y, dieAbsX, dieAbsY, originWidth, originHeight, queue.Count);

                            bundle.Add(workplace);
                            queue.Enqueue(workplace);
                        }
                    }
                }


                // Left
                for (int x = masterX - 1; x >= 0; x--)
                {
                    // Top
                    for (int y = masterY; y >= 0; y--)
                    {
                        if ((mapData[x + y * mapSizeX] == (int)CHIP_TYPE.NORMAL) ||
                             (mapData[x + y * mapSizeX] == (int)CHIP_TYPE.EXTRA))
                        {
                            int dx = x - masterX;
                            int dy = y - masterY;
                            int dieAbsX = originAbsX + dx * diePitchX;
                            int dieAbsY = originAbsY + dy * diePitchY;

                            Workplace workplace = new Workplace(x, y, dieAbsX, dieAbsY, originWidth, originHeight, queue.Count);

                            if (useExclusiveRegion == true)
                            {
                                if (PolygonController.HitTest(exRegion,
                                    new Rect(new Point(dieAbsX, dieAbsY),
                                    new Point(dieAbsX + originWidth, dieAbsY + originHeight))) == true)
                                {
                                    continue;
                                }
                            }

                            if (y == masterY)
                            {
                                workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
                            }

                            bundle.Add(workplace);
                            queue.Enqueue(workplace);
                        }
                    }

                    // Bottom
                    for (int y = masterY + 1; y < mapSizeY; y++)
                    {
                        if ((mapData[x + y * mapSizeX] == (int)CHIP_TYPE.NORMAL) ||
                            (mapData[x + y * mapSizeX] == (int)CHIP_TYPE.EXTRA))
                        {
                            int dx = x - masterX;
                            int dy = y - masterY;
                            int dieAbsX = originAbsX + dx * diePitchX;
                            int dieAbsY = originAbsY + dy * diePitchY;

                            if (useExclusiveRegion == true)
                            {
                                if (PolygonController.HitTest(exRegion,
                                    new Rect(new Point(dieAbsX, dieAbsY),
                                    new Point(dieAbsX + originWidth, dieAbsY + originHeight))) == true)
                                {
                                    continue;
                                }
                            }

                            Workplace workplace = new Workplace(x, y, dieAbsX, dieAbsY, originWidth, originHeight, queue.Count);
                            
                            bundle.Add(workplace);
                            queue.Enqueue(workplace);
                        }
                    }
                }

                foreach (Workplace temp in bundle)
                {
                    temp.ParentBundle = bundle;
                    temp.SetSharedBuffer(bufferInfo);
                    temp.SetCameraInfo(cameraInfo);
                }

                return queue;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Inspection 생성에 실패 하였습니다.\n", ex.Message);
            }
        }
    }
}
