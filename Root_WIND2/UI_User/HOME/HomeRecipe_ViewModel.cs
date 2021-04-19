﻿using Root_WIND2.Module;
using RootTools.Module;
using RootTools.Trees;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WIND2.UI_User
{
    public class HomeRecipe_ViewModel : ObservableObject
    {
        #region [Properties]

        private RecipeSelectionViewer_ViewModel recipeSelectionViewerVM = new RecipeSelectionViewer_ViewModel();
        public RecipeSelectionViewer_ViewModel RecipeSelectionViewerVM
        {
            get => this.recipeSelectionViewerVM;
            set
            {
                SetProperty(ref this.recipeSelectionViewerVM, value);
            }
        }


        private ObservableCollection<ModuleView> moduleList = new ObservableCollection<ModuleView>();
        public ObservableCollection<ModuleView> ModuleList
        {
            get => this.moduleList;
            set
            {
                SetProperty(ref moduleList, value);
            }
        }

        private ModuleView_ViewModel selectedModule;
        public ModuleView_ViewModel SelectedModule
        {
            get => this.selectedModule;
            set
            {
                SetProperty(ref selectedModule, value);
            }
        }
        #endregion

        Vision vision = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).p_Vision;

        WIND2_Recipe m_recipe;
        ModuleRunList m_moduleRunList;


        public HomeRecipe_ViewModel()
        {
            m_recipe = ((WIND2_Handler)GlobalObjects.Instance.Get<WIND2_Engineer>().ClassHandler()).m_recipe;
            m_moduleRunList = m_recipe.m_moduleRunList;

            m_moduleRunList.Add(vision.p_id,  "GrabLineScan");
            m_moduleRunList.RunTree(Tree.eMode.Init);

            ModuleList.Add(new ModuleView() { ModuleName = "FRONT" });
            ModuleList.Add(new ModuleView() { ModuleName = "BACK" });
            ModuleList.Add(new ModuleView() { ModuleName = "EDGE" });
            ModuleList.Add(new ModuleView() { ModuleName = "EBR" });
        }


        #region [Command]
        public ICommand LoadedCommand
        {
            get => new RelayCommand(() =>
            {
                this.RecipeSelectionViewerVM.RefreshProductItemList();
            });
        }
        #endregion
    }
}