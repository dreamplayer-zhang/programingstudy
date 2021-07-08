using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_VEGA_P_Vision
{
    public class ConditionItem_ViewModel:ObservableObject
    {
        int conditioncnt;
        string defectCode, defectName;
        bool isEnable;
        public ConditionItem Main;
        RecipeBase recipe;
        #region Property
        public int ConditionCnt
        {
            get => conditioncnt;
            set => SetProperty(ref conditioncnt, value);
        }
        public bool IsEnable 
        {
            get => isEnable;
            set => SetProperty(ref isEnable, value);
        }
        public string DefectCode
        {
            get => defectCode;
            set => SetProperty(ref defectCode, value);
        }
        public string DefectName
        {
            get => defectName;
            set => SetProperty(ref defectName, value);
        }
        InspectionOneItem_ViewModel inspectionOneItem;
        public InspectionOneItem_ViewModel InspectionOneItem
        {
            get => inspectionOneItem;
            set => SetProperty(ref inspectionOneItem, value);
        }
        #endregion
        RecipeItemBase recipeItem;
        EUVPodSurfaceParameterBase parameterBase;
        public ConditionItem_ViewModel(int Num,RecipeBase recipe,RecipeItemBase recipeItem,InspectionOneItem_ViewModel inspectionOneItem)
        {
            Main = new ConditionItem();
            Main.DataContext = this;
            ConditionCnt = Num;
            this.recipe = recipe;
            this.recipeItem = recipeItem;
            this.inspectionOneItem = inspectionOneItem;

            //DefectName = recipeItem.

            Type t = recipeItem.GetType();
            if(t.Name.Contains("LowResRecipe"))
                parameterBase = recipe.GetItem<EUVPodSurfaceParameter>().PodTDI;
            else if (t.Name.Contains("StainRecipe"))
                parameterBase = recipe.GetItem<EUVPodSurfaceParameter>().PodStain;
            else if(t.Name.Contains("SideRecipe"))
            {
                //parameterBase = recipe.GetItem<EUVPodSurfaceParameter>().PodTDI;
            }
            else if(t.Name.Contains("HighResRecipe"))
            {
                parameterBase = recipe.GetItem<EUVPodSurfaceParameter>().PodStacking;

            }

            if (parameterBase.BrightParam.IsEnable)
            {
                defectName = parameterBase.BrightParam.DefectName;
                defectCode = parameterBase.BrightParam.DefectCode.ToString();
            }
            else if (parameterBase.DarkParam.IsEnable)
            {
                defectName = parameterBase.DarkParam.DefectName;
                defectCode = parameterBase.DarkParam.DefectCode.ToString();
            }
        }
        public ICommand btnImagenROI
        {
            get => new RelayCommand(() => {
                    VegaPEventManager.OnImageROIBtnClicked(this, new ImageROIEventArgs(recipe,recipeItem, parameterBase));
            });
        }
        public ICommand btnDelete
        {
            get => new RelayCommand(() => {
                InspectionOneItem.ListItem.RemoveAt(ConditionCnt-1);
            });
        }
    }
}
