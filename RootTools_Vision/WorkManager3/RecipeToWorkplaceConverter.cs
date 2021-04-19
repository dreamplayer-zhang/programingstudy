﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RootTools_Vision.WorkManager3
{
    public class RecipeToWorkplaceConverter
    {
        /// <summary>
        /// WaferMap을 WorkplaceBundle롤 변경
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public static WorkplaceBundle Convert(RecipeType_WaferMap waferMap)
        {
            WorkplaceBundle bundle = new WorkplaceBundle();

            try
            {
                bundle.Add(new Workplace(-1, -1, 0, 0, 0, 0, bundle.Count));

                var mapData = waferMap.Data;
                int mapSizeX = waferMap.MapSizeX;
                int mapSizeY = waferMap.MapSizeY;
                int masterX = waferMap.MasterDieX;
                int masterY = waferMap.MasterDieY;

                bundle.SizeX = mapSizeX;
                bundle.SizeY = mapSizeY;

                // Right
                for (int x = masterX; x < mapSizeX; x++)
                {
                    // Top
                    for (int y = masterY; y >= 0; y--)
                    {
                        if (mapData[x + y * mapSizeX] == 1)
                        {
                            Workplace workplace = new Workplace(x, y, 0, 0, 0, 0, bundle.Count);
                            if (y == masterY)
                            {
                                workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
                            }

                            bundle.Add(workplace);
                        }
                    }

                    // Bottom
                    for (int y = masterY + 1; y < mapSizeY; y++)
                    {
                        if (mapData[x + y * mapSizeX] == 1)
                        {
                            Workplace workplace = new Workplace(x, y, 0, 0, 0, 0, bundle.Count);
                            bundle.Add(workplace);
                        }
                    }
                }


                // Left
                for (int x = masterX - 1; x >= 0; x--)
                {
                    // Top
                    for (int y = masterY; y >= 0; y--)
                    {
                        if (mapData[x + y * mapSizeX] == 1)
                        {
                            Workplace workplace = new Workplace(x, y, 0, 0, 0, 0, bundle.Count);
                            if (y == masterY)
                            {
                                workplace.SetSubState(WORKPLACE_SUB_STATE.LINE_FIRST_CHIP, true);
                            }

                            bundle.Add(workplace);
                        }
                    }

                    // Bottom
                    for (int y = masterY + 1; y < mapSizeY; y++)
                    {
                        if (mapData[x + y * mapSizeX] == 1)
                        {
                            Workplace workplace = new Workplace(x, y, 0, 0, 0, 0, bundle.Count);
                            bundle.Add(workplace);
                        }
                    }
                }

                return bundle;
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Inspection 생성에 실패 하였습니다.\n", ex.Message);
            }
        }

        public static ConcurrentQueue<Workplace> ConvertToQueue(RecipeType_WaferMap waferMap, OriginRecipe originRecipe, SharedBufferInfo bufferInfo)
        {
            ConcurrentQueue<Workplace> queue = new ConcurrentQueue<Workplace>();
            WorkplaceBundle bundle = new WorkplaceBundle();

            try
            {
                Workplace wp = new Workplace(-1, -1, 0, 0, 0, 0, queue.Count);

                bundle.Add(wp);
                queue.Enqueue(wp);

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
                int originAbsY = originRecipe.OriginY - originRecipe.DiePitchY; // 좌상단 기준

                // Right
                for (int x = masterX; x < mapSizeX; x++)
                {
                    // Top
                    for (int y = masterY; y >= 0; y--)
                    {
                        if (mapData[x + y * mapSizeX] == 1)
                        {
                            int dx = x - masterX;
                            int dy = y - masterY;
                            int dieAbsX = originAbsX + dx * diePitchX;
                            int dieAbsY = originAbsY + dy * diePitchY;

                            Workplace workplace = new Workplace(x, y, dieAbsX, dieAbsY, diePitchX, diePitchY, queue.Count);
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
                        if (mapData[x + y * mapSizeX] == 1)
                        {
                            int dx = x - masterX;
                            int dy = y - masterY;
                            int dieAbsX = originAbsX + dx * diePitchX;
                            int dieAbsY = originAbsY + dy * diePitchY;

                            Workplace workplace = new Workplace(x, y, dieAbsX, dieAbsY, diePitchX, diePitchY, queue.Count);

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
                        if (mapData[x + y * mapSizeX] == 1)
                        {
                            int dx = x - masterX;
                            int dy = y - masterY;
                            int dieAbsX = originAbsX + dx * diePitchX;
                            int dieAbsY = originAbsY + dy * diePitchY;

                            Workplace workplace = new Workplace(x, y, dieAbsX, dieAbsY, diePitchX, diePitchY, queue.Count);

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
                        if (mapData[x + y * mapSizeX] == 1)
                        {
                            int dx = x - masterX;
                            int dy = y - masterY;
                            int dieAbsX = originAbsX + dx * diePitchX;
                            int dieAbsY = originAbsY + dy * diePitchY;

                            Workplace workplace = new Workplace(x, y, dieAbsX, dieAbsY, diePitchX, diePitchY, queue.Count);
                            
                            bundle.Add(workplace);
                            queue.Enqueue(workplace);
                        }
                    }
                }

                foreach (Workplace temp in bundle)
                {
                    temp.ParentBundle = bundle;
                    temp.SetSharedBuffer(bufferInfo);
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