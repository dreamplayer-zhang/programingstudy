﻿using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public enum AlignBtnState
    {
        Align,Position,ManualAlign
    }
    public class AlignFeatureInfo_ViewModel:ObservableObject
    {
        #region Property
        public EUVPositionRecipe PositionRecipe
        {
            get => positionRecipe;
            set => SetProperty(ref positionRecipe, value);
        }
        private ObservableCollection<UIElement> alignFeatureList = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> AlignFeatureList
        {
            get
            {
                return alignFeatureList;
            }
            set
            {
                SetProperty(ref alignFeatureList, value);
            }
        }
        private ObservableCollection<UIElement> positionFeatureList = new ObservableCollection<UIElement>();
        public ObservableCollection<UIElement> PositionFeatureList
        {
            get
            {
                return positionFeatureList;
            }
            set
            {
                positionFeatureList = value;
            }
        }

        int positionFeatureIdx = -1;
        public int PositionFeatureIdx
        {
            get => positionFeatureIdx;
            set
            {
                SetProperty(ref positionFeatureIdx, value);

                if (CurPosData == null) return;
                if (value == -1)
                {
                    CurPosData = new RecipeType_ImageData(0, 0, 0, 0, 1, null);
                    return;
                }
                CurPosData = list.ListPositionFeature[value];
            } 
        }
        int alignFeatureIdx = -1;
        public int AlignFeatureIdx
        {
            get => alignFeatureIdx;
            set
            {
                SetProperty(ref alignFeatureIdx, value);
                if (CurAlignData == null) return;
                if (value == -1)
                {
                    CurAlignData = new RecipeType_ImageData(0, 0, 0, 0, 1, null);
                    return;
                }

                CurAlignData = list.ListAlignFeature[value];
            }
        }
        RecipeType_ImageData curAlignData; //현재 select한 Data 정보
        public RecipeType_ImageData CurAlignData
        {
            get => curAlignData;
            set => SetProperty(ref curAlignData, value);
        }
        RecipeType_ImageData curPosData; //현재 select한 Data 정보
        public RecipeType_ImageData CurPosData
        {
            get => curPosData;
            set => SetProperty(ref curPosData, value);
        }
        #endregion

        public AlignFeatureInfo Main;
        EUVPositionRecipe positionRecipe;
        RecipeOrigin_ViewModel recipeOrigin;
        PositionFeature curTabIdx;
        public AlignBtnState btnState;
        FeatureLists list;
        public AlignFeatureInfo_ViewModel(RecipeOrigin_ViewModel recipeOrigin,FeatureLists list)
        {
            Main = new AlignFeatureInfo();
            this.recipeOrigin = recipeOrigin;
            curTabIdx = recipeOrigin.PositionViewerTab.curTab;
            Main.DataContext = this;
            positionRecipe = GlobalObjects.Instance.Get<RecipeVision>().GetItem<EUVPositionRecipe>();
            this.list = list;
            CurPosData = new RecipeType_ImageData();
            CurAlignData = new RecipeType_ImageData();
        }

        public void LoadRecipe()
        {
            //PositionFeatureList.Clear();
            //AlignFeatureList.Clear();

            //foreach (RecipeType_ImageData feature in positionRecipe.ListAlignFeature)
            //{
            //    FeatureControl fc = new FeatureControl();
            //    fc.p_ImageSource = Tools.ConvertBitmapToSource(Tools.CovertArrayToBitmap(feature.RawData, feature.Width, feature.Height, feature.ByteCnt));
            //    fc.DataContext = this;

            //    AlignFeatureList.Add(fc);
            //}
            //foreach (RecipeType_ImageData feature in positionRecipe.ListPositionFeature)
            //{
            //    FeatureControl fc = new FeatureControl();
            //    fc.p_ImageSource = Tools.ConvertBitmapToSource(Tools.CovertArrayToBitmap(feature.RawData, feature.Width, feature.Height, feature.ByteCnt));
            //    fc.DataContext = this;

            //    PositionFeatureList.Add(fc);
            //}
        }
        #region ICommand
        public ICommand btnAlign
        {
            get => new RelayCommand(() => {
                recipeOrigin.PositionViewerTab.selectedViewer.btnState = btnState = AlignBtnState.Align;
            });
        }
        public ICommand btnPosition
        {
            get => new RelayCommand(() => {
                recipeOrigin.PositionViewerTab.selectedViewer.btnState = btnState = AlignBtnState.Position;
            });
        }
        public ICommand btnAdd
        {
            get => new RelayCommand(() => {
                switch (btnState)
                {
                    case AlignBtnState.Align:
                        AlignAdd();
                        break;
                    case AlignBtnState.Position:
                        PosAdd();
                        break;
                }
            });
        }
        public ICommand btnDelete
        {
            get => new RelayCommand(() =>
              {
                  switch(btnState)
                  {
                      case AlignBtnState.Align:
                          AlignDelete();
                          break;
                      case AlignBtnState.Position:
                          PosDelete();
                          break;
                  }
              });
        }
        public ICommand btnClear
        {
            get => new RelayCommand(() => { 
                switch(btnState)
                {
                    case AlignBtnState.Align:
                        AlignClear();
                        break;
                    case AlignBtnState.Position:
                        PosClear();
                        break;
                }
            });
        }
        public ICommand ManualAlign
        {
            get => new RelayCommand(() => {
                recipeOrigin.PositionViewerTab.selectedViewer.btnState = btnState = AlignBtnState.ManualAlign;
            });
        }
        void AlignAdd()
        {
            if (recipeOrigin.memRect == null)
                return;

            ImageData data = recipeOrigin.boxImage;
            CRect memRect = recipeOrigin.memRect;
            int width = memRect.Width;
            int height = memRect.Height;
            int posX = memRect.Left;
            int posY = memRect.Top;

            list.AddAlignFeature(posX, posY, width, height, 1, data.GetByteArray());
            RecipeType_ImageData feature = list.ListAlignFeature[list.ListAlignFeature.Count - 1];
            FeatureControl fc = new FeatureControl();
            fc.p_ImageSource = Tools.ConvertBitmapToSource(Tools.CovertArrayToBitmap(feature.RawData, feature.Width, feature.Height, feature.ByteCnt));
            fc.DataContext = this;
            AlignFeatureList.Add(fc);
        }
        void PosAdd()
        {
            if (recipeOrigin.memRect == null)
                return;
            ImageData data = recipeOrigin.boxImage;
            CRect memRect = recipeOrigin.memRect;
            int width = memRect.Width;
            int height = memRect.Height;
            int posX = memRect.Left;
            int posY = memRect.Top;

            list.AddPositionFeature(posX, posY, width, height, 1, data.GetByteArray());
            RecipeType_ImageData feature = list.ListPositionFeature[list.ListPositionFeature.Count - 1];
            FeatureControl fc = new FeatureControl();
            fc.p_ImageSource = Tools.ConvertBitmapToSource(Tools.CovertArrayToBitmap(feature.RawData, feature.Width, feature.Height, feature.ByteCnt));
            fc.DataContext = this;
            PositionFeatureList.Add(fc);
        }
        void AlignDelete()
        {
            list.RemoveAlignFeature(AlignFeatureIdx);
            AlignFeatureList.RemoveAt(AlignFeatureIdx);
            if (AlignFeatureList.Count == 0)
                AlignFeatureIdx = -1;
        }
        void PosDelete()
        {
            list.RemoveAlignFeature(AlignFeatureIdx);
            PositionFeatureList.RemoveAt(AlignFeatureIdx);
            if (PositionFeatureList.Count == 0)
                PositionFeatureIdx = -1;
        }
        void PosClear()
        {
            list.ClearPositionFeature();
            PositionFeatureList.Clear();
            PositionFeatureIdx = -1;
        }

        void AlignClear()
        {
            list.ClearAlignFeature();
            AlignFeatureList.Clear();
            AlignFeatureIdx = -1;
        }
        #endregion
    }
}
