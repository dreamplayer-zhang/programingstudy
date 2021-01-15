using RootTools;
using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
	class Edgeside_ViewModel : ObservableObject
	{
		private Setup_ViewModel setupVM;
		Recipe recipe;

		public Edgeside_Panel Main;
		public EdgesideSetup_ViewModel PanelVM;
		public EdgesideSetup SetupPage;

		public Edgeside_ViewModel(Setup_ViewModel setup)
		{
			this.setupVM = setup;
			this.recipe = setup.Recipe;
			Init();
		}

		public void Init()
		{
			Main = new Edgeside_Panel();
			PanelVM = new EdgesideSetup_ViewModel();
			PanelVM.Init(setupVM, recipe);

			SetupPage = new EdgesideSetup();
			SetupPage.DataContext = PanelVM;
			SetPage(SetupPage);
		}

		public ICommand btnEdgeSetup
		{
			get
			{
				return new RelayCommand(() => SetPage(SetupPage));
			}
		}

		public ICommand btnEdgeSnap
		{
			get
			{
				return new RelayCommand(() => PanelVM.Scan());
			}
		}

		public ICommand btnEdgeInsp
		{
			get
			{
				return new RelayCommand(() => PanelVM.Inspect());
			}
		}

		public ICommand btnBack
		{
			get
			{
				return new RelayCommand(() => setupVM.SetRecipeWizard());
			}
		}

		private void SetPage(UserControl page)
		{
			Main.SubPanel.Children.Clear();
			Main.SubPanel.Children.Add(page);
		}

		public void UI_Redraw()
		{
			PanelVM.LoadParameter();
		}

	}
}
