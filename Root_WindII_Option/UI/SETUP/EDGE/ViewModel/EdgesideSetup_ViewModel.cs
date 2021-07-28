using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_WindII_Option.UI
{
	public class EdgesideSetup_ViewModel : ObservableObject
	{
		#region [Getter / Setter]
		private Edgeside_ImageViewer_ViewModel imageViewerVM;
		public Edgeside_ImageViewer_ViewModel ImageViewerVM
		{
			get => imageViewerVM;
			set => SetProperty(ref imageViewerVM, value);
		}

		private EdgesideSetupModule_ViewModel moduleVM;
		public EdgesideSetupModule_ViewModel ModuleVM
		{
			get => moduleVM;
			set => SetProperty(ref moduleVM, value);
		}

		private bool isTopChecked = true;
		public bool IsTopChecked
		{
			get => isTopChecked;
			set
			{
				SetProperty(ref isTopChecked, value);
				if (isTopChecked)
				{
					IsSideChecked = false;
					IsBtmChecked = false;
				}
			}
		}

		private bool isSideChecked = false;
		public bool IsSideChecked
		{
			get => isSideChecked;
			set
			{
				SetProperty(ref isSideChecked, value);
				if (isSideChecked)
				{
					IsTopChecked = false;
					IsBtmChecked = false;
				}
			}
		}

		private bool isBtmChecked = false;
		public bool IsBtmChecked
		{
			get => isBtmChecked;
			set
			{
				SetProperty(ref isBtmChecked, value);
				if (isBtmChecked)
				{
					IsTopChecked = false;
					IsSideChecked = false;
				}
			}
		}
		#endregion

		#region [Command]
		public ICommand btnTop
		{
			get
			{
				return new RelayCommand(() =>
				{
					ChangeViewer();
				});
			}
		}

		public ICommand btnSide
		{
			get
			{
				return new RelayCommand(() =>
				{
					ChangeViewer();
				});
			}
		}

		public ICommand btnBottom
		{
			get
			{
				return new RelayCommand(() =>
				{
					ChangeViewer();
				});
			}
		}
		#endregion

		public EdgesideSetup_ViewModel()
		{
			if (GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage").GetPtr() == IntPtr.Zero)
				return;

			imageViewerVM = new Edgeside_ImageViewer_ViewModel();
			imageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());

			moduleVM = new EdgesideSetupModule_ViewModel();
		}

		private void ChangeViewer()
		{
			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();

			if (recipe.GetItem<EdgeSurfaceParameter>() == null)
				return;

			if (IsTopChecked)
			{
				ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeTopImage"), GlobalObjects.Instance.Get<DialogService>());
				ModuleVM.SetRecipeParameter(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop);
			}
			else if (IsSideChecked)
			{
				ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeSideImage"), GlobalObjects.Instance.Get<DialogService>());
				ModuleVM.SetRecipeParameter(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide);
			}
			else if (IsBtmChecked)
			{
				ImageViewerVM.init(GlobalObjects.Instance.GetNamed<ImageData>("EdgeBottomImage"), GlobalObjects.Instance.Get<DialogService>());
				ModuleVM.SetRecipeParameter(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm);
			}
			else
				return;
		}

		public void LoadParameter()
		{
			RecipeEdge recipe = GlobalObjects.Instance.Get<RecipeEdge>();

			if (recipe.GetItem<OriginRecipe>() == null) return;
			if (recipe.GetItem<EdgeSurfaceParameter>() == null) return;
						
			ModuleVM.OriginRecipe = recipe.GetItem<OriginRecipe>();
			ModuleVM.ProcessDefectParameter = recipe.GetItem<ProcessDefectEdgeParameter>();

			if (IsTopChecked)
				ModuleVM.SetRecipeParameter(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseTop, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseTop);
			else if (IsSideChecked)
				ModuleVM.SetRecipeParameter(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseSide, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseSide);
			else if (IsBtmChecked)
				ModuleVM.SetRecipeParameter(recipe.GetItem<EdgeSurfaceRecipe>().EdgeRecipeBaseBtm, recipe.GetItem<EdgeSurfaceParameter>().EdgeParamBaseBtm);
		}
	}
}
