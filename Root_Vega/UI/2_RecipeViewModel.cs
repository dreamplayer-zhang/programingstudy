using Microsoft.Win32;
using RootTools;
using RootTools.Comm;
using RootTools.Module;
using RootTools.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_Vega
{
	public class _2_RecipeViewModel : ObservableObject
	{
		Vega_Engineer m_Engineer;

		#region Recipe
		ModuleList m_moduleList;
		ModuleRunList m_moduleRunList;
		public ModuleRunList p_moduleRunList
		{
			get { return m_moduleRunList; }
			set { SetProperty(ref m_moduleRunList, value);}
		}
		Vega_Recipe m_recipe;
		public Vega_Recipe p_recipe
		{
			get { return m_recipe; }
			set { SetProperty(ref m_recipe, value); }
		}

		System.Windows.Visibility m_vAddButtonVisibility = System.Windows.Visibility.Hidden;
		public System.Windows.Visibility p_vAddButtonVisibility
		{
			get { return m_vAddButtonVisibility; }
			set { SetProperty(ref m_vAddButtonVisibility, value); }
		}

		string m_strSelectedModuleRun;
		public string p_strSelectedModuleRun
		{
			get { return m_strSelectedModuleRun; }
			set 
			{
				SetProperty(ref m_strSelectedModuleRun, value);
				if (m_strSelectedModuleRun == null) p_vAddButtonVisibility = System.Windows.Visibility.Hidden;
				else p_vAddButtonVisibility = System.Windows.Visibility.Visible;
			}
		}
		
		List<string> m_aSelectedModuleRun;
		public List<string> p_aSelectedModuleRun
		{
			get { return m_aSelectedModuleRun; }
			set { SetProperty(ref m_aSelectedModuleRun, value); }
		}

		string m_strSelectedModule;
		public string p_strSelectedModule
		{
			get { return m_strSelectedModule; }
			set 
			{
				SetProperty(ref m_strSelectedModule, value);
				p_aSelectedModuleRun = m_moduleRunList.GetRecipeRunNames(m_strSelectedModule);
				p_strSelectedModuleRun = null;
			}
		}

		string m_sPath = "c:\\Recipe\\";
		void btnRecipeOpenClicked()
		{
			string sModel = EQ.m_sModel;
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.InitialDirectory = m_sPath;
			dlg.DefaultExt = "." + sModel;
			dlg.Filter = sModel + " Recipe (." + sModel + ")|*." + sModel;
			if (dlg.ShowDialog() == true) m_moduleRunList.OpenJob(dlg.FileName);
			p_moduleRunList.RunTree(Tree.eMode.Init);
		}

		void btnRecipeSaveClicked()
		{
			string sModel = EQ.m_sModel;
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.InitialDirectory = m_sPath;
			dlg.DefaultExt = "." + sModel;
			dlg.Filter = sModel + " Recipe (." + sModel + ")|*." + sModel;
			if (dlg.ShowDialog() == true) m_moduleRunList.SaveJob(dlg.FileName);
			p_moduleRunList.RunTree(Tree.eMode.Init);
		}

		void btnRecipeClearClicked()
		{
			m_moduleRunList.Clear();
			p_moduleRunList.RunTree(Tree.eMode.Init);
		}

		void btnRecipeAddClicked()
		{
			m_moduleRunList.Add(p_strSelectedModule, p_strSelectedModuleRun);
			p_moduleRunList.RunTree(Tree.eMode.Init);
		}

		public ICommand CommandRecipeOpen
		{
			get
			{
				return new RelayCommand(btnRecipeOpenClicked);
			}
		}

		public ICommand CommandRecipeSave
		{
			get
			{
				return new RelayCommand(btnRecipeSaveClicked);
			}
		}

		public ICommand CommandRecipeClear
		{
			get
			{
				return new RelayCommand(btnRecipeClearClicked);
			}
		}

		public ICommand CommandRecipeAdd
		{
			get
			{
				return new RelayCommand(btnRecipeAddClicked);
			}
		}
		#endregion

		public _2_RecipeViewModel(Vega_Engineer engineer)
		{
			m_Engineer = engineer;
			Init();
		}
		void Init()
		{
			m_moduleList = m_Engineer.m_handler.m_moduleList;
			m_moduleRunList = m_moduleList.m_moduleRunList;
			m_recipe = m_Engineer.m_handler.m_recipe;
		}

		private void _btnRcpLoad()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Vega Vision Recipe (*.VegaVision)|*.VegaVision";
			dlg.InitialDirectory = @"C:\VEGA\Recipe";
			if (dlg.ShowDialog() == true)
			{
				m_Engineer.m_recipe.Load(dlg.FileName);
			}
		}
		private void _btnRcpSaveAs()
		{
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
			dlg.Filter = "Vega Vision Recipe (*.VegaVision)|*.VegaVision";
			dlg.InitialDirectory = @"C:\VEGA\Recipe";
			if (dlg.ShowDialog() == true)
			{
				var target = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(dlg.FileName), System.IO.Path.GetFileNameWithoutExtension(dlg.FileName));
				m_Engineer.m_recipe.Save(target);
			}
		}

		private void _currentRcpSave()
		{
			if (m_Engineer.m_recipe.Loaded)
			{
				var target = System.IO.Path.Combine(System.IO.Path.Combine(@"C:\VEGA\Recipe", m_Engineer.m_recipe.RecipeName));
				m_Engineer.m_recipe.Save(target);
			}
			else
			{
				_btnRcpSaveAs();
			}
		}

		private void _rcpCreate()
		{
			m_Engineer.m_recipe.Init();

			if (m_Engineer.m_recipe.LoadComplete != null)
			{
				m_Engineer.m_recipe.LoadComplete();
			}
		}
		public ICommand CommandRcpSave
		{
			get
			{
				return new RelayCommand(_currentRcpSave);
			}
		}
		public ICommand CommandRcpSaveAs
		{
			get
			{
				return new RelayCommand(_btnRcpSaveAs);
			}
		}
		public ICommand CommandRcpLoad
		{
			get
			{
				return new RelayCommand(_btnRcpLoad);
			}
		}
		public ICommand CommandRcpCreate
		{
			get
			{
				return new RelayCommand(_rcpCreate);
			}
		}
	}
}
