using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Root_WindII
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    MainWindow window = new MainWindow();

        //    // Create the ViewModel to which 
        //    // the main window binds.

        //    var viewModel = new MainWindow_ViewModel();

        //    // When the ViewModel asks to be closed, 
        //    // close the window.
        //    //viewModel.RequestClose += delegate
        //    //{
        //    //    window.Close();
        //    //};

        //    // Allow all controls in the window to 
        //    // bind to the ViewModel by setting the 
        //    // DataContext, which propagates down 
        //    // the element tree.
        //    window.DataContext = viewModel;

        //    window.Show();
        //}
        private void Application_Startup(object sender, StartupEventArgs e)
        {

            //base.OnStartup(e);
            Thread thread = new Thread(
                new System.Threading.ThreadStart(
                    delegate ()
                    {
                        SplashScreenHelper.SplashScreen = new Dlg_SplashScreen(); //로딩창을 만든다.
                        SplashScreenHelper.Show(); // 만들어진 로딩창을 화면에 띄운다.
                        System.Windows.Threading.Dispatcher.Run(); //
                    }
                ));
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            Thread.Sleep(10);

            Task.Factory.StartNew(() =>
            {
                //simulate some work being done
                //System.Threading.Thread.Sleep(3000);

                //since we're not on the UI thread
                //once we're done we need to use the Dispatcher
                //to create and show the main window

                this.Dispatcher.Invoke
                (() =>
                {
                    //initialize the main window, set it as the application main window
                    //and close the splash screen
                    
                    MainWindow mainWindow = new MainWindow();
                    
                    var viewModel = new MainWindow_ViewModel(); //이코드가 끝나면 로딩창 사라짐
                    
                    mainWindow.DataContext = viewModel;
                    
                    this.MainWindow = mainWindow;
                                       
                    mainWindow.Show();
                    

                    //SplashScreenHelper.Hide();
                }
                );
            });

            //thread.Join();
        }
    }
}
