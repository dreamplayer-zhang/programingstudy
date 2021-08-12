using Root_WindII;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Root_WindII
{
    class SplashScreen_ViewModel : ObservableObject
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

        private Brush splashScreenBrush = Brushes.AliceBlue;
        public Brush SplashScreenBrush
        {
            get { return splashScreenBrush; }
            set
            {
                SetProperty(ref splashScreenBrush, value);
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
