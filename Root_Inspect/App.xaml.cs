using RootTools;
using System.Windows;

namespace Root_Inspect
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            EQ.m_sModel = "Root";
            if (e.Args.Length > 0) EQ.m_sModel += "." + e.Args[0];
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show(); 
        }
    }
}
