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
        public WaferCentering m_waferCentering;
        public Calibration m_calibration;
        public SaveMeasureData m_SaveMeasureData;
        public MainWindow_ViewModel Main { get; set; }

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
        public DataManager(MainWindow_ViewModel main = null)
        {
            Main = main;
            recipeDM = new RecipeDataManager(this);
            m_waferCentering = new WaferCentering();
            m_calibration = new Calibration();
            m_SaveMeasureData = new SaveMeasureData();
        }
    }
}
