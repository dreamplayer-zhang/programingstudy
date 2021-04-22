using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_VEGA_P_Vision
{
    class RecipeManager_ViewModel:ObservableObject
    {
        public Setup_ViewModel m_Setup;
        public RecipeMangerPanel Main;

        public RecipeManager_ViewModel(Setup_ViewModel setup)
        {
            m_Setup = setup;
            Init();
        }
        private void Init()
        {
            Main = new RecipeManagerPanel();

            //SetPage(Main);
        }

    }
}
