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

		public Edgeside_Panel Main;
		public EdgesideSetup_ViewModel SetupVM;
		public EdgesideSetup SetupPage;

		public Edgeside_ViewModel(Setup_ViewModel setup)
		{
			this.setupVM = setup;
			Init();
		}

		public void Init()
		{
			Main = new Edgeside_Panel();
			SetupVM = new EdgesideSetup_ViewModel();
			SetupVM.Init(setupVM);

			SetupPage = new EdgesideSetup();
			SetupPage.DataContext = SetupVM;
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
				return new RelayCommand(() => SetupVM.Scan());
			}
		}

		public ICommand btnEdgeInsp
		{
			get
			{
				return new RelayCommand(() => SetupVM.Inspect());
			}
		}

		public ICommand btnBack
		{
			get
			{
				return new RelayCommand(() => setupVM.SetRecipeWizard());
			}
		}
		public RelayCommand btnWaferLoad
        {
            get
            {
				return new RelayCommand(btnWaferLoadClick);
            }
        }

		private void btnWaferLoadClick()
        {
			setupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRunOpen();
			setupVM.maintVM.HandlerUI.GetModuleList_UI().ModuleListRun();
        }

		private void SetPage(UserControl page)
		{
			Main.SubPanel.Children.Clear();
			Main.SubPanel.Children.Add(page);
		}

		public void UI_Redraw()
		{
			//SetupVM.LoadParameter();
		}

	}
}
