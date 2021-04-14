using RootTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA
{
    public static class BaseDefine
    {
        public const int ViewSize = 310;
        public const int WFRadius = 300;
        public const int MaxPoint = 50000;
        public const double CanvasWidth = 1000;
        public const double CanvasHeight = 1000;
        public const int ArrowLength = 15;

        public const string RegNanoViewConfig = "NanoViewConfigPath";
        public const string RegNanoViewPort = "NanoViewPort";
        public const string RegNanoViewExceptNIR = "ExceptNIR";
        public const string RegNanoViewUseThickness = "Use Thickness"; 
        public const string RegLightSourcePath = "LightSourcePath";
        public const string Dir_Preset = @"C:\Camellia\Preset\";
        public const string Dir_StageMap = @"C:\Camellia\StageMap\";
        public const string Dir_Recipe = @"C:\Camellia\Recipe\PRD\15Line\"; // 변경해야함
        public const string Dir_StageHole = @"C:\Camellia\StageCircleHole.txt"; //변경해야함
        public const string Dir_LockImg = "\\Resource\\locked.png";
        public const string Dir_MeasureSaveRootPath = @"C:\Camellia2\PRD\Data\";

        public const string TOOL_NAME = "Camellia2";

        private static ConfigModel _configModel = new ConfigModel();
        public static ConfigModel Configuration
        {
            get { return _configModel; }
            set { _configModel = value; }
        }

    }

    public class ConfigModel : ObservableObject
    {

        private string _Version = "1.0.0.0";
        public string Version
        {
            get 
            {
                string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
                _Version = fileVersion;
                return "Camellia Ⅱ - Version " + _Version;
            }
        }
    }
}
