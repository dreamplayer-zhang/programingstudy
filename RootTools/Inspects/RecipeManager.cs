using System.Collections.Generic;

namespace RootTools.Inspects
{
    class RecipeManager
    {
        int m_nCurrent = 0;

        List<Recipe> m_rcp = new List<Recipe>();

        public void Save()
        {

        }
        public void Read()
        {

        }
        public Recipe GetCurrent()
        {
            return m_rcp[m_nCurrent];
        }
    }
}
