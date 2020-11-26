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
        public MainWindow Main { get; set; }

        private static DataManager instance;
        public static DataManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataManager();
                }
                return instance;
            }
            private set
            {
                instance = value;
            }
        }
        public DataManager(MainWindow main = null)
        {
            recipeDM = new RecipeDataManager(this);
        }
    }
}
