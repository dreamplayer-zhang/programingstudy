using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Root_VEGA_P_Vision
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public const string mPool = "Vision.Memory";
        public const string mGroup = "Vision";
        public const string mMaskLayer = "MaskLayer";
        public const string mMainGrab = "Main Grab";
        public const string mSideGrab = "Side Grab";
        public const string mStainGrab = "Stain Grab";
        public const string mZStack = "Z Stack Grab";
    }
}
