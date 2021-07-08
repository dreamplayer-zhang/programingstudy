using Root_VEGA_P.Engineer;
using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_P
{
    public class PodRecipe_ViewModel:ObservableObject
    {
        public PodRecipe_Panel Main;
        NozzleRecipeInfo_ViewModel domeRecipe, doorRecipe, coverRecipe, plateRecipe;

        #region Property
        public NozzleRecipeInfo_ViewModel DomeRecipe
        {
            get => domeRecipe;
            set => SetProperty(ref domeRecipe, value);
        }
        public NozzleRecipeInfo_ViewModel DoorRecipe
        {
            get => doorRecipe;
            set => SetProperty(ref doorRecipe, value);
        }
        public NozzleRecipeInfo_ViewModel CoverRecipe
        {
            get => coverRecipe;
            set => SetProperty(ref coverRecipe, value);
        }
        public NozzleRecipeInfo_ViewModel PlateRecipe
        {
            get => plateRecipe;
            set => SetProperty(ref plateRecipe, value);
        }
        ObservableCollection<UIElement> fileList;
        public ObservableCollection<UIElement> FileList
        {
            get => fileList;
            set => SetProperty(ref fileList, value);
        }
        #endregion
        VEGA_P_Handler handler;
        public PodRecipe_ViewModel()
        {
            Main = new PodRecipe_Panel();
            Main.DataContext = this;
            handler = GlobalObjects.Instance.Get<VEGA_P_Engineer>().m_handler;
            DomeRecipe = new NozzleRecipeInfo_ViewModel(@"..\Resources\doom.png", handler.m_EOP.m_dome.m_particleCounterSet.m_nozzleSet);
            DoorRecipe = new NozzleRecipeInfo_ViewModel(@"..\Resources\door.png",handler.m_EOP.m_door.m_particleCounterSet.m_nozzleSet);
            CoverRecipe = new NozzleRecipeInfo_ViewModel(@"..\Resources\coverbot.png",handler.m_EIP_Cover.m_particleCounterSet.m_nozzleSet);
            PlateRecipe = new NozzleRecipeInfo_ViewModel(@"..\Resources\basebot.png",handler.m_EIP_Plate.m_particleCounterSet.m_nozzleSet);

            fileList = new ObservableCollection<UIElement>();
            for (int i = 0; i < 5; i++)
            {
                FileItem item = new FileItem("Pod "+i,"2.34kg");
                fileList.Add(item);
            }
        }
    }
}
