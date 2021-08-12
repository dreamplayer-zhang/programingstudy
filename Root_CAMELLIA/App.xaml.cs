using Root_CAMELLIA.LibSR_Met;
using SSLNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Root_CAMELLIA
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        public static CAMELLIA_Engineer m_engineer = new CAMELLIA_Engineer();
        public static Nanoview m_nanoView = new Nanoview();
        public static ConstValue m_constValue = new ConstValue();

        public App()

        {

            this.Dispatcher.UnhandledException += this.Dispatcher_UnhandledException;

            this.Dispatcher.UnhandledExceptionFilter += this.Dispatcher_UnhandledExceptionFilter;

        }



        private void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)

        {

            e.Handled = true;

        }



        private void Dispatcher_UnhandledExceptionFilter(object sender, System.Windows.Threading.DispatcherUnhandledExceptionFilterEventArgs e)

        {

            e.RequestCatch = true;

        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            //base.OnStartup(e);
            Thread thread = new Thread(
                new System.Threading.ThreadStart(
                    delegate ()
                    {
                        SplashScreenHelper.SplashScreen = new Dlg_SplashScreen();
                        SplashScreenHelper.Show();
                        System.Windows.Threading.Dispatcher.Run();
                    }
                ));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();


            Task.Factory.StartNew(() =>
            {

                //simulate some work being done
                //System.Threading.Thread.Sleep(3000);

                //since we're not on the UI thread
                //once we're done we need to use the Dispatcher
                //to create and show the main window
                this.Dispatcher.Invoke(() =>
                {
                    //initialize the main window, set it as the application main window
                    //and close the splash screen
                    var mainWindow = new MainWindow();
                    this.MainWindow = mainWindow;
                    mainWindow.Show();
                    SplashScreenHelper.Hide();
                });
            });


            //thread.Join();
        }
    }
}
