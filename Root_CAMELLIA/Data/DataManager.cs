using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Data
{
    public class DataManager
    {
        public RecipeDataManager recipeDM { get; set; }
        
        public DataManager()
        {
            recipeDM = new RecipeDataManager(this);
        }
    }
}
