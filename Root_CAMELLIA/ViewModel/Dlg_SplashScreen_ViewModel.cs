using RootTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA
{
    class Dlg_SplashScreen_ViewModel : ObservableObject
    {
        private string splashScreenText = "Initializing...";
        public string SplashScreenText
        {
            get { return splashScreenText; }
            set
            {
                SetProperty(ref splashScreenText, value);
            }
        }

        private int splashProgressValue = 0;
        public int SplashProgressValue
        {
            get
            {
                return splashProgressValue;
            }
            set
            {
                SetProperty(ref splashProgressValue, value);
            }
        }
    }
}
