using RootTools;
using RootTools.Module;

namespace Root_Vega
{
    class _6_LogViewModel : ObservableObject
    {
        Vega_Engineer m_engineer;
		ModuleRunList m_moduleRunList;
		//_1_2_LoadPort m_lp = ;
		public ModuleRunList p_moduleRunList
		{
			get { return m_moduleRunList; }
			set { SetProperty(ref m_moduleRunList, value); }
		}
		Vega_Recipe m_recipe;
		public Vega_Recipe p_recipe
		{
			get { return m_recipe; }
			set { SetProperty(ref m_recipe, value); }
		}

		public _6_LogViewModel(Vega_Engineer engineer)
        {
            m_engineer = engineer;
			Init();
		}

		void Init()
        {
			p_recipe = m_engineer.m_handler.m_recipe;
			p_moduleRunList = p_recipe.m_moduleRunList;
		}
    }
}
