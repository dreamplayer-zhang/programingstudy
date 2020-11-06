using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Data
{
    public class RecipeDataManager
    {
        private DataManager dataManager { get; set; }
        public PresetData PresetData { get; set; }

        public RecipeData TeachingRD { get; set; }
        public RecipeDataManager(DataManager DM)
        {
            dataManager = DM;

            PresetData = new PresetData();
            TeachingRD = new RecipeData();
        }
    }
}
