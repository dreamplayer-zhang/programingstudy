﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Root_VEGA_P_Vision.Engineer;
using RootTools;
using RootTools.Memory;
using RootTools_Vision;

namespace Root_VEGA_P_Vision
{
    public class RecipeSideImageViewers_ViewModel:ObservableObject
    {
        RootViewer_ViewModel top_ViewerVM, bottom_ViewerVM, left_ViewerVM, right_ViewerVM;

        #region Property
        public RootViewer_ViewModel Top_ViewerVM
        {
            get => top_ViewerVM;
            set => SetProperty(ref top_ViewerVM, value);
        }
        public RootViewer_ViewModel Bottom_ViewerVM
        {
            get => bottom_ViewerVM;
            set => SetProperty(ref bottom_ViewerVM, value);
        }
        public RootViewer_ViewModel Left_ViewerVM
        {
            get => left_ViewerVM;
            set => SetProperty(ref left_ViewerVM, value);
        }
        public RootViewer_ViewModel Right_ViewerVM
        {
            get => right_ViewerVM;
            set => SetProperty(ref right_ViewerVM, value);
        }
        #endregion
        RecipeSideImageViewers_Panel Main;
        public RecipeSideImageViewers_ViewModel(string parts,RecipeSide_ViewModel recipeSide)
        {
            Main = new RecipeSideImageViewers_Panel();
            Main.DataContext = this;
            top_ViewerVM = new MaskRootViewer_ViewModel(parts + ".Top", recipeSide.recipeSetting.MaskTools);

            bottom_ViewerVM = new MaskRootViewer_ViewModel(parts+".Bottom", recipeSide.recipeSetting.MaskTools);

            left_ViewerVM = new MaskRootViewer_ViewModel(parts+".Left", recipeSide.recipeSetting.MaskTools);

            right_ViewerVM = new MaskRootViewer_ViewModel(parts+".Right", recipeSide.recipeSetting.MaskTools);
        }
    }
}