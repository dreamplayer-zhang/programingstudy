using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Root_WIND2
{
	class Edge_ViewModel : ObservableObject
	{
		private Setup_ViewModel setupVM;

		public Edge_Panel Main;
		public EdgeSetup_ViewModel PanelVM;
		public EdgeSetupPage SetupPage;

		public Edge_ViewModel(Setup_ViewModel setup)
		{
			this.setupVM = setup;
			Init();
		}

		public void Init()
		{
			Main = new Edge_Panel();
			PanelVM = new EdgeSetup_ViewModel(setupVM);
			PanelVM.Init();

			SetupPage = new EdgeSetupPage();
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

	}
}
