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
        public const string Dir_MeasureSaveRootPath = @"D:\Camellia2\PRD\Data\";
        public const string Dir_HistorySaveRootPath = @"D:\Camellia2\History\";

        public const string TOOL_NAME = "Camellia2";

        public const string USERNAME = "0a50ffc00ab87ec51cd41269a8820b1ba28c04fbbf51208803e073824b612fa3";
        public const string PASSWORD = "e664ff11431cd8eaf28ec5c5a43a4633e8c338306522d73efc9882bea7ee2640";



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
                return "Camellia2 - Version " + _Version;
            }
        }

        private bool _loginSuccess = false;
        public bool LoginSuccess
        {
            get
            {
                return _loginSuccess;
            }
            set
            {
                //_loginSuccess = value;
                SetProperty(ref _loginSuccess, value);
            }
        }
    }
}
