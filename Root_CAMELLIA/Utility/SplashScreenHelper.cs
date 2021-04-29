using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Root_CAMELLIA
{
    internal class SplashScreenHelper
    {
        public static Dlg_SplashScreen SplashScreen { get; set; }

        public static void Show()
        {
            if (SplashScreen != null)
                SplashScreen.Show();
        }

        public static void Hide()
        {
            if (SplashScreen == null) return;

            if (!SplashScreen.Dispatcher.CheckAccess())
            {
                Thread thread = new Thread(
                    new System.Threading.ThreadStart(
                        delegate ()
                        {
                            SplashScreen.Dispatcher.Invoke(
                                DispatcherPriority.Normal,
                                new Action(delegate ()
                                {
                                    Thread.Sleep(2000);
                                    SplashScreen.Hide();
                                }
                            ));
                        }
                ));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else
                SplashScreen.Hide();
        }

        public static void ShowText(string text)
        {
            if (SplashScreen == null) return;

            if (!SplashScreen.Dispatcher.CheckAccess())
            {
                Thread thread = new Thread(
                    new System.Threading.ThreadStart(
                        delegate ()
                        {
                            SplashScreen.Dispatcher.Invoke(
                                DispatcherPriority.Normal,

                                new Action(delegate ()
                                {
                                    ((Dlg_SplashScreen_ViewModel)SplashScreen.DataContext).SplashScreenText = text;
                                }
                            ));
                            SplashScreen.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                        }
                ));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else
                ((Dlg_SplashScreen_ViewModel)SplashScreen.DataContext).SplashScreenText = text;
        }

        public static void ShowProgress(int value)
        {
            if (SplashScreen == null) return;

            if (!SplashScreen.Dispatcher.CheckAccess())
            {
                Thread thread = new Thread(
                    new System.Threading.ThreadStart(
                        delegate ()
                        {
                            SplashScreen.Dispatcher.Invoke(
                                DispatcherPriority.Normal,

                                new Action(delegate ()
                                {
                                    ((Dlg_SplashScreen_ViewModel)SplashScreen.DataContext).SplashProgressValue = value;
                                }
                            ));
                            SplashScreen.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));
                        }
                ));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else
                ((Dlg_SplashScreen_ViewModel)SplashScreen.DataContext).SplashProgressValue = value;
        }
    }
}
