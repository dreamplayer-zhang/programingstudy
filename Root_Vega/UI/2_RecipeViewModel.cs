using Microsoft.Win32;
using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Root_Vega
{
	public class _2_RecipeViewModel: ObservableObject
	{
		Vega_Engineer m_Engineer;

		public _2_RecipeViewModel(Vega_Engineer engineer)
		{
			m_Engineer = engineer;
			Init();
		}
		void Init()
		{

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
	}
}
