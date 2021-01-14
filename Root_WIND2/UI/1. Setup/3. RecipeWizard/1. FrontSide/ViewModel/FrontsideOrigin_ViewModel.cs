using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Root_WIND2
{
    class FrontsideOrigin_ViewModel : ObservableObject, IRecipeUILoadable
    {
        ImageData OriginImageData;
        TRect InspAreaBuf;
        Setup_ViewModel setup;

        public delegate void setOrigin(object e);
        public event setOrigin SetOrigin;
        Recipe m_Recipe;

        public void init(Setup_ViewModel setup, Recipe recipe)
        {
            m_Recipe = recipe;
            this.setup = setup;
            p_OriginBoxTool_VM = new FrontsideOriginBox_ViewModel();
            p_OriginBoxTool_VM.init(setup, recipe);
            p_OriginBoxTool_VM.BoxDone += P_BoxTool_VM_BoxDone;

            MapControl_VM = new MapControl_ViewModel(this.setup.InspectionVision, recipe);
            MapControl_VM.SetMap(setup.InspectionVision.mapdata, new CPoint(14, 14));

            p_OriginTool_VM = new FrontsideOriginTool_ViewModel(recipe);
            p_OriginTool_VM.AddOrigin += P_OriginTool_VM_AddOrigin;
            p_OriginTool_VM.AddPitch += P_OriginTool_VM_AddPitch;
            p_OriginTool_VM.DelegateInspArea += P_OriginTool_VM_DelegateInspArea;

            MasterDieX = m_Recipe.WaferMap.MasterDieX;
            MasterDieY = m_Recipe.WaferMap.MasterDieY;
            mapControl_VM.ChangeMasterImage(m_Recipe.WaferMap.MasterDieX, masterDieY);
        }
        public void SetPage()
        {
            DrawMapData();
            SetMapData();
        }
        #region[GET/SET]
        private FrontsideOriginBox_ViewModel m_OriginBoxTool_VM;
        public FrontsideOriginBox_ViewModel p_OriginBoxTool_VM
        {
            get
            {
                return m_OriginBoxTool_VM;
            }
            set
            {
                SetProperty(ref m_OriginBoxTool_VM, value);
            }
        }
        private FrontsideOriginTool_ViewModel m_OriginTool_VM;
        public FrontsideOriginTool_ViewModel p_OriginTool_VM
        {
            get
            {
                return m_OriginTool_VM;
            }
            set
            {
                SetProperty(ref m_OriginTool_VM, value);
            }
        }
        private MapControl_ViewModel mapControl_VM;
        public MapControl_ViewModel MapControl_VM
        {
            get
            {
                return mapControl_VM;
            }
            set
            {
                SetProperty(ref mapControl_VM, value);
            }
        }

        private int masterDieX = 0;
        public int MasterDieX
        {
            get
            {
                mapControl_VM.ChangeMasterImage(masterDieX, m_Recipe.WaferMap.MasterDieY);                
                return masterDieX;
            }
            set
            {
                int val = value;

                if (val < 0)
                    val = 0;
                if (val > m_Recipe.WaferMap.MapSizeX)
                    val = m_Recipe.WaferMap.MapSizeX - 1;

                SetProperty(ref masterDieX, value);
                m_Recipe.WaferMap.MasterDieX = masterDieX;
            }
        }
        private int masterDieY = 5;
        public int MasterDieY
        {
            get
            {                
                mapControl_VM.ChangeMasterImage(m_Recipe.WaferMap.MasterDieX, masterDieY);
                return masterDieY;
            }
            set
            {
                int[] map = mapControl_VM.GetMap();
                if(map.Length != 0)
                {
                    int val = value;
                    
                    if (val < 0)
                        val = 0;
                    if (val > m_Recipe.WaferMap.MapSizeY)
                        val = m_Recipe.WaferMap.MapSizeY - 1;

                    if (map[masterDieX + (val * m_Recipe.WaferMap.MapSizeX)] == (int)CHIP_TYPE.NO_CHIP)
                    {
                        int pos = 0;
                        while (true)
                        {
                            if (val - pos >= 0)
                                if (map[masterDieX + ((val - pos) * m_Recipe.WaferMap.MapSizeX)] == (int)CHIP_TYPE.NORMAL)
                                {
                                    masterDieY = val - pos;
                                    break;
                                }
                            if (val + pos < m_Recipe.WaferMap.MapSizeY)                
                                if (map[masterDieX + ((val + pos) * m_Recipe.WaferMap.MapSizeX)] == (int)CHIP_TYPE.NORMAL)
                                {
                                    masterDieY = val + pos;
                                    break;
                                }
                            pos++;

                            if (pos > 100)
                            {
                                masterDieY = m_Recipe.WaferMap.MapSizeX / 2;
                                break;
                            }
                        }
                        SetProperty(ref masterDieY, masterDieY);
                        m_Recipe.WaferMap.MasterDieY = masterDieY;
                    }
                    else
                    {
                        SetProperty(ref masterDieY, val);
                        m_Recipe.WaferMap.MasterDieY = masterDieY;
                    }
                }  
            }
        }

        private int mapSzX = 0;
        public int MapSzX
        {
            get
            {
                return mapSzX;
            }
            set
            {
                SetProperty(ref mapSzX, value);
            }
        }
        private int mapSzY = 0;
        public int MapSzY
        {
            get => mapSzY;
            set
            {
                SetProperty(ref mapSzY, value);
            }
        }
        private int shotW = 0;
        public int ShotW
        {
            get
            {
                return shotW;
            }
            set
            {
                SetProperty(ref shotW, value);
            }
        }
        private int shotH = 0;
        public int ShotH
        {
            get => shotH;
            set
            {
                SetProperty(ref shotH, value);
            }
        }
        #endregion
        public void LoadOriginData()
        {
            OriginRecipe origin = m_Recipe.GetRecipe<OriginRecipe>();
            CPoint ptOrigin = new CPoint(origin.OriginX, origin.OriginY);
            CPoint ptPitchSize = new CPoint(origin.DiePitchX, origin.DiePitchY);
            CPoint ptPadding = new CPoint(origin.InspectionBufferOffsetX, origin.InspectionBufferOffsetY);
            p_OriginTool_VM.LoadOriginData(ptOrigin, ptPitchSize, ptPadding);
            p_OriginBoxTool_VM.SetRoiRect();
            
            DrawMapData();
            SetMapData();
            //여기서 박스 그리기?
        }
        public void SetMasterDie(object e)
        {
            CPoint masterDiePos = e as CPoint;
            MasterDieX = masterDiePos.X;
            MasterDieY = masterDiePos.Y;
        }


        private void P_BoxTool_VM_BoxDone(object e)
        {
            TRect BOX = e as TRect;
            int byteCnt = p_OriginBoxTool_VM.p_ImageData.p_nByte;

            ImageData BoxImageData = new ImageData(BOX.MemoryRect.Width, BOX.MemoryRect.Height, byteCnt);

            BoxImageData.m_eMode = ImageData.eMode.ImageBuffer;
            BoxImageData.SetData(p_OriginBoxTool_VM.p_ImageData
                , new CRect(BOX.MemoryRect.Left, BOX.MemoryRect.Top, BOX.MemoryRect.Right, BOX.MemoryRect.Bottom)
                , (int)p_OriginBoxTool_VM.p_ImageData.p_Stride, byteCnt);

            p_OriginTool_VM.Offset = new CPoint(BOX.MemoryRect.Left, BOX.MemoryRect.Top);
            p_OriginTool_VM.p_ImageData = BoxImageData;
            p_OriginTool_VM.SetRoiRect();
        }

        private void P_OriginTool_VM_DelegateInspArea(object e)
        {
            TRect InspAreaBuf = e as TRect;
            p_OriginBoxTool_VM.AddInspArea(InspAreaBuf);

            CRect rect = new CRect(InspAreaBuf.MemoryRect.Left, InspAreaBuf.MemoryRect.Top, InspAreaBuf.MemoryRect.Right, InspAreaBuf.MemoryRect.Bottom);
            OriginImageData = new ImageData(rect.Width, rect.Height, p_OriginBoxTool_VM.p_ImageData.p_nByte);
            OriginImageData.m_eMode = ImageData.eMode.ImageBuffer;
            //OriginImageData.SetData(p_OriginBoxTool_VM.p_ImageData.GetPtr(), InspAreaBuf.MemoryRect, (int)p_OriginBoxTool_VM.p_ImageData.p_Stride, p_OriginBoxTool_VM.p_ImageData.p_nByte);
            OriginImageData.SetData(p_OriginBoxTool_VM.p_ImageData, rect, (int)p_OriginBoxTool_VM.p_ImageData.p_Stride, p_OriginBoxTool_VM.p_ImageData.p_nByte);
            InspAreaBuf.Tag = OriginImageData;

            SetOrigin(InspAreaBuf);

        }
        private void P_OriginTool_VM_AddPitch(object e)
        {
            p_OriginBoxTool_VM.AddPitchPoint(e as CPoint, Brushes.Green);
        }
        private void P_OriginTool_VM_AddOrigin(object e)
        {
            p_OriginBoxTool_VM.AddOriginPoint(e as CPoint, Brushes.Red);
        }
        private void DrawMapData()
        {
            RecipeType_WaferMap mapdata = m_Recipe.WaferMap;
            if (mapdata.Data.Length > 0)
            {
                int nMapX = mapdata.MapSizeX;
                int nMapY = mapdata.MapSizeY;

                MapControl_VM.SetMap(true, new CPoint(mapdata.MasterDieX, mapdata.MasterDieY), mapdata.Data, new CPoint(nMapX, nMapY));
            }
            else
            {
                MapControl_VM.SetMap(true, new CPoint(0, 5), setup.InspectionVision.mapdata, new CPoint(14, 14));
            }
        }
        private void SetMapData()
        {
            MapSzX = m_Recipe.WaferMap.MapSizeX;
            MapSzY = m_Recipe.WaferMap.MapSizeY;

            MasterDieX = m_Recipe.WaferMap.MasterDieX;
            MasterDieY = m_Recipe.WaferMap.MasterDieY;
        }

        public void Load()
        {
            LoadOriginData();
        }
    }
}
