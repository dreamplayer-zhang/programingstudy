using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Met = LibSR_Met;

namespace Root_CAMELLIA
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public static CAMELLIA_Engineer m_engineer = new CAMELLIA_Engineer();
        public static Met.Nanoview m_nanoView = new Met.Nanoview();
    }
}
